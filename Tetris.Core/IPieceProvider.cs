namespace Tetris.Core;

/// <summary>
/// Абстракция источника фигур. В продакшене — случайный 7-bag.
/// В тестах подставляется детерминированный провайдер.
/// </summary>
public interface IPieceProvider
{
    /// <summary>Достаёт следующую фигуру и сдвигает очередь.</summary>
    Tetromino Next();

    /// <summary>Возвращает тип следующей фигуры, не сдвигая очередь.</summary>
    TetrominoType Peek();
}

public sealed class RandomPieceProvider : IPieceProvider
{
    private readonly Random _random;
    private readonly List<TetrominoType> _bag = [];

    public RandomPieceProvider(int? seed = null)
    {
        _random = seed.HasValue 
            ? new Random(seed.Value) 
            : new Random();
    }

    public Tetromino Next()
    {
        EnsureBag();
        var type = _bag[0];
        _bag.RemoveAt(0);
        return new Tetromino(type);
    }

    public TetrominoType Peek()
    {
        EnsureBag();
        return _bag[0];
    }

    private void EnsureBag()
    {
        if (_bag.Count > 0)
        {
            return;
        }
        
        _bag.AddRange(Enum.GetValues<TetrominoType>());

        for (int i = _bag.Count - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (_bag[i], _bag[j]) = (_bag[j], _bag[i]);
        }
    }
}
