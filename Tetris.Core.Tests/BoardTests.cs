namespace Tetris.Core.Tests;

public class BoardTests
{
    [Fact]
    public void NewBoard_IsEmpty()
    {
        var board = new Board(10, 20);
        Assert.True(board.IsEmpty(0, 0));
        Assert.True(board.IsEmpty(9, 19));
    }

    [Fact]
    public void CanPlace_WithinBounds_ReturnsTrue()
    {
        var board = new Board(10, 20);
        var piece = new Tetromino(TetrominoType.O, 0, 0);
        Assert.True(board.CanPlace(piece));
    }

    [Fact]
    public void CanPlace_OutOfBoundsLeft_ReturnsFalse()
    {
        var board = new Board(10, 20);
        var piece = new Tetromino(TetrominoType.O, -1, 0);
        Assert.False(board.CanPlace(piece));
    }

    [Fact]
    public void CanPlace_OutOfBoundsBottom_ReturnsFalse()
    {
        var board = new Board(10, 20);
        var piece = new Tetromino(TetrominoType.O, 0, 20);
        Assert.False(board.CanPlace(piece));
    }

    [Fact]
    public void Lock_OccupiesCells()
    {
        var board = new Board(10, 20);
        var piece = new Tetromino(TetrominoType.O, 0, 0);
        board.Lock(piece);
        Assert.True(board.IsOccupied(0, 0));
        Assert.True(board.IsOccupied(1, 0));
        Assert.True(board.IsOccupied(0, 1));
        Assert.True(board.IsOccupied(1, 1));
    }

    [Fact]
    public void ClearFullLines_NoFullLines_ReturnsZero()
    {
        var board = new Board(3, 3);
        Assert.Equal(0, board.ClearFullLines());
    }

    [Fact]
    public void ClearFullLines_OneFullLine_ClearsAndShiftsDown()
    {
        var board = new Board(3, 3);
        // Заполняем нижний ряд полностью
        var piece = new Tetromino(TetrominoType.I, 0, 2); // горизонтальная линия из 4, но доска 3
        // Проще заполнить вручную через Lock нескольких O
        var p1 = new Tetromino(TetrominoType.O, 0, 1);
        board.Lock(p1); // занимает (0,1)(1,1)(0,2)(1,2)
        var p2 = new Tetromino(TetrominoType.O, 2, 1);
        board.Lock(p2); // занимает (2,1)(3,1)(2,2)(3,2) — но ширина 3, значит (2,1)(2,2)

        // Нижний ряд (y=2) заполнен: (0,2)(1,2)(2,2)
        // Средний ряд (y=1) заполнен: (0,1)(1,1)(2,1)
        // Верхний (y=0) пуст
        int cleared = board.ClearFullLines();
        Assert.Equal(2, cleared);
        Assert.True(board.IsEmpty(0, 0));
        Assert.True(board.IsEmpty(1, 0));
        Assert.True(board.IsEmpty(2, 0));
    }

    [Fact]
    public void IsRowFull_ReturnsCorrectly()
    {
        var board = new Board(2, 2);
        Assert.False(board.IsRowFull(0));
        var piece = new Tetromino(TetrominoType.O, 0, 0);
        board.Lock(piece);
        Assert.True(board.IsRowFull(0));
        Assert.True(board.IsRowFull(1));
    }

    [Fact]
    public void GetGhostY_OneAboveBlocker_ReturnsSameY()
    {
        var board = new Board(width: 10, height: 4);
        board.Lock(new Tetromino(TetrominoType.O, 3, 2));
        var engine = new GameEngine(board, new FixedPieceProvider(TetrominoType.O));

        // теперь фигура на Y=0, блоки на Y=2..3
        while (engine.MoveDown()) { }
        
        int y = engine.CurrentPiece.Y;
        Assert.Equal(y, engine.GetGhostY());
    }
}
