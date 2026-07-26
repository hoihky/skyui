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
}
