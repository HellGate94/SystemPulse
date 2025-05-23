﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using System;

namespace SystemPulse.Views;

public partial class MainWindow : Window {
    private bool _appBarRegistered;
    private nint _appBarHandle;

    public MainWindow() {
        InitializeComponent();
        DataContext = Ioc.Default.GetService<ViewModels.MainViewModel>();
    }

    protected override void OnOpened(EventArgs e) {
        base.OnOpened(e);
        if (Design.IsDesignMode) return;

        RegisterAppBar();
        Settings.Default!.PropertyChanged += Settings_PropertyChanged;
        AppDomain.CurrentDomain.UnhandledException += (sender, e) => UnregisterAppBar();
    }

    protected override void OnClosed(EventArgs e) {
        base.OnClosed(e);

        if (DataContext is IDisposable disposable)
            disposable.Dispose();

        if (Design.IsDesignMode) return;
        UnregisterAppBar();
    }

    private void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
        if (Design.IsDesignMode) return;
        if (!(e.PropertyName is nameof(Settings.TargetScreen) or nameof(Settings.Side))) return;

        Reinitialize();
    }

    public void Reinitialize() {
        UnregisterAppBar();
        RegisterAppBar();
    }

    private void RegisterAppBar() {
        var settings = Settings.Default!;
        RegisterAppBar(Screens.All[settings.TargetScreen], settings.Side);
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
                //Width = rect.Width / targetScreen.Scaling;
                //Height = rect.Height / targetScreen.Scaling;
            }, DispatcherPriority.ApplicationIdle);
        }
    }

    private void UnregisterAppBar() {
        if (_appBarRegistered) {
            Native.RemoveAppBar(_appBarHandle);
            _appBarRegistered = false;
        }
    }
    private void TextPointerReleasedHandler(object sender, PointerReleasedEventArgs args) {
        Clipboard?.SetTextAsync(((TextBlock)sender).Text);
    }
}
