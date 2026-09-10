using Avalonia.Controls;
using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class SkyTouchTargetTests
{
    [Fact]
    public void EnsureTouchTarget_sets_minimum_size()
    {
        var button = new Button { MinWidth = 0, MinHeight = 0 };
        SkyTouchTarget.SetEnsureTouchTarget(button, true);

        Assert.Equal(SkyTouchTarget.RecommendedSize, button.MinWidth);
        Assert.Equal(SkyTouchTarget.RecommendedSize, button.MinHeight);
    }

    [Fact]
    public void EnsureTouchTarget_respects_custom_minimum()
    {
        var button = new Button { MinWidth = 0, MinHeight = 0 };
        SkyTouchTarget.SetMinTouchSize(button, 52);
        SkyTouchTarget.SetEnsureTouchTarget(button, true);

        Assert.Equal(52, button.MinWidth);
        Assert.Equal(52, button.MinHeight);
    }
}
