using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.Input;
using Injectio.Attributes;
using SystemPulse.Views;

namespace SystemPulse.ViewModels;

[RegisterTransient]
public partial class AppViewModel : ViewModelBase {
    private static SettingsWindow? _settingsWindow;

    [RelayCommand]
    private static void Settings() {
        if (_settingsWindow == null || !_settingsWindow.IsVisible) {
            _settingsWindow = new SettingsWindow();
            _settingsWindow.Closed += (_, _) => _settingsWindow = null;
            _settingsWindow.Show();
        } else {
            _settingsWindow.Activate(); // TODO: Does not bring the Window to front for whatever reason...
        }
    }

    // Why? Because Windows is shit and explorer.exe crashes and need to register the appbar after a restart
    [RelayCommand]
    private static void Reinitialize() {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            (desktop.MainWindow as MainWindow)?.Reinitialize();
        }
    }

    [RelayCommand]
    private static void Exit() {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.Shutdown();
        }
    }
}
