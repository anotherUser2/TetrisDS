namespace Tetris.Core.Tests;

public class LevelSpeedTests
{
    [Theory]
    [InlineData(1, 500)]
    [InlineData(2, 460)]
    [InlineData(3, 420)]
    [InlineData(5, 340)]
    [InlineData(10, 140)]
    [InlineData(11, 100)]
    public void Interval_DecreasesWithLevel(int level, int expected)
    {
        Assert.Equal(expected, LevelSpeed.IntervalMilliseconds(level));
    }

    [Fact]
    public void Interval_ClampsToMinimum()
    {
        Assert.Equal(LevelSpeed.MinMilliseconds, LevelSpeed.IntervalMilliseconds(12));
        Assert.Equal(LevelSpeed.MinMilliseconds, LevelSpeed.IntervalMilliseconds(50));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Interval_TreatsNonPositiveLevelAsLevelOne(int level)
    {
        Assert.Equal(LevelSpeed.BaseMilliseconds, LevelSpeed.IntervalMilliseconds(level));
    }

    [Fact]
    public void Interval_IsMonotonicallyNonIncreasing()
    {
        int prev = int.MaxValue;

        for (int lvl = 1; lvl <= 100; lvl++)
        {
            int ms = LevelSpeed.IntervalMilliseconds(lvl);
            Assert.True(ms <= prev, $"Уровень {lvl}: {ms} > {prev}");
            prev = ms;
        }
    }

    [Fact]
    public void LevelUp_ProducesSmallerInterval()
    {
        int lvl1 = LevelSpeed.IntervalMilliseconds(1);
        int lvl2 = LevelSpeed.IntervalMilliseconds(2);
        Assert.True(lvl2 < lvl1);
    }
}
