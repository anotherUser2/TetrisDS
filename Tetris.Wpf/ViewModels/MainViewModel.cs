using System.Windows.Threading;
using Tetris.Core;

namespace Tetris.Wpf.ViewModels;

public sealed class MainViewModel : ObservableObject, IDisposable
{
    private readonly DispatcherTimer _timer;
    private readonly GameEngine _engine;
    private int _score;
    private int _level;
    private int _lines;
    private string _status = "Playing";
    private bool _showGhost = true;

    private int _prevLinesCleared;
    private int _lastCleared;
    public int LastCleared { get => _lastCleared; private set => SetProperty(ref _lastCleared, value); }

    /// <summary>Событие для перерисовки поля.</summary>
    public event EventHandler? BoardChanged;

    public Board Board => _engine.Board;
    public Tetromino CurrentPiece => _engine.CurrentPiece;
    public TetrominoType NextPieceType => _engine.NextPieceType;
    public GameStatus GameStatus => _engine.Status;

    public int Score { get => _score; private set => SetProperty(ref _score, value); }
    public int Level { get => _level; private set => SetProperty(ref _level, value); }
    public int Lines { get => _lines; private set => SetProperty(ref _lines, value); }
    public string Status { get => _status; private set => SetProperty(ref _status, value); }

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

    public MainViewModel()
    {
        _engine = new GameEngine(new Board(10, 20), new RandomPieceProvider());
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _timer.Tick += OnTick;
        SyncStats();
    }

    public void SetStatusText(string text = "new text")
    {
        Status = text;
    }

    public void Start() => _timer.Start();
    public void Stop() => _timer.Stop();
    public void TogglePause() => _timer.IsEnabled = !_timer.IsEnabled;
    public bool IsPaused => !_timer.IsEnabled;
    public void ToggleGhost() => ShowGhost = !ShowGhost;
    public int GetGhostY() => _engine.GetGhostY();
    public int TickMilliseconds => LevelSpeed.IntervalMilliseconds(Level);

    private void OnTick(object? sender, EventArgs e) => Step();

    public void Step()
    {
        _engine.Tick();
        SyncStats();

        //int before = _engine.LinesCleared;
        //LastCleared = _engine.LinesCleared - before;

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

    public void HardDrop()
    {
        _engine.HardDrop();
        SyncStats();

        //int before = _engine.LinesCleared;
        //LastCleared = _engine.LinesCleared - before;

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

        LastCleared = Lines - _prevLinesCleared;
        _prevLinesCleared = Lines;

        if (Level != previousLevel)
        {
            _timer.Interval = TimeSpan.FromMilliseconds(LevelSpeed.IntervalMilliseconds(Level));
            OnPropertyChanged(nameof(TickMilliseconds));
        }
    }

    public void Dispose() => 
        _timer.Stop();
}
