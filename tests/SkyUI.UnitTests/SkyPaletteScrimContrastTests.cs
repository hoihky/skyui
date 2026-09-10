using SkyUI.Core.Theming;

namespace SkyUI.UnitTests;

public class SkyPaletteScrimContrastTests
{
    public static IEnumerable<object[]> PaletteFiles =>
    [
        ["src/SkyUI.Themes.Sky/Themes/SkyDark/SkyPalette.Dark.axaml"],
        ["src/SkyUI.Themes.Sky/Themes/SkyDark/SkyPalette.Light.axaml"],
        ["src/SkyUI.Themes.Sky/Themes/SkyDark/SkyPalette.HighContrast.axaml"],
    ];

    [Theory]
    [MemberData(nameof(PaletteFiles))]
    public void Scrim_is_defined_for_overlay_use(string palettePath)
    {
        var palette = SkyPaletteAxamlReader.LoadPalette(palettePath);
        Assert.True(palette.ContainsKey(SkyPaletteKeys.Scrim), palettePath);
    }
}
