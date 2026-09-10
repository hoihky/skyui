using SkyUI.Controls;

namespace SkyUI.UnitTests.Shell;

public class SkyDrawerTests
{
    [Fact]
    public void Show_and_close_toggle_is_open()
    {
        var drawer = new SkyDrawer();

        drawer.Show();
        Assert.True(drawer.IsOpen);

        drawer.Close();
        Assert.False(drawer.IsOpen);
    }

    [Fact]
    public void Show_after_close_reopens_drawer()
    {
        var drawer = new SkyDrawer();

        drawer.Show();
        drawer.Close();
        drawer.Show();

        Assert.True(drawer.IsOpen);
    }

    [Fact]
    public void Close_raises_closed_event_when_not_templated()
    {
        var drawer = new SkyDrawer();
        var closed = false;
        drawer.Closed += (_, _) => closed = true;

        drawer.Show();
        drawer.Close();

        Assert.True(closed);
    }

    [Fact]
    public void Close_is_idempotent()
    {
        var drawer = new SkyDrawer();
        drawer.Close();
        drawer.Close();
        Assert.False(drawer.IsOpen);
    }

    [Fact]
    public void Placement_left_sets_style_class()
    {
        var drawer = new SkyDrawer { Placement = SkyDrawerPlacement.Right };
        drawer.Placement = SkyDrawerPlacement.Left;

        Assert.Contains("sky-drawer-left", drawer.Classes);
    }

    [Fact]
    public void Placement_right_sets_style_class()
    {
        var drawer = new SkyDrawer();
        drawer.Placement = SkyDrawerPlacement.Right;

        Assert.Contains("sky-drawer-right", drawer.Classes);
    }

    [Fact]
    public void Does_not_block_hits_when_closed()
    {
        var drawer = new SkyDrawer();
        Assert.False(drawer.IsHitTestVisible);

        drawer.Show();
        Assert.True(drawer.IsOpen);

        drawer.Close();
        Assert.False(drawer.IsHitTestVisible);
    }

    [Fact]
    public void Open_does_not_block_hits_before_template_is_applied()
    {
        var drawer = new SkyDrawer();
        drawer.Show();

        Assert.True(drawer.IsOpen);
        Assert.False(drawer.IsHitTestVisible);
    }

    [Fact]
    public void Drawer_content_and_title_roundtrip()
    {
        var drawer = new SkyDrawer
        {
            Title = "Filters",
            DrawerContent = "Filter form",
            DrawerWidth = 400,
        };

        Assert.Equal("Filters", drawer.Title);
        Assert.Equal("Filter form", drawer.DrawerContent);
        Assert.Equal(400, drawer.DrawerWidth);
    }

    [Fact]
    public void Close_while_not_open_does_not_raise_closed_event()
    {
        var drawer = new SkyDrawer();
        var closedCount = 0;
        drawer.Closed += (_, _) => closedCount++;

        drawer.Close();

        Assert.Equal(0, closedCount);
    }
}
