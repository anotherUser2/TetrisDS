namespace Tetris.Core;

/// <summary>
/// Правило начисления очков за очищенные линии.
/// Уровень учитывается как множитель — классическая механика.
/// </summary>
public static class Scoring
{
    /// <summary>Базовые очки за 1..4 очищенные линии за один ход.</summary>
    public static readonly IReadOnlyDictionary<int, int> BasePoints = new Dictionary<int, int>
    {
        [1] = 100,   // Single
        [2] = 300,   // Double
        [3] = 500,   // Triple
        [4] = 1500,  // Tetris — увеличенный бонус (было 800)
    };

    /// <summary>
    /// Очки за очистку <paramref name="lines"/> линий на уровне <paramref name="level"/>.
    /// Возвращает 0 для lines вне диапазона 1..4.
    /// </summary>
    public static int ForLinesCleared(int lines, int level)
    {
        if (lines < 1 || lines > 4) return 0;
        if (level < 1) level = 1;
        return BasePoints[lines] * level;
    }
}