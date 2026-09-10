using SkyUI.Controls;
using SkyUI.Icons;

namespace SkyUI.UnitTests.Shell;

public class AppChromeControlsTests
{
    [Fact]
    public void App_chrome_controls_instantiate()
    {
        Assert.NotNull(new SkyCommandBar());
        Assert.NotNull(new SkyCommandBarItem());
        Assert.NotNull(new SkyStatusBar());
        Assert.NotNull(new SkySplitView());
        Assert.NotNull(new SkyDrawer());
        Assert.NotNull(new SkyPageHeader());
    }

    [Fact]
    public void SkyCommandBar_has_sky_classes()
    {
        var bar = new SkyCommandBar();
        Assert.Contains("sky", bar.Classes);
        Assert.Contains("sky-command-bar", bar.Classes);
    }

    [Fact]
    public void SkyStatusBar_has_sky_classes()
    {
        var statusBar = new SkyStatusBar();
        Assert.Contains("sky", statusBar.Classes);
        Assert.Contains("sky-status-bar", statusBar.Classes);
    }

    [Fact]
    public void SkySplitView_has_sky_classes()
    {
        var splitView = new SkySplitView();
        Assert.Contains("sky", splitView.Classes);
        Assert.Contains("sky-split-view", splitView.Classes);
    }

    [Fact]
    public void SkyDrawer_has_sky_classes()
    {
        var drawer = new SkyDrawer();
        Assert.Contains("sky", drawer.Classes);
        Assert.Contains("sky-drawer", drawer.Classes);
    }

    [Fact]
    public void SkyPageHeader_has_sky_classes()
    {
        var header = new SkyPageHeader();
        Assert.Contains("sky", header.Classes);
        Assert.Contains("sky-page-header", header.Classes);
    }

    [Fact]
    public void SkyCommandBarItem_exposes_bindable_properties()
    {
        var item = new SkyCommandBarItem
        {
            Label = "Save",
            IconKind = SkyIconKind.Check,
            ToolTip = "Save changes",
            IsVisible = true,
        };

        Assert.Equal("Save", item.Label);
        Assert.Equal(SkyIconKind.Check, item.IconKind);
        Assert.Equal("Save changes", item.ToolTip);
        Assert.True(item.IsVisible);
    }

    [Fact]
    public void SkyStatusBar_text_and_progress_properties_roundtrip()
    {
        var statusBar = new SkyStatusBar
        {
            Text = "3 items selected",
            Progress = 42,
        };

        Assert.Equal("3 items selected", statusBar.Text);
        Assert.Equal(42, statusBar.Progress);
    }

    [Fact]
    public void SkySplitView_defaults_to_inline_open_left_pane()
    {
        var splitView = new SkySplitView();

        Assert.True(splitView.IsPaneOpen);
        Assert.Equal(SkySplitViewDisplayMode.Inline, splitView.DisplayMode);
        Assert.Equal(SkySplitViewPanePlacement.Left, splitView.PanePlacement);
        Assert.Equal(280, splitView.OpenPaneLength);
    }

    [Fact]
    public void SkyDrawer_defaults_to_closed_left_placement()
    {
        var drawer = new SkyDrawer();

        Assert.False(drawer.IsOpen);
        Assert.False(drawer.IsHitTestVisible);
        Assert.Equal(SkyDrawerPlacement.Left, drawer.Placement);
        Assert.Equal(320, drawer.DrawerWidth);
    }

    [Fact]
    public void SkyPageHeader_title_and_subtitle_roundtrip()
    {
        var header = new SkyPageHeader
        {
            Title = "Customers",
            Subtitle = "Manage customer records",
            IsBackButtonVisible = true,
        };

        Assert.Equal("Customers", header.Title);
        Assert.Equal("Manage customer records", header.Subtitle);
        Assert.True(header.IsBackButtonVisible);
    }

    [Fact]
    public void SkyCommandBar_accepts_primary_commands()
    {
        var bar = new SkyCommandBar();
        bar.PrimaryCommands.Add(new SkyCommandBarItem { Label = "New" });
        bar.SecondaryCommands.Add(new SkyCommandBarItem { Label = "Refresh" });
        bar.OverflowCommands.Add(new SkyCommandBarItem { Label = "Export" });

        Assert.Single(bar.PrimaryCommands);
        Assert.Single(bar.SecondaryCommands);
        Assert.Single(bar.OverflowCommands);
    }
}
