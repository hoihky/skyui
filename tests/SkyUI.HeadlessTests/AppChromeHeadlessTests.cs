using SkyUI.Controls;
using SkyUI.Icons;

namespace SkyUI.HeadlessTests;

public class AppChromeHeadlessTests
{
    [Fact]
    public void SkySplitView_assigns_pane_and_detail_content()
    {
        var splitView = new SkySplitView
        {
            PaneContent = "List",
            DetailContent = "Editor",
        };

        Assert.Equal("List", splitView.PaneContent);
        Assert.Equal("Editor", splitView.DetailContent);
    }

    [Fact]
    public void SkyCommandBar_title_property_roundtrips()
    {
        var bar = new SkyCommandBar { Title = "Inventory" };
        Assert.Equal("Inventory", bar.Title);
    }

    [Fact]
    public void SkyStatusBar_left_and_right_content_roundtrip()
    {
        var statusBar = new SkyStatusBar
        {
            LeftContent = "Connected",
            RightContent = "Ready",
        };

        Assert.Equal("Connected", statusBar.LeftContent);
        Assert.Equal("Ready", statusBar.RightContent);
    }

    [Fact]
    public void SkyPageHeader_back_visibility_defaults_false()
    {
        var header = new SkyPageHeader();
        Assert.False(header.IsBackButtonVisible);
    }

    [Fact]
    public void SkyDrawer_closed_event_can_be_raised()
    {
        var drawer = new SkyDrawer();
        var closed = false;
        drawer.Closed += (_, _) => closed = true;

        drawer.Show();
        drawer.Close();

        Assert.True(closed);
    }

    [Fact]
    public void SkyCommandBarItem_icon_kind_roundtrips()
    {
        var item = new SkyCommandBarItem { IconKind = SkyIconKind.Filter };
        Assert.Equal(SkyIconKind.Filter, item.IconKind);
    }
}
