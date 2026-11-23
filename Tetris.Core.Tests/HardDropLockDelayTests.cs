namespace Tetris.Core.Tests;

public class HardDropLockDelayTests
{
    private static GameEngine NewEngine(int hardDropDelay = -1)
    {
        return new GameEngine(
            new Board(width: 10, height: 20),
            new FixedPieceProvider(TetrominoType.O, TetrominoType.O, TetrominoType.O))
        {
            HardDropLockDelayMilliseconds = hardDropDelay
        };
    }

    [Fact]
    public void HardDrop_DoesNotLockImmediately_WhenDelayEnabled()
    {
        var engine = NewEngine();
        engine.HardDrop();

        // Фигура упала до низа, но не зафиксирована
        Assert.True(engine.IsGrounded);
        Assert.False(engine.Board.IsOccupied(3, 19));
        Assert.Equal(0, engine.LockDelayAccumulatedMs);
    }

    [Fact]
    public void HardDrop_LocksAfterDelayElapses()
    {
        var engine = NewEngine();
        engine.HardDrop();

        bool locked = engine.Tick(engine.LockDelayMilliseconds);
        Assert.True(locked);
        Assert.True(engine.Board.IsOccupied(3, 19));
        Assert.True(engine.Board.IsOccupied(4, 19));
    }

    [Fact]
    public void HardDrop_DelayAccumulatesAcrossTicks()
    {
        var engine = NewEngine();
        engine.HardDrop();

        engine.Tick(200);
        engine.Tick(200);
        Assert.False(engine.Board.IsOccupied(3, 19));   // 400 < 500

        bool locked = engine.Tick(150);                  // 550 >= 500
        Assert.True(locked);
        Assert.True(engine.Board.IsOccupied(3, 19));
    }

    [Fact]
    public void HardDrop_MoveAfterDrop_ResetsLockDelay()
    {
        var engine = NewEngine();
        engine.HardDrop();
        engine.Tick(400);

        engine.MoveLeft();
        Assert.Equal(0, engine.LockDelayAccumulatedMs);

        bool locked = engine.Tick(400);
        Assert.False(locked);   // 400 < 500
    }

    [Fact]
    public void HardDrop_RotateAfterDrop_ResetsLockDelay()
    {
        var engine = new GameEngine(
            new Board(width: 10, height: 20),
            new FixedPieceProvider(TetrominoType.O, TetrominoType.O, TetrominoType.O));

        engine.HardDrop();
        engine.Tick(400);
        Assert.Equal(400, engine.LockDelayAccumulatedMs);

        engine.Rotate();   // O остаётся 2×2 — коллизии нет

        Assert.Equal(0, engine.LockDelayAccumulatedMs);
    }

    [Fact]
    public void HardDrop_RotateWithCollision_DoesNotResetLockDelay()
    {
        var engine = new GameEngine(
            new Board(width: 10, height: 20),
            new FixedPieceProvider(TetrominoType.T, TetrominoType.O, TetrominoType.O));

        engine.HardDrop();
        engine.Tick(400);
        Assert.Equal(400, engine.LockDelayAccumulatedMs);

        engine.Rotate();   // T на дне: поворот упирается в границу → откат

        Assert.Equal(400, engine.LockDelayAccumulatedMs);
    }

    [Fact]
    public void HardDrop_LockResetsAreLimited_SharedWithNormalLanding()
    {
        var engine = new GameEngine(
            new Board(width: 10, height: 20),
            new FixedPieceProvider(TetrominoType.O, TetrominoType.O, TetrominoType.O))
        {
            MaxLockResets = 2
        };

        engine.HardDrop();

        // 2 сброса — ок
        engine.Tick(400); engine.MoveLeft();
        engine.Tick(400); engine.MoveRight();

        // Третий сдвиг не сбрасывает
        engine.Tick(400);
        Assert.Equal(400, engine.LockDelayAccumulatedMs);
        engine.MoveLeft();
        Assert.Equal(400, engine.LockDelayAccumulatedMs);

        bool locked = engine.Tick(100);
        Assert.True(locked);
    }

    [Fact]
    public void HardDrop_InstantLock_WhenDelayIsZero()
    {
        var engine = NewEngine(hardDropDelay: 0);
        engine.HardDrop();

        // Фиксация сразу
        Assert.True(engine.Board.IsOccupied(3, 19));
        Assert.Equal(0, engine.LockDelayAccumulatedMs);
    }

    [Fact]
    public void HardDrop_DefaultDelayEqualsLockDelay()
    {
        var engine = new GameEngine(
            new Board(width: 10, height: 20),
            new FixedPieceProvider(TetrominoType.O, TetrominoType.O, TetrominoType.O))
        {
            LockDelayMilliseconds = 300
        };

        Assert.Equal(300, engine.HardDropLockDelayMilliseconds);
    }

    [Fact]
    public void HardDrop_MovesToGhostY()
    {
        var engine = new GameEngine(
            new Board(width: 10, height: 20),
            new FixedPieceProvider(TetrominoType.O, TetrominoType.O, TetrominoType.O));

        int ghostY = engine.GetGhostY();
        engine.HardDrop();
        Assert.Equal(ghostY, engine.CurrentPiece.Y);
    }

    [Fact]
    public void HardDrop_AfterGameOver_DoesNothing()
    {
        var engine = new GameEngine(
            new Board(width: 4, height: 4),
            new FixedPieceProvider(TetrominoType.O, TetrominoType.O, TetrominoType.O));

        engine.ForceGameOver();
        int scoreBefore = engine.Score;
        engine.HardDrop();
        Assert.Equal(scoreBefore, engine.Score);
    }
}