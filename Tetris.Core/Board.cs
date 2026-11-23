namespace Tetris.Core;

public sealed class Board
{
    public int Width { get; }
    public int Height { get; }
    private readonly int[,] _grid;

    public Board(int width = 10, int height = 20)
    {
        Width = width;
        Height = height;
        _grid = new int[height, width];
    }

    public bool IsEmpty(int x, int y) =>
        x >= 0 && 
        x < Width && 
        y >= 0 && 
        y < Height && 
        _grid[y, x] == 0;

    public bool IsOccupied(int x, int y) => 
        !IsEmpty(x, y) && 
        x >= 0 && 
        x < Width && 
        y >= 0 && 
        y < Height;

    public bool CanPlace(Tetromino piece)
    {
        foreach (var (x, y) in piece.GetAbsoluteCells())
        {
            if (x < 0 || 
                x >= Width || 
                y >= Height)
            {
                return false;
            }

            if (y >= 0 && 
                _grid[y, x] != 0)
            {
                return false;
            }
        }

        return true;
    }

    public void Lock(Tetromino piece)
    {
        foreach (var (x, y) in piece.GetAbsoluteCells())
        {
            if (y >= 0 && 
                y < Height && 
                x >= 0 && 
                x < Width)
            {
                _grid[y, x] = 1;
            }
        }
    }

    public int ClearFullLines()
    {
        int cleared = 0;

        for (int row = Height - 1; row >= 0; row--)
        {
            bool full = true;

            for (int col = 0; col < Width; col++)
            {
                if (_grid[row, col] == 0) 
                { 
                    full = false; 
                    break; 
                }
            }

            if (!full)
            {
                continue;
            }
            
            cleared++;

            for (int r = row; r > 0; r--)
            {
                for (int c = 0; c < Width; c++)
                {
                    _grid[r, c] = _grid[r - 1, c];
                }
            }

            for (int c = 0; c < Width; c++)
            {
                _grid[0, c] = 0;
            }

            row++;
        }

        return cleared;
    }

    public int this[int x, int y] => 
        _grid[y, x];

    public bool IsRowFull(int row)
    {
        for (int col = 0; col < Width; col++)
        {
            if (_grid[row, col] == 0)
            {
                return false;
            }
        }

        return true;
    }
}
