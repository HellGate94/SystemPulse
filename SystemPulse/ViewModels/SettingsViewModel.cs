using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Injectio.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Versioning;

namespace SystemPulse.ViewModels;

[RegisterTransient]
[SupportedOSPlatform("windows")]
public partial class SettingsViewModel : ViewModelBase {
    const string RUN_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    const string APP_NAME = "SystemPulse";
    public ObservableCollection<string> DefaultIPServices { get; } = [
        "https://api.ipify.org",
        "https://icanhazip.com",
        "https://ipinfo.io/ip",
        "https://www.trackip.net/ip"
    ];

    private readonly ILogger<SettingsViewModel> _logger;

    public ISettings Settings { get; }
    public IReadOnlyList<Screen> ScreenList { get; init; }

    [ObservableProperty]
    ObservableCollection<Side> _sides = new ObservableCollection<Side>(Enum.GetValues<Side>());

    [ObservableProperty]
    private bool _runOnStartup;

    public SettingsViewModel(ILogger<SettingsViewModel> logger, ISettings settings) {
        _logger = logger;
        Settings = settings;

        var app = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var mainWindow = app?.MainWindow;
        ScreenList = mainWindow?.Screens.All ?? [];

        _runOnStartup = CheckStartup();
    }

    partial void OnRunOnStartupChanged(bool value) {
        if (value) {
            AddStartup();
        } else {
            RemoveStartup();
        }
    }

    [RelayCommand]
    private void AddStartup() {
        try {
            using var key = Registry.CurrentUser.OpenSubKey(RUN_KEY, true);
            if (key == null) {
                _logger.LogError("Error accessing the registry.");
                return;
            }

            string appPath = Environment.ProcessPath!;
            key.SetValue(APP_NAME, appPath);
            _logger.LogInformation("Application added to startup.");
        } catch (Exception ex) {
            _logger.LogError("An error occurred while trying to add the application to startup {ex}", ex);
        }
    }

    [RelayCommand]
    private void RemoveStartup() {
        try {
            using var key = Registry.CurrentUser.OpenSubKey(RUN_KEY, true);
            if (key == null) {
                _logger.LogError("Error accessing the registry.");
                return;
            }

            key.DeleteValue(APP_NAME);
            _logger.LogInformation("Application removed from startup.");
        } catch (Exception ex) {
            _logger.LogError("An error occurred while trying to remove the application from startup {ex}", ex);
        }
    }


    private bool CheckStartup() {
        try {
            using var key = Registry.CurrentUser.OpenSubKey(RUN_KEY, true);
            if (key == null) {
                _logger.LogError("Error accessing the registry.");
                return false;
            }

            return key.GetValue(APP_NAME) is not null;
        } catch (Exception ex) {
            _logger.LogError("An error occurred while trying to remove the application from startup {ex}", ex);
        }
        return false;
    }
}
