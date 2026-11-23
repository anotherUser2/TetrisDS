using System.Windows.Media;

namespace Tetris.Wpf.Effects;

/// <summary>
/// Визуальный эффект на доске. Живёт заданное время, затем удаляется.
/// </summary>
public interface IBoardEffect
{
    /// <summary>Оставшееся время жизни, мс. Когда <= 0 — эффект удаляется.</summary>
    int RemainingMs { get; }

    /// <summary>Обновить состояние на прошедшие deltaMs. Возвращает false, если эффект завершён.</summary>
    bool Update(int deltaMs);

    /// <summary>Нарисовать эффект поверх доски.</summary>
    void Render(DrawingContext dc, double cellSize, int boardWidth, int boardHeight);
}

/// <summary>
/// Базовый класс: хранит время жизни и альфу. Наследники реализуют только Render.
/// </summary>
public abstract class TimedBoardEffect : IBoardEffect
{
    public int RemainingMs { get; protected set; }
    public int TotalMs { get; }
    public double Alpha => TotalMs == 0 ? 0 : Math.Clamp(RemainingMs / (double)TotalMs, 0, 1);

    protected TimedBoardEffect(int durationMs) { TotalMs = durationMs; RemainingMs = durationMs; }

    public bool Update(int deltaMs)
    {
        RemainingMs -= deltaMs;
        return RemainingMs > 0;
    }

    public abstract void Render(DrawingContext dc, double cellSize, int boardWidth, int boardHeight);
}