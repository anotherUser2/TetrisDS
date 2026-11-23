using System.Windows;
using System.Windows.Media;

namespace Tetris.Wpf.Effects;

public sealed class FlashEffect : TimedBoardEffect
{
    private readonly IReadOnlyList<(int X, int Y)> _cells;
    public FlashEffect(IReadOnlyList<(int X, int Y)> cells, int durationMs = 400)
        : base(durationMs) { _cells = cells; }

    public override void Render(DrawingContext dc, double cellSize, int boardWidth, int boardHeight)
    {
        var brush = new SolidColorBrush(Color.FromArgb((byte)(120 * Alpha), 0xFF, 0xFF, 0xFF));
        brush.Freeze();
        foreach (var (x, y) in _cells)
        {
            if (x < 0 || x >= boardWidth || y < 0 || y >= boardHeight) continue;
            dc.DrawRectangle(brush, null,
                new Rect(x * cellSize + 1, y * cellSize + 1, cellSize - 2, cellSize - 2));
        }
    }
}