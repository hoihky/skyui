using SkyUI.Controls;

namespace SkyUI.HeadlessTests;

public class NavigationHeadlessTests
{
    [Fact]
    public void SkyNavigationView_selected_item_sets_content()
    {
        var nav = new SkyNavigationView();
        var item = new SkyNavigationViewItem { Label = "Home", Content = "Home page" };

        nav.SelectedItem = item;

        Assert.Equal(item, nav.SelectedItem);
        Assert.Equal("Home page", nav.Content);
    }

    [Fact]
    public void SkyBreadcrumb_separator_defaults_to_slash()
    {
        var breadcrumb = new SkyBreadcrumb();
        Assert.Equal("/", breadcrumb.Separator);

        breadcrumb.Separator = ">";
        Assert.Equal(">", breadcrumb.Separator);
    }
}
