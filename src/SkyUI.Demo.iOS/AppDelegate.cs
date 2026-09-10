using Avalonia;
using Avalonia.iOS;
using Foundation;
using SkyUI.Demo.Mobile;
using SkyUI.Fonts;

namespace SkyUI.Demo.iOS;

[Register(nameof(AppDelegate))]
public class AppDelegate : AvaloniaAppDelegate<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) =>
        base.CustomizeAppBuilder(builder)
            .ConfigureSkyFonts();
}
