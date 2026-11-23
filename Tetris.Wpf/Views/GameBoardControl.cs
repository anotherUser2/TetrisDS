using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Tetris.Core;
using Tetris.Wpf.ViewModels;

namespace Tetris.Wpf.Views;

public sealed class GameBoardControl : FrameworkElement
{
    private MainViewModel? _vm;
    private const int CellSize = 30;

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

        dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(0x1a, 0x1f, 0x2b)), null,
            new Rect(0, 0, ActualWidth, ActualHeight));

        if (_vm is null) return;
        var board = _vm.Board;

        // Сетка
        var gridPen = new Pen(new SolidColorBrush(Color.FromRgb(0x25, 0x2c, 0x3a)), 1);
        for (int x = 0; x <= board.Width; x++)
            dc.DrawLine(gridPen, new Point(x * CellSize, 0),
                new Point(x * CellSize, board.Height * CellSize));
        for (int y = 0; y <= board.Height; y++)
            dc.DrawLine(gridPen, new Point(0, y * CellSize),
                new Point(board.Width * CellSize, y * CellSize));

        // Зафиксированные блоки
        for (int y = 0; y < board.Height; y++)
            for (int x = 0; x < board.Width; x++)
                if (board.IsOccupied(x, y))
                    DrawCell(dc, x, y, Color.FromRgb(0x5c, 0x8f, 0xbf));

        if (_vm.GameStatus == GameStatus.Playing)
        {
            var color = TetrominoColors.For(_vm.CurrentPiece.Type);

            // ── Тень (ghost) ────────────────────────────────────
            if (_vm.ShowGhost)
            {
                int ghostY = _vm.GetGhostY();
                int deltaY = ghostY - _vm.CurrentPiece.Y;

                // Если фигура уже на месте приземления — тень не нужна
                if (deltaY > 0)
                {
                    var ghostColor = Color.FromArgb(90, color.R, color.G, color.B);
                    var ghostPen = new Pen(new SolidColorBrush(ghostColor), 2);

                    foreach (var (x, y) in _vm.CurrentPiece.GetAbsoluteCells())
                    {
                        int gy = y + deltaY;
                        if (gy < 0 || gy >= board.Height) continue;
                        var rect = new Rect(x * CellSize + 2, gy * CellSize + 2,
                                            CellSize - 4, CellSize - 4);
                        dc.DrawRectangle(null, ghostPen, rect);
                    }
                }
            }

            // ── Текущая фигура ──────────────────────────────────
            foreach (var (x, y) in _vm.CurrentPiece.GetAbsoluteCells())
                if (y >= 0 && y < board.Height)
                    DrawCell(dc, x, y, color);

            // ── Эффекты ────────────────────────────────────────────
            foreach (var effect in _vm.Effects.Active)
                effect.Render(dc, CellSize, _vm.Board.Width, _vm.Board.Height);
        }
    }

    private static void DrawCell(DrawingContext dc, int x, int y, Color color)
    {
        var rect = new Rect(x * CellSize + 1, y * CellSize + 1, CellSize - 2, CellSize - 2);
        dc.DrawRectangle(new SolidColorBrush(color), null, rect);

        // Блик
        var highlight = Color.FromArgb(60, 255, 255, 255);
        dc.DrawRectangle(new SolidColorBrush(highlight), null, new Rect(rect.X, rect.Y, rect.Width, 3));
    }
}
