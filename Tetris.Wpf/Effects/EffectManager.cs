using Tetris.Core;
using static Tetris.Core.GameEngine;

namespace Tetris.Wpf.Effects;

/// <summary>
/// Хранит активные эффекты и тикает их. Расширяется через регистрацию фабрик.
/// </summary>
public sealed class EffectManager
{
    private readonly List<IBoardEffect> _active = [];

    /// <summary>Фабрики: тип эффекта → как создать по событию HardDrop.</summary>
    private readonly Dictionary<string, Func<HardDropInfo, Board, IBoardEffect?>> _factories = [];

    public IReadOnlyList<IBoardEffect> Active => _active;

    /// <summary>Зарегистрировать фабрику эффекта.</summary>
    public void Register(string name, Func<HardDropInfo, Board, IBoardEffect?> factory)
        => _factories[name] = factory;

    /// <summary>Вызвать все фабрики для события HardDrop.</summary>
    public void OnHardDrop(HardDropInfo info, Board board)
    {
        foreach (var factory in _factories.Values)
        {
            var effect = factory(info, board);
            if (effect is not null) _active.Add(effect);
        }
    }

    /// <summary>Обновить все эффекты на deltaMs, удалить завершённые.</summary>
    public void Update(int deltaMs)
    {
        for (int i = _active.Count - 1; i >= 0; i--)
            if (!_active[i].Update(deltaMs))
                _active.RemoveAt(i);
    }

    public void Clear() => _active.Clear();
}