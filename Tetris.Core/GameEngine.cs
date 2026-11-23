namespace Tetris.Core;

public enum GameStatus { Playing, GameOver }

public sealed class GameEngine
{
    private readonly Board _board;
    private readonly IPieceProvider _pieceProvider;
    private Tetromino _currentPiece = null!;

    // ── Fall accumulator ───────────────────────────────────────
    /// <summary>Накопленное время с последнего шага падения.</summary>
    private int _fallAccumulatedMs;

    /// <summary>Текущий интервал падения (мс). Меняется по уровню.</summary>
    public int FallIntervalMilliseconds => LevelSpeed.IntervalMilliseconds(Level);

    // ── Lock delay ─────────────────────────────────────────────
    /// <summary>Сколько миллисекунд фигура может «висеть» на земле перед фиксацией.</summary>
    public int LockDelayMilliseconds { get; init; } = 500;
    /// <summary>Сколько раз сдвиг/поворот может сбросить таймер фиксации.</summary>
    public int MaxLockResets { get; init; } = 15;

    private int _lockDelayAccumulatedMs;
    private int _lockResetsUsed;

    /// <summary>Накопленное «время на земле» — для UI/тестов.</summary>
    public int LockDelayAccumulatedMs => _lockDelayAccumulatedMs;

    /// <summary>Текущая фигура уже касается земли и находится в окне lock delay.</summary>
    public bool IsGrounded => !_board.CanPlaceBelow(_currentPiece);

    public int Score { get; private set; }
    public int LinesCleared { get; private set; }
    public int Level { get; private set; } = 1;
    public GameStatus Status { get; private set; } = GameStatus.Playing;
    public Tetromino CurrentPiece => _currentPiece;
    public Board Board => _board;
    public TetrominoType NextPieceType => _pieceProvider.Peek();

    /// <summary>
    /// Задержка фиксации после HardDrop.
    /// 0 = фиксировать мгновенно (классическое поведение).
    /// По умолчанию равна <see cref="LockDelayMilliseconds"/>.
    /// </summary>
    private int? _hardDropLockDelayOverride;
    public int HardDropLockDelayMilliseconds
    {
        get => _hardDropLockDelayOverride ?? LockDelayMilliseconds;
        init => _hardDropLockDelayOverride = value;
    }

    public GameEngine(Board board, IPieceProvider pieceProvider)
    {
        _board = board;
        _pieceProvider = pieceProvider;
        SpawnNewPiece();
    }

    public void SpawnNewPiece()
    {
        _currentPiece = _pieceProvider.Next();
        _lockDelayAccumulatedMs = 0;
        _lockResetsUsed = 0;
        _fallAccumulatedMs = 0;         // ← добавить
        if (!_board.CanPlace(_currentPiece))
            Status = GameStatus.GameOver;
    }

    public bool MoveLeft()
    {
        if (Status != GameStatus.Playing) return false;
        _currentPiece.X--;
        if (!_board.CanPlace(_currentPiece))
        {
            _currentPiece.X++;
            return false;
        }
        OnSuccessfulMove();
        return true;
    }

    public bool MoveRight()
    {
        if (Status != GameStatus.Playing) return false;
        _currentPiece.X++;
        if (!_board.CanPlace(_currentPiece))
        {
            _currentPiece.X--;
            return false;
        }
        OnSuccessfulMove();
        return true;
    }

    public bool MoveDown()
    {
        if (Status != GameStatus.Playing) return false;
        _currentPiece.Y++;

        if (!_board.CanPlace(_currentPiece))
        {
            _currentPiece.Y--;
            return false;
        }

        // Успешное падение сбрасывает накопленное время на земле
        _lockDelayAccumulatedMs = 0;
        return true;
    }

    public void Rotate()
    {
        if (Status != GameStatus.Playing) return;
        var original = (int[,])_currentPiece.Shape.Clone();
        _currentPiece.Rotate();
        if (!_board.CanPlace(_currentPiece))
        {
            _currentPiece.Shape = original;
            return;
        }
        OnSuccessfulMove();
    }

