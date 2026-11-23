using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Tetris.Wpf.Converters;

/// <summary>
/// Возвращает Visibility.Visible, если значение равно параметру,
/// иначе Visibility.Collapsed. Параметр по умолчанию — "4".
/// </summary>
public sealed class IntEqualsToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        int expected = parameter is string s && int.TryParse(s, out var p) ? p : 4;
        return value is int actual && actual == expected
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}