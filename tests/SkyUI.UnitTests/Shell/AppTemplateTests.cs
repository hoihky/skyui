using SkyUI.Controls;

namespace SkyUI.UnitTests.Shell;

public class AppTemplateTests
{
    [Fact]
    public void App_template_pages_instantiate()
    {
        Assert.NotNull(new SkySettingsPage());
        Assert.NotNull(new SkyListDetailPage());
        Assert.NotNull(new SkyFormPage());
    }

    [Fact]
    public void SkySettingsPage_has_sky_classes()
    {
        var page = new SkySettingsPage();
        Assert.Contains("sky", page.Classes);
        Assert.Contains("sky-settings-page", page.Classes);
    }

    [Fact]
    public void SkyListDetailPage_has_sky_classes()
    {
        var page = new SkyListDetailPage();
        Assert.Contains("sky", page.Classes);
        Assert.Contains("sky-list-detail-page", page.Classes);
    }

    [Fact]
    public void SkyFormPage_has_sky_classes()
    {
        var page = new SkyFormPage();
        Assert.Contains("sky", page.Classes);
        Assert.Contains("sky-form-page", page.Classes);
    }

    [Fact]
    public void SkySettingsPage_footer_and_commands_roundtrip()
    {
        var page = new SkySettingsPage
        {
            SaveButtonText = "Apply",
            ResetButtonText = "Discard",
            IsFooterVisible = true,
            NavigationContent = "Nav",
            FooterContent = "Footer",
        };

        Assert.Equal("Apply", page.SaveButtonText);
        Assert.Equal("Discard", page.ResetButtonText);
        Assert.True(page.IsFooterVisible);
        Assert.Equal("Nav", page.NavigationContent);
        Assert.Equal("Footer", page.FooterContent);
    }

    [Fact]
    public void SkyFormPage_form_content_uses_content_attribute()
    {
        var page = new SkyFormPage
        {
            Title = "Create item",
            Subtitle = "Required fields marked",
            FormContent = "Fields",
        };

        Assert.Equal("Create item", page.Title);
        Assert.Equal("Required fields marked", page.Subtitle);
        Assert.Equal("Fields", page.FormContent);
    }

    [Fact]
    public void SkyListDetailPage_exposes_split_view_and_drawer_properties()
    {
        var page = new SkyListDetailPage
        {
            Title = "Orders",
            Subtitle = "Fulfillment queue",
            CommandBarTitle = "Operations",
            PaneContent = "List",
            DetailContent = "Detail",
            IsPaneOpen = true,
            OpenPaneLength = 240,
            DisplayMode = SkySplitViewDisplayMode.Inline,
            StatusText = "Ready",
            IsDrawerOpen = false,
            DrawerTitle = "Filters",
            DrawerPlacement = SkyDrawerPlacement.Right,
            DrawerWidth = 300,
        };

        Assert.Equal("Orders", page.Title);
        Assert.Equal("Operations", page.CommandBarTitle);
        Assert.Equal("List", page.PaneContent);
        Assert.Equal(240, page.OpenPaneLength);
        Assert.Equal("Filters", page.DrawerTitle);
        Assert.Equal(300, page.DrawerWidth);
    }

    [Fact]
    public void SkyListDetailPage_accepts_primary_commands()
    {
        var page = new SkyListDetailPage();
        page.PrimaryCommands.Add(new SkyCommandBarItem { Label = "New" });

        Assert.Single(page.PrimaryCommands);
    }

    [Fact]
    public void SkyListDetailPage_drawer_closed_syncs_is_drawer_open()
    {
        var page = new SkyListDetailPage { IsDrawerOpen = true };
        page.IsDrawerOpen = false;

        Assert.False(page.IsDrawerOpen);
    }

    [Fact]
    public void SkyListDetailPage_is_drawer_open_defaults_false()
    {
        var page = new SkyListDetailPage();
        Assert.False(page.IsDrawerOpen);
    }

    [Fact]
    public void SkyListDetailPage_is_drawer_open_can_be_toggled()
    {
        var page = new SkyListDetailPage();
        page.IsDrawerOpen = true;
        Assert.True(page.IsDrawerOpen);
        page.IsDrawerOpen = false;
        Assert.False(page.IsDrawerOpen);
    }
}
