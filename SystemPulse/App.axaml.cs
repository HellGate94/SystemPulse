using Avalonia;
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
    public const string AppName = "SystemPulse";
    private const string SettingsFilePath = "settings.json";

    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public void ConfigureServices() {
        var services = new ServiceCollection();

        services.AddLogging(builder => builder.AddDebug());

        Settings settings = LoadSettings();
        settings.PropertyChanged += Settings_PropertyChanged;
        services.AddSingleton(settings);
        services.AddSingleton(new StartupOptions(
            AppName,
            $"Starts {AppName} when the user logs in.",
            Environment.ProcessPath!
        ));
        services.AddSystemPulse();

        Ioc.Default.ConfigureServices(services.BuildServiceProvider());
        Resources[typeof(IServiceProvider)] = services;
    }

    private static Settings LoadSettings() {
        if (!File.Exists(SettingsFilePath))
            return new Settings();

        var json = File.ReadAllText(SettingsFilePath);
        return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
    }

    private static void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
        string json = JsonSerializer.Serialize(Settings.Default);
        File.WriteAllText(SettingsFilePath, json);
    }

    public override void OnFrameworkInitializationCompleted() {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        //BindingPlugins.DataValidators.RemoveAt(0);

        ConfigureServices();
        DataContext = Ioc.Default.GetService<ViewModels.AppViewModel>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.MainWindow = new MainWindow();
            Native.EnablePowerThrottling();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e) {
        if (DataContext is IDisposable disposable)
            disposable.Dispose();
    }
}
