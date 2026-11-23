using System.Windows;
using System.Windows.Media;

namespace Tetris.Wpf.Effects;

/// <summary>
/// Трещины на клетках вокруг точки приземления HardDrop.
/// Рисуется как короткие ломаные линии внутри соседних клеток.
/// </summary>
public sealed class CrackEffect : TimedBoardEffect
{
    private readonly IReadOnlyList<(int X, int Y)> _crackCells;
    private readonly Random _rng = new();

    public CrackEffect(IReadOnlyList<(int X, int Y)> crackedCells, int durationMs = 2000)
        : base(durationMs)
    {
        _crackCells = crackedCells;
    }

    public override void Render(DrawingContext dc, double cellSize, int boardWidth, int boardHeight)
    {
        var brush = new SolidColorBrush(Color.FromArgb((byte)(200 * Alpha), 0xFF, 0x10, 0x10));
        brush.Freeze();
        var pen = new Pen(brush, 1.5);
        pen.Freeze();

        foreach (var (x, y) in _crackCells)
        {
            if (x < 0 || x >= boardWidth || y < 0 || y >= boardHeight) continue;
            double px = x * cellSize;
            double py = y * cellSize;

            // Детерминированный «рисунок» на основе координат — чтобы не мигал
            var local = new Random(HashCode.Combine(x, y));

            // 2–3 короткие ломаные линии внутри клетки
            int cracks = 2 + local.Next(2);
            for (int i = 0; i < cracks; i++)
            {
                double sx = px + local.NextDouble() * cellSize;
                double sy = py + local.NextDouble() * cellSize;
                double len1 = cellSize * 0.25 + local.NextDouble() * cellSize * 0.15;
                double ang1 = local.NextDouble() * Math.PI * 2;
                double ex = sx + Math.Cos(ang1) * len1;
                double ey = sy + Math.Sin(ang1) * len1;

                double ang2 = ang1 + (local.NextDouble() - 0.5) * 1.2;
                double len2 = len1 * 0.7;
                double fx = ex + Math.Cos(ang2) * len2;
                double fy = ey + Math.Sin(ang2) * len2;

                var geo = new StreamGeometry();
                using (var g = geo.Open())
                {
                    g.BeginFigure(new Point(sx, sy), false, false);
                    g.LineTo(new Point(ex, ey), true, false);
                    g.LineTo(new Point(fx, fy), true, false);
                }
                geo.Freeze();
                dc.DrawGeometry(null, pen, geo);
            }
        }
    }
}