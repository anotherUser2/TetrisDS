namespace Tetris.Core.Tests;

public class PieceProviderTests
{
    [Fact]
    public void Peek_DoesNotConsumePiece()
    {
        var provider = new RandomPieceProvider(seed: 42);
        var first = provider.Peek();
        var second = provider.Peek();
        Assert.Equal(first, second);
    }

    [Fact]
    public void Peek_ReturnsSamePieceAsNext()
    {
        var provider = new RandomPieceProvider(seed: 42);
        var peeked = provider.Peek();
        var next = provider.Next();
        Assert.Equal(peeked, next.Type);
    }

    [Fact]
    public void Next_AfterSevenPieces_ContainsAllTypes()
    {
        var provider = new RandomPieceProvider(seed: 7);
        var seen = new HashSet<TetrominoType>();

        for (int i = 0; i < 7; i++)
        {
            seen.Add(provider.Next().Type);
        }
        
        Assert.Equal(7, seen.Count);
        Assert.Equal(Enum.GetValues<TetrominoType>().ToHashSet(), seen);
    }
}
