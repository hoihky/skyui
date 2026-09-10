using SkyUI.Controls;

namespace SkyUI.UnitTests.Shell;

public class SkySplitViewTests
{
    [Fact]
    public void IsPaneOpen_false_sets_closed_pseudo_class()
    {
        var splitView = new SkySplitView();
        splitView.IsPaneOpen = false;

        Assert.Contains(":closed", splitView.Classes);
        Assert.DoesNotContain(":open", splitView.Classes);
    }

    [Fact]
    public void IsPaneOpen_true_sets_open_pseudo_class()
    {
        var splitView = new SkySplitView { IsPaneOpen = false };
        splitView.IsPaneOpen = true;

        Assert.Contains(":open", splitView.Classes);
        Assert.DoesNotContain(":closed", splitView.Classes);
    }

    [Fact]
    public void DisplayMode_overlay_sets_overlay_pseudo_class()
    {
        var splitView = new SkySplitView();
        splitView.DisplayMode = SkySplitViewDisplayMode.Overlay;

        Assert.Contains(":overlay", splitView.Classes);
        Assert.DoesNotContain(":inline", splitView.Classes);
    }

    [Fact]
    public void DisplayMode_inline_sets_inline_pseudo_class()
    {
        var splitView = new SkySplitView { DisplayMode = SkySplitViewDisplayMode.Overlay };
        splitView.DisplayMode = SkySplitViewDisplayMode.Inline;

        Assert.Contains(":inline", splitView.Classes);
    }

    [Theory]
    [InlineData(SkySplitViewPanePlacement.Right, ":right")]
    [InlineData(SkySplitViewPanePlacement.Left, ":left")]
    public void PanePlacement_sets_edge_pseudo_class(SkySplitViewPanePlacement placement, string expectedClass)
    {
        var splitView = new SkySplitView
        {
            PanePlacement = placement == SkySplitViewPanePlacement.Left
                ? SkySplitViewPanePlacement.Right
                : SkySplitViewPanePlacement.Left,
        };

        splitView.PanePlacement = placement;
        Assert.Contains(expectedClass, splitView.Classes);
    }

    [Fact]
    public void Pane_and_detail_content_roundtrip()
    {
        var splitView = new SkySplitView
        {
            PaneContent = "Pane",
            DetailContent = "Detail",
        };

        Assert.Equal("Pane", splitView.PaneContent);
        Assert.Equal("Detail", splitView.DetailContent);
    }

    [Fact]
    public void OpenPaneLength_updates_when_changed()
    {
        var splitView = new SkySplitView { OpenPaneLength = 360 };
        Assert.Equal(360, splitView.OpenPaneLength);
    }

    [Fact]
    public void Resize_properties_have_expected_defaults()
    {
        var splitView = new SkySplitView();

        Assert.True(splitView.IsPaneResizable);
        Assert.Equal(120, splitView.MinPaneLength);
        Assert.Equal(640, splitView.MaxPaneLength);
    }

    [Fact]
    public void IsPaneResizable_can_be_disabled()
    {
        var splitView = new SkySplitView { IsPaneResizable = false };
        Assert.False(splitView.IsPaneResizable);
    }

    [Fact]
    public void Min_and_max_pane_length_roundtrip()
    {
        var splitView = new SkySplitView
        {
            MinPaneLength = 160,
            MaxPaneLength = 480,
        };

        Assert.Equal(160, splitView.MinPaneLength);
        Assert.Equal(480, splitView.MaxPaneLength);
    }
}
