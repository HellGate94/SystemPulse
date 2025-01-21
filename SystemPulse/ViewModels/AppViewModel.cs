using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.Input;
using Injectio.Attributes;
using Microsoft.Extensions.Logging;

namespace SystemPulse.ViewModels;

[RegisterTransient]
public partial class AppViewModel(ILogger<AppViewModel> logger) : ViewModelBase {
    private static SettingsWindow? _settingsWindow;

    [RelayCommand]
    private static void Settings() {
        if (_settingsWindow == null || !_settingsWindow.IsVisible) {
            _settingsWindow = new SettingsWindow();
            _settingsWindow.Closed += (_, _) => _settingsWindow = null;
            _settingsWindow.Show();
        } else {
            _settingsWindow.Activate(); // Does not bring the Window to front for whatever reason...
        }
    }


    [RelayCommand]
    private static void Exit() {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.Shutdown();
        }
    }
}
