using System.Windows.Media;
using Tetris.Core;

namespace Tetris.Wpf.Views;

/// <summary>
/// Единый источник цветов фигур. Используется и в игровом поле, и в превью.
/// </summary>
public static class TetrominoColors
{
    private static readonly IReadOnlyDictionary<TetrominoType, Color> Map =
        new Dictionary<TetrominoType, Color>
        {
            [TetrominoType.I] = Colors.Cyan,
            [TetrominoType.O] = Colors.Yellow,
            [TetrominoType.T] = Colors.MediumPurple,
            [TetrominoType.S] = Colors.LimeGreen,
            [TetrominoType.Z] = Colors.OrangeRed,
            [TetrominoType.J] = Colors.RoyalBlue,
            [TetrominoType.L] = Colors.Orange,
        };

    private static readonly Color Fallback = Colors.Gray;

    public static Color For(TetrominoType type) =>
        Map.TryGetValue(type, out var color) 
            ? color 
            : Fallback;
}
