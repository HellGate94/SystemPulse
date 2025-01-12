using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using System;
using System.Runtime.InteropServices;

namespace SystemPulse.Views;

public partial class MainWindow : Window {
    private bool _appBarRegistered;
    private nint _appBarHandle;

    public MainWindow() {
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e) {
        base.OnOpened(e);

        var targetScreen = Screens.All[^1];
        if (targetScreen != null) {
            var bounds = targetScreen.Bounds;

            // Position the window on the far right of the specific monitor
            Position = new PixelPoint(bounds.Right - (int)Width, bounds.TopLeft.Y);
            Height = bounds.Height;

            // Adjust the work area of the specific monitor
            RegisterAppBar(targetScreen);
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => UnregisterAppBar();
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e) {
        base.OnClosing(e);

        UnregisterAppBar();
    }

    private void RegisterAppBar(Screen targetScreen) {
        _appBarHandle = TryGetPlatformHandle()?.Handle ?? nint.Zero;
        _appBarRegistered = Native.CreateAppBar(_appBarHandle);
        if (_appBarRegistered) {
            var rect = Native.SetAppBarPosition(_appBarHandle, targetScreen, new Size(Width, Height));
            Position = rect.Position;
            Width = rect.Width;
            Height = rect.Height;
        }
    }

    private void UnregisterAppBar() {
        if (_appBarRegistered) {
            Native.RemoveAppBar(_appBarHandle);
            _appBarRegistered = false;
        }
    }
}
