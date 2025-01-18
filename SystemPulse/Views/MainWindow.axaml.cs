using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using System;

namespace SystemPulse.Views;

public partial class MainWindow : Window {
    private bool _appBarRegistered;
    private nint _appBarHandle;

    public MainWindow() {
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e) {
        base.OnOpened(e);
        RegisterAppBar();
        Settings.Default.PropertyChanged += Settings_PropertyChanged;
        AppDomain.CurrentDomain.UnhandledException += (sender, e) => UnregisterAppBar();
    }

    protected override void OnClosing(WindowClosingEventArgs e) {
        base.OnClosing(e);

        UnregisterAppBar();
    }

    private void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
        if (!(e.PropertyName is nameof(ISettings.TargetScreen) or nameof(ISettings.Side))) return;

        UnregisterAppBar();
        RegisterAppBar();
    }

    private void RegisterAppBar() {
        RegisterAppBar(Screens.All[Settings.Default.TargetScreen], Settings.Default.Side);
    }
    private void RegisterAppBar(Screen targetScreen, Side side) {
        var bounds = targetScreen.Bounds;

        Position = side == Side.Right ? new PixelPoint(bounds.Right - (int)Width, bounds.TopLeft.Y) : new PixelPoint(bounds.X, bounds.TopLeft.Y);
        Height = bounds.Height;

        _appBarHandle = TryGetPlatformHandle()?.Handle ?? nint.Zero;
        _appBarRegistered = Native.CreateAppBar(_appBarHandle);
        if (_appBarRegistered) {
            var rect = Native.SetAppBarPosition(_appBarHandle, targetScreen, new Size(Width, Height), side);
            Dispatcher.UIThread.Invoke(() => {
                Position = rect.Position;
                Width = rect.Width;
                Height = rect.Height;
            }, DispatcherPriority.ApplicationIdle);
        }
    }

    private void UnregisterAppBar() {
        if (_appBarRegistered) {
            Native.RemoveAppBar(_appBarHandle);
            _appBarRegistered = false;
        }
    }
}
