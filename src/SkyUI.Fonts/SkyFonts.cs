using Avalonia;
using Avalonia.Media.Fonts;

namespace SkyUI.Fonts;

/// <summary>Registers Inter and embedded Noto Sans SC for Sky UI.</summary>
public static class SkyFonts
{
    /// <summary>
    /// Call from <c>AppBuilder</c> (e.g. <c>Program.BuildAvaloniaApp()</c>) before the app starts.
    /// </summary>
    public static AppBuilder ConfigureSkyFonts(this AppBuilder builder) =>
        builder
            .WithInterFont()
            .ConfigureFonts(fontManager => fontManager.AddFontCollection(new SkyNotoFontCollection()));
}
