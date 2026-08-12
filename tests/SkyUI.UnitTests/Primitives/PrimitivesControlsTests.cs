using Avalonia.Controls;
using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class PrimitivesControlsTests
{
    [Fact]
    public void Primitive_controls_instantiate()
    {
        Assert.NotNull(new SkyTooltip());
        Assert.NotNull(new SkyPopover());
    }

    [Fact]
    public void SkyTooltip_has_sky_class() =>
        Assert.Contains("sky", new SkyTooltip().Classes);

    [Fact]
    public void SkyPopover_adds_presenter_sky_class() =>
        Assert.Contains("sky", new SkyPopover().FlyoutPresenterClasses);

    [Fact]
    public void SkyTooltipProperties_sets_sky_tooltip()
    {
        var button = new Button();
        SkyTooltipProperties.SetTip(button, "Help text");

        var tip = ToolTip.GetTip(button);
        Assert.IsType<SkyTooltip>(tip);
        Assert.Equal("Help text", ((SkyTooltip)tip).Content);
    }

    [Fact]
    public void SkyButtonProperties_loading_replaces_content_and_disables_button()
    {
        var button = new Button { Content = "Save", IsEnabled = true };
        SkyButtonProperties.SetIsLoading(button, true);

        Assert.Contains("sky-loading", button.Classes);
        Assert.False(button.IsEnabled);
        Assert.IsType<SkyProgressRing>(button.Content);

        SkyButtonProperties.SetIsLoading(button, false);

        Assert.DoesNotContain("sky-loading", button.Classes);
        Assert.Equal("Save", button.Content);
        Assert.True(button.IsEnabled);
    }

    [Fact]
    public void SkyButtonProperties_loading_preserves_disabled_state_when_cleared()
    {
        var button = new Button { Content = "Save", IsEnabled = false };
        SkyButtonProperties.SetIsLoading(button, true);
        SkyButtonProperties.SetIsLoading(button, false);

        Assert.False(button.IsEnabled);
    }
}
