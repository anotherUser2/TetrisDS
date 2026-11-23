using Tetris.Core;
using static Tetris.Core.GameEngine;

namespace Tetris.Wpf.Effects.Factories;

public static class CrackEffectFactory
{
    /// <summary>
    /// Трещины на соседних клетках с приземлившейся фигурой (по 4-связности),
    /// которые заняты, но не принадлежат самой фигуре.
    /// </summary>
    public static IBoardEffect? Create(HardDropInfo info, Board board)
    {
        var own = new HashSet<(int X, int Y)>(info.Cells);
        var cracked = new HashSet<(int X, int Y)>();

        foreach (var (x, y) in info.Cells)
        {
            foreach (var (nx, ny) in new[] { (x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1) })
            {
                if (nx < 0 || nx >= board.Width || ny < 0 || ny >= board.Height) continue;
                if (own.Contains((nx, ny))) continue;
                if (!board.IsOccupied(nx, ny)) continue;
                cracked.Add((nx, ny));
            }
        }

        return cracked.Count == 0 ? null : new CrackEffect(cracked.ToList());
    }
}