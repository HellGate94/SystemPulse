using Avalonia.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using System;

namespace SystemPulse;

public partial class SettingsWindow : Window {
    public SettingsWindow() {
        InitializeComponent();
        DataContext = Ioc.Default.GetService<ViewModels.SettingsViewModel>();
    }

    protected override void OnClosed(EventArgs e) {
        base.OnClosed(e);

        if (DataContext is IDisposable disposable)
            disposable.Dispose();
    }
}