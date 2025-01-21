using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SystemPulse;

public partial class SettingsWindow : Window {
    public SettingsWindow() {
        InitializeComponent();
        if (!Design.IsDesignMode)
            DataContext = Ioc.Default.GetService<ViewModels.SettingsViewModel>();
    }
}