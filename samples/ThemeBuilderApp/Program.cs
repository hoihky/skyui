using Avalonia;
using SkyUI.Fonts;
using System;

namespace SkyUI.Samples.ThemeBuilderApp;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .ConfigureSkyFonts()
            .LogToTrace();
}
