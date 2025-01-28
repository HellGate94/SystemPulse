using Avalonia.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using System;

namespace SystemPulse;

public partial class OverlayWindow : Window {
    public OverlayWindow() {
        InitializeComponent();
        DataContext = Ioc.Default.GetService<ViewModels.OverlayViewModel>();
    }

    protected override void OnClosed(EventArgs e) {
        base.OnClosed(e);

        if (DataContext is IDisposable disposable)
            disposable.Dispose();
    }
}