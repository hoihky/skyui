using Avalonia.Media;
using SkyUI.Core.Theming;

namespace SkyUI.UnitTests;

public class SkyPaletteContrastTests
{
    public static IEnumerable<object[]> PaletteFiles =>
    [
        ["src/SkyUI.Themes.Sky/Themes/SkyDark/SkyPalette.Dark.axaml"],
        ["src/SkyUI.Themes.Sky/Themes/SkyDark/SkyPalette.Light.axaml"],
        ["src/SkyUI.Themes.Sky/Themes/SkyDark/SkyPalette.HighContrast.axaml"],
    ];

    [Theory]
    [MemberData(nameof(PaletteFiles))]
    public void Primary_text_meets_AA_on_background_and_surface(string palettePath)
    {
        var p = SkyPaletteAxamlReader.LoadPalette(palettePath);
        var text = p[SkyPaletteKeys.TextPrimary];
        Assert.True(SkyContrast.MeetsAaText(text, p[SkyPaletteKeys.Background]), palettePath);
        Assert.True(SkyContrast.MeetsAaText(text, p[SkyPaletteKeys.Surface]), palettePath);
    }

    [Theory]
    [MemberData(nameof(PaletteFiles))]
    public void Secondary_text_meets_AA_on_background_and_surface(string palettePath)
    {
        var p = SkyPaletteAxamlReader.LoadPalette(palettePath);
        var text = p[SkyPaletteKeys.TextSecondary];
        Assert.True(SkyContrast.MeetsAaText(text, p[SkyPaletteKeys.Background]), palettePath);
        Assert.True(SkyContrast.MeetsAaText(text, p[SkyPaletteKeys.Surface]), palettePath);
    }

    [Theory]
    [MemberData(nameof(PaletteFiles))]
    public void On_accent_meets_AA_on_accent(string palettePath)
    {
        var p = SkyPaletteAxamlReader.LoadPalette(palettePath);
        Assert.True(
            SkyContrast.MeetsAaText(p[SkyPaletteKeys.OnAccent], p[SkyPaletteKeys.Accent]),
            palettePath);
    }

    [Theory]
    [MemberData(nameof(PaletteFiles))]
    public void Border_and_focus_ring_meet_AA_UI_on_surface(string palettePath)
    {
        var p = SkyPaletteAxamlReader.LoadPalette(palettePath);
        var surface = p[SkyPaletteKeys.Surface];
        Assert.True(SkyContrast.MeetsAaUi(p[SkyPaletteKeys.Border], surface), palettePath);
        Assert.True(SkyContrast.MeetsAaUi(p[SkyPaletteKeys.FocusRing], surface), palettePath);
    }
}
