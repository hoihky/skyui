using Avalonia.Media;
using SkyUI.Core.Theming;

namespace SkyUI.UnitTests;

public class SkyColorHarmonyTests
{
    [Fact]
    public void Darken_reduces_channels()
    {
        var c = Color.FromRgb(200, 100, 50);
        var d = SkyColorHarmony.Darken(c, 0.1);
        Assert.True(d.R < c.R);
        Assert.True(d.G < c.G);
        Assert.True(d.B < c.B);
    }

    [Fact]
    public void AccentSelectionTint_uses_refined_alpha()
    {
        var accent = Color.FromRgb(30, 215, 96);
        var tint = SkyColorHarmony.AccentSelectionTint(accent);
        Assert.Equal(0x3A, tint.A);
        Assert.Equal(accent.R, tint.R);
    }

    [Fact]
    public void AccentSelectionBorder_uses_semi_transparent_accent()
    {
        var accent = Color.FromRgb(30, 215, 96);
        var border = SkyColorHarmony.AccentSelectionBorder(accent);
        Assert.Equal(0x66, border.A);
    }
}
