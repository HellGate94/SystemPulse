using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using Config.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;
using SystemPulse.Views;

namespace SystemPulse;
public partial class App : Application {
    private const string SettingsFilePath = "settings.json";

    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
    }

    public static void ConfigureServices() {
        var services = new ServiceCollection();

        services.AddLogging(builder => builder.AddDebug());

        ISettings settings = new ConfigurationBuilder<ISettings>()
           .UseJsonFile(SettingsFilePath)
           .Build();
        settings.PropertyChanged += Settings_PropertyChanged;
        services.AddSingleton(settings);

        services.AddSystemPulse();

        Ioc.Default.ConfigureServices(services.BuildServiceProvider());
    }

    private static void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
        string json = JsonSerializer.Serialize(Settings.Default);
        File.WriteAllText(SettingsFilePath, json);
    }

    public override void OnFrameworkInitializationCompleted() {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            if (!Design.IsDesignMode)
                ConfigureServices();
            desktop.MainWindow = new MainWindow();
        }
        if (!Design.IsDesignMode)
            DataContext = Ioc.Default.GetService<ViewModels.AppViewModel>();

        base.OnFrameworkInitializationCompleted();
    }
}
