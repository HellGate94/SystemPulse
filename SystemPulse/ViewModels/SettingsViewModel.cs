using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using Injectio.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Security.Principal;
using SystemPulse.Services;

namespace SystemPulse.ViewModels;

[RegisterTransient]
[SupportedOSPlatform("windows")]
public partial class SettingsViewModel : ViewModelBase {
    public ObservableCollection<string> DefaultIPServices { get; } = [
        "https://api.ipify.org",
        "https://icanhazip.com",
        "https://ipinfo.io/ip",
        "https://www.trackip.net/ip"
    ];

    private readonly ILogger<SettingsViewModel> _logger;

    public Settings Settings { get; }
    public StartupService StartupService { get; }
    public IReadOnlyList<Screen> ScreenList { get; init; }

    [ObservableProperty]
    ObservableCollection<Side> _sides = [.. Enum.GetValues<Side>()];

    [ObservableProperty]
    private bool _runOnStartup;

    public SettingsViewModel(ILogger<SettingsViewModel> logger, Settings settings, StartupService startupService) {
        _logger = logger;
        Settings = settings;
        StartupService = startupService;
        var app = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var mainWindow = app?.MainWindow;
        ScreenList = mainWindow?.Screens.All ?? [];

        _runOnStartup = startupService.IsEnabled;
    }

    partial void OnRunOnStartupChanged(bool value) {
        if (value) {
            StartupService.Enable();
        } else {
            StartupService.Disable();
        }
    }
}
