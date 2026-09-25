using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Headless;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using SkyUI.Themes.Sky;

[assembly: AvaloniaTestApplication(typeof(SkyUI.HeadlessTests.SkyHeadlessTestApp))]

namespace SkyUI.HeadlessTests;

/// <summary>Minimal Avalonia application that loads the Sky theme for headless UI tests.</summary>
public sealed class SkyHeadlessTestApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        Styles.Add(new StyleInclude(new Uri(SkyTheme.IncludeUri)));
        SkyTheme.Apply(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new Window();

        base.OnFrameworkInitializationCompleted();
    }
}
