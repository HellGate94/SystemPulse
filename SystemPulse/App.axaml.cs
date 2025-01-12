using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SystemPulse.Views;

namespace SystemPulse;
public partial class App : Application {
    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
    }

    public static void ConfigureServices() {
        var services = new ServiceCollection();

        services.AddLogging(builder => builder.AddDebug());
        services.AddSystemPulse();

        Ioc.Default.ConfigureServices(services.BuildServiceProvider());
    }

    public override void OnFrameworkInitializationCompleted() {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            ConfigureServices();
            desktop.MainWindow = new MainWindow();
        }
        DataContext = Ioc.Default.GetService<ViewModels.AppViewModel>();

        base.OnFrameworkInitializationCompleted();
    }
}
