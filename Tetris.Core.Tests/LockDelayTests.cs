namespace Tetris.Core.Tests;

public class LockDelayTests
{
    [Fact]
    public void Tick_AtLockDelayThreshold_LocksPiece()
    {
        var engine = new GameEngine(
            new Board(width: 10, height: 4),
            new FixedPieceProvider(TetrominoType.O, TetrominoType.O));

        while (engine.MoveDown()) { }
        Assert.True(engine.IsGrounded);

        bool locked = engine.Tick(engine.LockDelayMilliseconds);  // Tick(500)
        Assert.True(locked);
        Assert.True(engine.Board.IsOccupied(3, 3));
    }

    [Fact]
    public void Tick_BeforeLockDelay_DoesNotLock()
    {
        var engine = new GameEngine(
            new Board(width: 10, height: 4),
            new FixedPieceProvider(TetrominoType.O, TetrominoType.O));

        while (engine.MoveDown()) { }

        bool locked = engine.Tick(engine.LockDelayMilliseconds - 1);
        Assert.False(locked);
        Assert.False(engine.Board.IsOccupied(3, 3));
    }

    [Fact]
    public void LockDelay_AccumulatesAcrossTicks()
    {
        var engine = new GameEngine(
            new Board(width: 10, height: 4),
            new FixedPieceProvider(TetrominoType.O, TetrominoType.O));

        while (engine.MoveDown()) { }

        engine.Tick(200);
        engine.Tick(200);
        Assert.False(engine.Board.IsOccupied(3, 3));  // 400 < 500

        bool locked = engine.Tick(150);               // 550 >= 500
        Assert.True(locked);
        Assert.True(engine.Board.IsOccupied(3, 3));
    }

    [Fact]
    public void MoveLeft_OnGround_ResetsLockDelay()
    {
        var engine = new GameEngine(
            new Board(width: 10, height: 4),
            new FixedPieceProvider(TetrominoType.O, TetrominoType.O));

        while (engine.MoveDown()) { }

        engine.Tick(400);
        Assert.Equal(400, engine.LockDelayAccumulatedMs);

        engine.MoveLeft();
        Assert.Equal(0, engine.LockDelayAccumulatedMs);

        bool locked = engine.Tick(400);
        Assert.False(locked);   // ещё рано
    }

    [Fact]
    public void MoveRight_OnGround_ResetsLockDelay()
    {
        var engine = new GameEngine(
            new Board(width: 10, height: 4),
            new FixedPieceProvider(TetrominoType.O, TetrominoType.O));

        while (engine.MoveDown()) { }
        engine.Tick(400);
        engine.MoveRight();
        Assert.Equal(0, engine.LockDelayAccumulatedMs);
    }

    [Fact]
    public void Rotate_OnGround_ResetsLockDelay()
    {
        var engine = new GameEngine(
            new Board(width: 10, height: 4),
            new FixedPieceProvider(TetrominoType.O, TetrominoType.O));

        while (engine.MoveDown()) { }
        Assert.True(engine.IsGrounded);

        engine.Tick(400);
        Assert.Equal(400, engine.LockDelayAccumulatedMs);

        engine.Rotate();   // O остаётся 2×2, коллизии нет → OnSuccessfulMove сработает

        Assert.Equal(0, engine.LockDelayAccumulatedMs);
    }

    [Fact]
    public void MoveInAir_DoesNotResetLockDelay()
    {
        var engine = new GameEngine(
            new Board(width: 10, height: 20),
            new FixedPieceProvider(TetrominoType.O, TetrominoType.O));

        engine.Tick(400);            // O ещё в воздухе
        engine.MoveLeft();
        Assert.Equal(0, engine.LockDelayAccumulatedMs); // в воздухе ничего не копится
    }

    [Fact]
    public void LockResets_AreLimited()
    {
        var engine = new GameEngine(
            new Board(width: 10, height: 4),
            new FixedPieceProvider(TetrominoType.O, TetrominoType.O))
        {
            MaxLockResets = 3
        };

        while (engine.MoveDown()) { }

        // Три сброса — ок
        for (int i = 0; i < 3; i++)
        {
            engine.Tick(400);
            engine.MoveLeft();
        }

        // Четвёртый сдвиг уже не должен сбросить
        engine.Tick(400);
        Assert.Equal(400, engine.LockDelayAccumulatedMs);
        engine.MoveRight();          // не сбрасывает
        Assert.Equal(400, engine.LockDelayAccumulatedMs);

        bool locked = engine.Tick(100);   // 500 → фиксация
        Assert.True(locked);
    }

    [Fact]
    public void InAir_AccumulatedStaysZero()
    {
        var engine = new GameEngine(
            new Board(width: 10, height: 20),
            new FixedPieceProvider(TetrominoType.O, TetrominoType.O));

        engine.Tick(400);   // O падает, но не на земле
        Assert.Equal(0, engine.LockDelayAccumulatedMs);
    }
}