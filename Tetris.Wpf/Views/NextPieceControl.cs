using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Tetris.Core;
using Tetris.Wpf.ViewModels;

namespace Tetris.Wpf.Views;

public sealed class NextPieceControl : FrameworkElement
{
    private MainViewModel? _vm;
    private const int CellSize = 22;

    public void Bind(MainViewModel vm)
    {
        if (_vm is not null)
        {
            _vm.BoardChanged -= OnChanged;
        }

        _vm = vm;
        _vm.BoardChanged += OnChanged;
        InvalidateVisual();
    }

    private void OnChanged(object? sender, EventArgs e)
    {
        if (Dispatcher.CheckAccess())
            InvalidateVisual();
        else
            Dispatcher.BeginInvoke(DispatcherPriority.Render, InvalidateVisual);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        var brush = new SolidColorBrush(Color.FromRgb(0x1a, 0x1f, 0x2b));

        // Фон превью
        dc.DrawRectangle(brush, null, new Rect(0, 0, ActualWidth, ActualHeight));

        if (_vm is null)
        {
            return;
        }

        var type = _vm.NextPieceType;
        var piece = new Tetromino(type, 0, 0);

        int rows = piece.Shape.GetLength(0);
        int cols = piece.Shape.GetLength(1);
        double pieceW = cols * CellSize;
        double pieceH = rows * CellSize;
        double offsetX = (ActualWidth - pieceW) / 2.0;
        double offsetY = (ActualHeight - pieceH) / 2.0;

        var color = TetrominoColors.For(type);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (piece.Shape[r, c] == 1)
                {
                    DrawCell(dc, offsetX + c * CellSize, offsetY + r * CellSize, color);
                }
            }
        }
    }

    private static void DrawCell(DrawingContext dc, double x, double y, Color color)
    {
        var rect = new Rect(x + 1, y + 1, CellSize - 2, CellSize - 2);
        dc.DrawRectangle(new SolidColorBrush(color), null, rect);

        var brush = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255));
        dc.DrawRectangle(brush, null, new Rect(rect.X, rect.Y, rect.Width, 3));
    }
}