    /// <summary>
    /// Сбрасывает фигуру до позиции приземления.
    /// Если <see cref="HardDropLockDelayMilliseconds"/> > 0 — фиксация откладывается
    /// на это время, как при обычном касании (с учётом MaxLockResets).
    /// Если 0 — фиксирует мгновенно.
    /// </summary>
    public void HardDrop()
    {
        if (Status != GameStatus.Playing) return;

        // Телепорт до позиции приземления (без MoveDown-цикла, чтобы не трогать lock delay)
        int ghostY = GetGhostY();
        _currentPiece.Y = ghostY;

        _fallAccumulatedMs = 0;

        if (HardDropLockDelayMilliseconds == 0)
        {
            LockAndClear();
            return;
        }

        // Начинаем lock delay с нуля — как при обычном касании.
        // Дальнейшее накопление времени происходит в Tick().
        _lockDelayAccumulatedMs = 0;
        // _lockResetsUsed НЕ сбрасываем — лимит общий на фигуру.
        // (Если хочется отдельный лимит для HardDrop — заведите отдельный счётчик,
        // но Guideline-поведение — общий.)
    }

    /// <summary>
    /// Шаг симуляции на <paramref name="deltaMs"/> миллисекунд.
    /// Возвращает true, если фигура была зафиксирована на этом шаге.
    /// </summary>
    public bool Tick(int deltaMs)
    {
        if (Status != GameStatus.Playing) return false;
        if (deltaMs < 0) deltaMs = 0;

        // ── 1. Накопить время падения и, если пора, опустить фигуру ──
        _fallAccumulatedMs += deltaMs;
        int fallInterval = FallIntervalMilliseconds;

        while (_fallAccumulatedMs >= fallInterval)
        {
            _fallAccumulatedMs -= fallInterval;
            if (MoveDown())
            {
                // Успешное падение — сбрасываем lock delay
                // (это уже есть в MoveDown)
                continue;
            }

            // Не смогли упасть — фигура на земле.
            // Не тратим оставшееся «время падения» впустую — выходим из while,
            // lock delay начнёт копиться ниже.
            _fallAccumulatedMs = 0;
            break;
        }

        // ── 2. Lock delay — только если фигура на земле ─────────────
        if (!IsGrounded)
            return false;

        _lockDelayAccumulatedMs += deltaMs;
        if (_lockDelayAccumulatedMs < LockDelayMilliseconds)
            return false;

        LockAndClear();
        return true;
    }

    /// <summary>
    /// Успешный сдвиг/поворот: если фигура на земле — сбросить таймер фиксации
    /// (в пределах MaxLockResets). В воздухе — ничего не делаем.
    /// </summary>
    private void OnSuccessfulMove()
    {
        if (!IsGrounded) return;
        if (_lockResetsUsed >= MaxLockResets) return;

        _lockDelayAccumulatedMs = 0;
        _lockResetsUsed++;
    }

    private void LockAndClear()
    {
        _board.Lock(_currentPiece);
        int cleared = _board.ClearFullLines();
        if (cleared > 0)
        {
            LinesCleared += cleared;
            Score += Scoring.ForLinesCleared(cleared, Level);
            Level = LinesCleared / 10 + 1;
        }
        SpawnNewPiece();
    }

    public int GetGhostY()
    {
        var original = _currentPiece.Y;
        int dropY = original;
        while (true)
        {
            _currentPiece.Y = dropY + 1;
            if (!_board.CanPlace(_currentPiece))
            {
                _currentPiece.Y = original;
                return dropY;
            }
            dropY++;
        }
    }

    public void ForceGameOver()
    {
        while (Status == GameStatus.Playing)
        {
            if (!MoveDown()) LockAndClear();
        }
    }
}