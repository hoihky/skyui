using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class SkyGridBreakpointTests
{
    [Theory]
    [InlineData(320, 1)]
    [InlineData(500, 1)]
    [InlineData(600, 2)]
    [InlineData(800, 3)]
    [InlineData(950, 4)]
    [InlineData(1100, 5)]
    [InlineData(1400, 5)]
    public void ResolveColumns_uses_design_breakpoints(double width, int expected) =>
        Assert.Equal(expected, SkyGridBreakpoint.ResolveColumns(width));

    [Fact]
    public void ResolveColumns_honors_custom_profile()
    {
        var profile = new SkyResponsiveColumnProfile
        {
            MobileSmall = 2,
            Mobile = 2,
            Tablet = 3,
            TabletLarge = 3,
            DesktopSmall = 4,
            Desktop = 4,
            LargeDesktop = 6,
        };

        Assert.Equal(6, SkyGridBreakpoint.ResolveColumns(1500, profile));
        Assert.Equal(2, SkyGridBreakpoint.ResolveColumns(400, profile));
    }
}
