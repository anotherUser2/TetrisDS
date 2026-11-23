namespace Tetris.Core.Tests; 
using Tetris.Core;


public class EffectsTests
{
    [Fact]
    public void CrackSelector_FindsNeighbors_NotOwnCells()
    {
        var board = new Board(10, 20);
        board.Lock(new Tetromino(TetrominoType.O, 0, 18)); // соседи снизу
                                                           // O падает в (0, 16)-(1,17) → соседи: (0,18),(1,18)
        var pieceCells = new List<(int, int)> { (0, 16), (1, 16), (0, 17), (1, 17) };
        var cracked = CrackSelector.SelectCrackedCells(pieceCells, board);
        Assert.Contains((0, 18), cracked);
        Assert.Contains((1, 18), cracked);
        Assert.DoesNotContain((0, 17), cracked);  // своя клетка
    }
}
