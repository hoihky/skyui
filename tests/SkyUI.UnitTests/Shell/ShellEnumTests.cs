using SkyUI.Controls;

namespace SkyUI.UnitTests.Shell;

public class ShellEnumTests
{
    [Fact]
    public void SkySplitViewDisplayMode_has_inline_and_overlay()
    {
        Assert.Equal(0, (int)SkySplitViewDisplayMode.Inline);
        Assert.Equal(1, (int)SkySplitViewDisplayMode.Overlay);
    }

    [Fact]
    public void SkySplitViewPanePlacement_has_left_and_right()
    {
        Assert.Equal(0, (int)SkySplitViewPanePlacement.Left);
        Assert.Equal(1, (int)SkySplitViewPanePlacement.Right);
    }

    [Fact]
    public void SkyDrawerPlacement_has_left_and_right()
    {
        Assert.Equal(0, (int)SkyDrawerPlacement.Left);
        Assert.Equal(1, (int)SkyDrawerPlacement.Right);
    }
}
