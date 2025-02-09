using Avalonia;
using System;
using System.IO;

namespace SystemPulse.Desktop;

class Program {
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) {
        Directory.SetCurrentDirectory(Path.GetDirectoryName(Environment.ProcessPath)!);
        BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions {
                CompositionMode = [
                    Win32CompositionMode.RedirectionSurface, // seems to use way less cpu with no visible issues
                    Win32CompositionMode.WinUIComposition,
                    Win32CompositionMode.DirectComposition,
                ],
            })
            .WithInterFont()
            .LogToTrace();

}
