using Avalonia.Controls;
using SkyUI.Controls;

namespace SkyUI.HeadlessTests;

public class PrimitivesHeadlessTests
{
    [Fact]
    public void SkyButtonProperties_loading_indicator_measures()
    {
        var button = new Button { Content = "Save", Width = 120, Height = 40 };
        SkyButtonProperties.SetIsLoading(button, true);

        button.Measure(new Avalonia.Size(120, 40));
        button.Arrange(new Avalonia.Rect(0, 0, 120, 40));

        Assert.IsType<SkyProgressRing>(button.Content);
        Assert.False(button.IsEnabled);
    }

    [Fact]
    public void SkyTooltip_accepts_string_content()
    {
        var tooltip = new SkyTooltip { Content = "Details" };
        Assert.Equal("Details", tooltip.Content);
        Assert.Contains("sky", tooltip.Classes);
    }
}
