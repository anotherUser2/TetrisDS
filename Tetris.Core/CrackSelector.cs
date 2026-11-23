namespace Tetris.Core;

public static class CrackSelector
{
    public static IReadOnlyList<(int X, int Y)> SelectCrackedCells(
        IReadOnlyList<(int X, int Y)> pieceCells, Board board)
    {
        var own = pieceCells.ToHashSet();
        var cracked = new HashSet<(int X, int Y)>();
        foreach (var (x, y) in pieceCells)
            foreach (var (nx, ny) in new[] { (x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1) })
            {
                if (nx < 0 || nx >= board.Width || ny < 0 || ny >= board.Height) continue;
                if (own.Contains((nx, ny))) continue;
                if (!board.IsOccupied(nx, ny)) continue;
                cracked.Add((nx, ny));
            }
        return cracked.ToList();
    }
}