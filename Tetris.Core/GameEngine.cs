namespace Tetris.Core;

public enum GameStatus { Playing, GameOver }

public sealed class GameEngine
{
    private readonly Board _board;
    private readonly IPieceProvider _pieceProvider;
    private Tetromino _currentPiece = null!;

    private int _lockDelayTicks;
    private int _lockResets;

    public int Score { get; private set; }
    public int LinesCleared { get; private set; }
    public int Level { get; private set; } = 1;
    public GameStatus Status { get; private set; } = GameStatus.Playing;
    public Tetromino CurrentPiece => _currentPiece;
    public TetrominoType NextPieceType => _pieceProvider.Peek();
    public Board Board => _board;

    public GameEngine(Board board, IPieceProvider pieceProvider)
    {
        _board = board;
        _pieceProvider = pieceProvider;
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

    public void SpawnNewPiece()
    {
        _currentPiece = _pieceProvider.Next();

        if (!_board.CanPlace(_currentPiece))
        {
            Status = GameStatus.GameOver;
        }
    }

    public bool MoveLeft()
    {
        if (Status != GameStatus.Playing)
        {
            return false;
        }

        _currentPiece.X--;
        
        if (!_board.CanPlace(_currentPiece))
        {
            _currentPiece.X++;
            return false;
        }

        return true;
    }

    public bool MoveRight()
    {
        if (Status != GameStatus.Playing)
        {
            return false;
        }

        _currentPiece.X++;
        
        if (!_board.CanPlace(_currentPiece))
        {
            _currentPiece.X--;
            return false;
        }

        return true;
    }

    public bool MoveDown()
    {
        if (Status != GameStatus.Playing)
        {
            return false;
        }

        _currentPiece.Y++;
        
        if (!_board.CanPlace(_currentPiece))
        {
            _currentPiece.Y--;
            return false;
        }

        return true;
    }

    public void Rotate()
    {
        if (Status != GameStatus.Playing)
        {
            return;
        }

        var originalShape = (int[,])_currentPiece.Shape.Clone();
        _currentPiece.Rotate();

        if (!_board.CanPlace(_currentPiece))
        {
            _currentPiece.Shape = originalShape;
        }
    }

    public void HardDrop()
    {
        while (MoveDown()) { }
        LockAndClear();
    }

    public bool Tick()
    {
        if (Status != GameStatus.Playing)
        {
            return false;
        }

        if (MoveDown())
        {
            return false;
        }

        Thread.Sleep(500);

        LockAndClear();
        return true;
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

    public void ForceGameOver()
    {
        while (Status == GameStatus.Playing)
        {
            if (!MoveDown()) LockAndClear();
        }
    }
}
