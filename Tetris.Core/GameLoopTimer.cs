namespace Tetris.Core;

/// <summary>
/// Абстракция над таймером, чтобы GameEngine не зависел от UI-потока.
/// </summary>
public interface IGameTimer : IDisposable
{
    event EventHandler? Tick;
    void Start();
    void Stop();
    bool IsRunning { get; }
}