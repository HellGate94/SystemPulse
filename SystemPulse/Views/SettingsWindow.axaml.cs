using Avalonia.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace SystemPulse;

public partial class SettingsWindow : Window {
    public SettingsWindow() {
        InitializeComponent();
        if (!Design.IsDesignMode)
            DataContext = Ioc.Default.GetService<ViewModels.SettingsViewModel>();
    }
}