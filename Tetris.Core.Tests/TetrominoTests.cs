namespace Tetris.Core.Tests;

public class TetrominoTests
{
    [Fact]
    public void GetAbsoluteCells_ReturnsCorrectCount()
    {
        var piece = new Tetromino(TetrominoType.I);
        Assert.Equal(4, piece.GetAbsoluteCells().Count());
    }

    [Fact]
    public void GetAbsoluteCells_UsesPositionOffset()
    {
        var piece = new Tetromino(TetrominoType.O, 5, 10);
        var cells = piece.GetAbsoluteCells().ToList();
        Assert.Contains((5, 10), cells);
        Assert.Contains((6, 10), cells);
        Assert.Contains((5, 11), cells);
        Assert.Contains((6, 11), cells);
    }

    [Fact]
    public void Rotate_I_BecomesVertical()
    {
        var piece = new Tetromino(TetrominoType.I, 0, 0);
        Assert.Equal(1, piece.Shape.GetLength(0)); // 1 строка
        Assert.Equal(4, piece.Shape.GetLength(1)); // 4 столбца
        piece.Rotate();
        Assert.Equal(4, piece.Shape.GetLength(0));
        Assert.Equal(1, piece.Shape.GetLength(1));
    }

    [Fact]
    public void Rotate_O_Remains2x2()
    {
        var piece = new Tetromino(TetrominoType.O, 0, 0);
        piece.Rotate();
        Assert.Equal(2, piece.Shape.GetLength(0));
        Assert.Equal(2, piece.Shape.GetLength(1));
    }

    [Fact]
    public void Rotate_T_ChangesShape()
    {
        var piece = new Tetromino(TetrominoType.T, 0, 0);
        var before = (int[,])piece.Shape.Clone();
        piece.Rotate();
        Assert.NotEqual(before, piece.Shape);
    }
}
