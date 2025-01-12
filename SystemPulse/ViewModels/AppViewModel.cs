using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.Input;
using Injectio.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace SystemPulse.ViewModels;

[RegisterTransient]
[SupportedOSPlatform("windows")]
public partial class AppViewModel(ILogger<AppViewModel> logger) : ViewModelBase {
    const string RUN_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    const string APP_NAME = "SystemPulse";

    [RelayCommand]
    private void AddStartup() {
        string appPath = Environment.ProcessPath;
        try {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RUN_KEY, true);
            if (key == null) {
                logger.LogError("Error accessing the registry.");
                return;
            }

            key.SetValue(APP_NAME, appPath);
            logger.LogInformation("Application added to startup.");
        } catch (Exception ex) {
            logger.LogError("An error occurred while trying to add the application to startup {ex}", ex);
        }
    }

    [RelayCommand]
    private void RemoveStartup() {
        try {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RUN_KEY, true);
            if (key == null) {
                logger.LogError("Error accessing the registry.");
                return;
            }

            key.DeleteValue(APP_NAME);
            logger.LogInformation("Application removed from startup.");
        } catch (Exception ex) {
            logger.LogError("An error occurred while trying to remove the application from startup {ex}", ex);
        }
    }

    [RelayCommand]
    private static void Exit() {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.Shutdown();
        }
    }
}
