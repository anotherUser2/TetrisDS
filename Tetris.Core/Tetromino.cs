namespace Tetris.Core;

public enum TetrominoType { I, O, T, S, Z, J, L }

public sealed class Tetromino
{
    public TetrominoType Type { get; }
    public int[,] Shape { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    private static readonly Dictionary<TetrominoType, int[,]> BaseShapes = new()
    {
        [TetrominoType.I] = new int[,] { { 1, 1, 1, 1 } },
        [TetrominoType.O] = new int[,] { { 1, 1 }, { 1, 1 } },
        [TetrominoType.T] = new int[,] { { 0, 1, 0 }, { 1, 1, 1 } },
        [TetrominoType.S] = new int[,] { { 0, 1, 1 }, { 1, 1, 0 } },
        [TetrominoType.Z] = new int[,] { { 1, 1, 0 }, { 0, 1, 1 } },
        [TetrominoType.J] = new int[,] { { 1, 0, 0 }, { 1, 1, 1 } },
        [TetrominoType.L] = new int[,] { { 0, 0, 1 }, { 1, 1, 1 } }
    };

    public Tetromino(TetrominoType type, int startX = 3, int startY = 0)
    {
        Type = type;
        Shape = (int[,])BaseShapes[type].Clone();
        X = startX;
        Y = startY;
    }

    public void Rotate()
    {
        int rows = Shape.GetLength(0);
        int cols = Shape.GetLength(1);
        var rotated = new int[cols, rows];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                rotated[c, rows - 1 - r] = Shape[r, c];
            }
        }

        Shape = rotated;
    }

    public IEnumerable<(int X, int Y)> GetAbsoluteCells()
    {
        int rows = Shape.GetLength(0);
        int cols = Shape.GetLength(1);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (Shape[r, c] == 1)
                {
                    yield return (X + c, Y + r);
                }
            }
        }
    }
}
