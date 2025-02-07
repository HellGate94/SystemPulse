using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using SystemPulse.Services;
using SystemPulse.Views;

namespace SystemPulse;
public partial class App : Application {
    private const string SettingsFilePath = "settings.json";

    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
    }

    public void ConfigureServices() {
        var services = new ServiceCollection();

        services.AddLogging(builder => builder.AddDebug());

        Settings settings;
        if (File.Exists(SettingsFilePath)) {
            var json = File.ReadAllText(SettingsFilePath);
            settings = JsonSerializer.Deserialize<Settings>(json)!;
        } else {
            settings = new Settings();
        }
        settings.PropertyChanged += Settings_PropertyChanged;
        services.AddSingleton(settings);
        services.AddSystemPulse();

        Ioc.Default.ConfigureServices(services.BuildServiceProvider());
        Resources[typeof(IServiceProvider)] = services;
    }

    private static void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
        string json = JsonSerializer.Serialize(Settings.Default);
        File.WriteAllText(SettingsFilePath, json);
    }

    public override void OnFrameworkInitializationCompleted() {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        ConfigureServices();
        DataContext = Ioc.Default.GetService<ViewModels.AppViewModel>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    private void Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e) {
        if (DataContext is IDisposable disposable)
            disposable.Dispose();
    }
}
