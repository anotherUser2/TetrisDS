namespace Tetris.Core.Tests;

public class ScoringTests
{
    [Theory]
    [InlineData(1, 100)]
    [InlineData(2, 300)]
    [InlineData(3, 500)]
    [InlineData(4, 1500)]
    public void BasePoints_AreCorrect(int lines, int expected)
    {
        Assert.Equal(expected, Scoring.ForLinesCleared(lines, level: 1));
    }

    [Fact]
    public void TetrisPaysMoreThanTriple()
    {
        Assert.True(
            Scoring.ForLinesCleared(4, 1) > Scoring.ForLinesCleared(3, 1),
            "Tetris должен приносить больше, чем Triple");
    }

    [Fact]
    public void TetrisBonus_IsAtLeastHalfOfFullFourSingles()
    {
        // Четыре одиночные линии (по отдельности) vs один Tetris
        int fourSingles = 4 * Scoring.ForLinesCleared(1, 1);
        int oneTetris = Scoring.ForLinesCleared(4, 1);
        Assert.True(oneTetris >= fourSingles / 2,
            "Tetris не должен обесцениваться по сравнению с четырьмя синглами");
    }

    [Theory]
    [InlineData(1, 2, 200)]
    [InlineData(2, 2, 600)]
    [InlineData(3, 2, 1000)]
    [InlineData(4, 2, 3000)]
    [InlineData(4, 5, 7500)]
    public void PointsScaleWithLevel(int lines, int level, int expected)
    {
        Assert.Equal(expected, Scoring.ForLinesCleared(lines, level));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(-1)]
    public void OutOfRangeLines_YieldZero(int lines)
    {
        Assert.Equal(0, Scoring.ForLinesCleared(lines, 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void NonPositiveLevel_ClampedToOne(int level)
    {
        Assert.Equal(100, Scoring.ForLinesCleared(1, level));
    }

    [Fact]
    public void MonotonicInLevel()
    {
        int prev = -1;
        for (int lvl = 1; lvl <= 20; lvl++)
        {
            int pts = Scoring.ForLinesCleared(4, lvl);
            Assert.True(pts > prev);
            prev = pts;
        }
    }
}