namespace Tetris.Core;

/// <summary>
/// Правило: с каждым уровнем интервал тика уменьшается.
/// Формула: max(minMs, baseMs - (level - 1) * stepMs).
/// </summary>
public static class LevelSpeed
{
    public const int BaseMilliseconds = 500;
    public const int StepMilliseconds = 40;
    public const int MinMilliseconds = 80;

    public static int IntervalMilliseconds(int level)
    {
        if (level < 1)
        {
            level = 1;
        }

        int ms = BaseMilliseconds - (level - 1) * StepMilliseconds;
        return Math.Max(MinMilliseconds, ms);
    }
}