using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class NavigationTests
{
    [Theory]
    [InlineData(1280, SkyNavigationDisplayMode.Expanded)]
    [InlineData(1024, SkyNavigationDisplayMode.Expanded)]
    [InlineData(900, SkyNavigationDisplayMode.Compact)]
    [InlineData(768, SkyNavigationDisplayMode.Compact)]
    [InlineData(600, SkyNavigationDisplayMode.Bottom)]
    [InlineData(360, SkyNavigationDisplayMode.Bottom)]
    public void ResolveNavigationDisplayMode_matches_breakpoints(double width, SkyNavigationDisplayMode expected) =>
        Assert.Equal(expected, SkyBreakpoint.ResolveNavigationDisplayMode(width));

    [Fact]
    public void Navigation_controls_instantiate()
    {
        Assert.NotNull(new SkyNavigationView());
        Assert.NotNull(new SkyTabView());
        Assert.NotNull(new SkyBreadcrumb());
        Assert.NotNull(new SkyNavigationViewItem { Label = "Test" });
    }
}
