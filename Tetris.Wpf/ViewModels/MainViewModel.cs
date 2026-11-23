using System.Windows.Input;
using System.Windows.Threading;
using Tetris.Core;

namespace Tetris.Wpf.ViewModels;

public sealed class MainViewModel : ObservableObject, IDisposable
{
    private const bool HARD_DROP_INSTANT_LOCK = true;

    private const int FrameMilliseconds = 16;           // ≈60 FPS
    private const int MoveRepeatMs = 80;                // скорость key-repeat при удержании
    private const int SoftDropRepeatMs = 40;            // софт-дроп быстрее

    private readonly DispatcherTimer _timer;
    private readonly GameEngine _engine;
    private int _score, _level, _lines, _lastCleared, _prevLinesCleared;
    private string _status = "Playing";
    private bool _showGhost = true;

    // ── Key repeat ───────────────────────────────────────────
    private int _leftRepeatAccum, _rightRepeatAccum, _downRepeatAccum;

    /// <summary>Событие для перерисовки поля.</summary>
    public event EventHandler? BoardChanged;

    public int LastCleared { get => _lastCleared; private set => SetProperty(ref _lastCleared, value); }

    public Board Board => _engine.Board;
    public Tetromino CurrentPiece => _engine.CurrentPiece;
    public TetrominoType NextPieceType => _engine.NextPieceType;
    public GameStatus GameStatus => _engine.Status;

    public int Score { get => _score; private set => SetProperty(ref _score, value); }
    public int Level { get => _level; private set => SetProperty(ref _level, value); }
    public int Lines { get => _lines; private set => SetProperty(ref _lines, value); }
    public string Status { get => _status; private set => SetProperty(ref _status, value); }

    public void Start() => _timer.Start();
    public void Stop() => _timer.Stop();
    public void TogglePause() => _timer.IsEnabled = !_timer.IsEnabled;
    public bool IsPaused => !_timer.IsEnabled;
    public void ToggleGhost() => ShowGhost = !ShowGhost;
    public int GetGhostY() => _engine.GetGhostY();
    public int TickMilliseconds => LevelSpeed.IntervalMilliseconds(Level);

    public bool ShowGhost
    {
        get => _showGhost;
        set
        {
            if (SetProperty(ref _showGhost, value))
            {
                Redraw();
            }
        }
    }

    public MainViewModel(bool hardDropInstantLock = false)
    {
        _engine = new GameEngine(new Board(10, 20), new RandomPieceProvider())
        {
            HardDropLockDelayMilliseconds = HARD_DROP_INSTANT_LOCK || hardDropInstantLock ? 0 : -1
        };

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(FrameMilliseconds) };
        _timer.Tick += OnTick;
        SyncStats();
    }

    public void SetStatusText(string text = "new text")
    {
        Status = text;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        // 1. Обработать удержание клавиш
        HandleKeyRepeat(FrameMilliseconds);

        // 2. Шаг движка
        _engine.Tick(FrameMilliseconds);

        // 3. Обновить UI
        SyncStats();
        Redraw();

        if (_engine.Status == GameStatus.GameOver)
        {
            _timer.Stop();
            Status = "Game Over";
        }
    }

    private void HandleKeyRepeat(int deltaMs)
    {
        if (GameStatus != GameStatus.Playing) return;

        // Левая / правая
        if (Keyboard.IsKeyDown(Key.Left))
        {
            _leftRepeatAccum += deltaMs;
            if (_leftRepeatAccum >= MoveRepeatMs)
            {
                _leftRepeatAccum = 0;
                if (_engine.MoveLeft()) Redraw();
            }
        }
        else _leftRepeatAccum = 0;

        if (Keyboard.IsKeyDown(Key.Right))
        {
            _rightRepeatAccum += deltaMs;
            if (_rightRepeatAccum >= MoveRepeatMs)
            {
                _rightRepeatAccum = 0;
                if (_engine.MoveRight()) Redraw();
            }
        }
        else _rightRepeatAccum = 0;

        // Вниз — ускоряем падение
        if (Keyboard.IsKeyDown(Key.Down))
        {
            _downRepeatAccum += deltaMs;
            if (_downRepeatAccum >= SoftDropRepeatMs)
            {
                _downRepeatAccum = 0;
                if (_engine.MoveDown()) Redraw();
            }
        }
        else _downRepeatAccum = 0;
    }

    public void Step(int deltaMs)
    {
        _engine.Tick(deltaMs);
        SyncStats();
        Redraw();
        if (_engine.Status == GameStatus.GameOver)
        {
            _timer.Stop();
            Status = "Game Over";
        }
    }

    public void MoveLeft() { if (_engine.MoveLeft()) Redraw(); }
    public void MoveRight() { if (_engine.MoveRight()) Redraw(); }
    public void MoveDown() { if (_engine.MoveDown()) Redraw(); }
    public void Rotate() { _engine.Rotate(); Redraw(); }

    // ── Немедленные действия на KeyDown (без задержки) ──────────
    public void MoveLeftNow() { if (_engine.MoveLeft()) Redraw(); _leftRepeatAccum = 0; }
    public void MoveRightNow() { if (_engine.MoveRight()) Redraw(); _rightRepeatAccum = 0; }
    public void MoveDownNow() { if (_engine.MoveDown()) Redraw(); _downRepeatAccum = 0; }

    public void HardDrop()
    {
        _engine.HardDrop();
        SyncStats();

        Redraw();

        if (_engine.Status == GameStatus.GameOver)
        {
            _timer.Stop();
            Status = "Game Over";
        }
    }

    private void Redraw() => 
        BoardChanged?.Invoke(this, EventArgs.Empty);

    private void SyncStats()
    {
        int previousLevel = Level;
        Score = _engine.Score;
        Level = _engine.Level;
        Lines = _engine.LinesCleared;

        //LastCleared = Lines - _prevLinesCleared;
        //_prevLinesCleared = Lines;

        LastCleared = 0;   // сбрасываем баннер

        if (Level != previousLevel)
        {
            OnPropertyChanged(nameof(TickMilliseconds));
        }
    }

    public void Dispose() => 
        _timer.Stop();
}
