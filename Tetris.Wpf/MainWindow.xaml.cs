using System.Windows;
using System.Windows.Input;
using Tetris.Wpf.ViewModels;

namespace Tetris.Wpf;

public partial class MainWindow : Window
{
    private MainViewModel _vm = null!;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _vm = new MainViewModel();
        DataContext = _vm;
        BoardControl.Bind(_vm);
        NextControl.Bind(_vm);   // <-- добавили
        _vm.Start();
        Focus();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (_vm is null)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Left: _vm.MoveLeft(); break;
            case Key.Right: _vm.MoveRight(); break;
            case Key.Down: _vm.MoveDown(); break;
            case Key.Up: _vm.Rotate(); break;
            case Key.Space: _vm.HardDrop(); break;
            case Key.P: _vm.TogglePause(); break;
            case Key.R: Restart(); break;
            case Key.G: _vm.ToggleGhost(); break;
            case Key.Escape: _vm.SetStatusText("Bye!"); Thread.Sleep(1000); Close(); break;
        }

        e.Handled = true;
    }

    private void Restart()
    {
        _vm.Dispose();
        _vm = new MainViewModel();
        DataContext = _vm;
        BoardControl.Bind(_vm);
        NextControl.Bind(_vm);   // <-- добавили
        _vm.Start();
    }
}
