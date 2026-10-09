using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.VisualTree;
using SkyUI.Controls;

namespace SkyUI.HeadlessTests;

public class CommandsThemedHeadlessTests
{
    private static readonly HeadlessUnitTestSession Session =
        HeadlessUnitTestSession.GetOrStartForAssembly(typeof(CommandsThemedHeadlessTests).Assembly);

    [Fact]
    public void SkyCommandPalette_template_exposes_search_and_results()
    {
        Session.Dispatch(() =>
        {
            var (palette, window) = CreatePalette();
            palette.Open();
            Assert.True(palette.IsOpen);

            var search = palette.GetVisualDescendants().OfType<TextBox>()
                .FirstOrDefault(t => t.Name == SkyCommandPalette.SearchPartName);
            var results = palette.GetVisualDescendants().OfType<ListBox>()
                .FirstOrDefault(t => t.Name == SkyCommandPalette.ResultsPartName);
            Assert.NotNull(search);
            Assert.NotNull(results);
            window.Close();
        }, CancellationToken.None);
    }

    [Fact]
    public void SkyCommandPalette_filters_results_in_template()
    {
        Session.Dispatch(() =>
        {
            var (palette, window) = CreatePalette();
            palette.Open();
            palette.SearchText = "save";
            Assert.Single(palette.FilteredItems);
            window.Close();
        }, CancellationToken.None);
    }

    [Fact]
    public void SkyDropDownButton_applies_theme_anchor()
    {
        Session.Dispatch(() =>
        {
            var button = new SkyDropDownButton
            {
                Width = 160,
                Content = "Actions",
            };
            button.Items.Add(new SkyDropDownMenuItem { Label = "Export" });
            var window = Show(button);
            var anchor = button.GetVisualDescendants().OfType<Button>()
                .FirstOrDefault(b => b.Name == SkyDropDownButton.AnchorPartName);
            Assert.NotNull(anchor);
            window.Close();
        }, CancellationToken.None);
    }

    [Fact]
    public void SkySplitButton_applies_theme_primary_and_drop_down_parts()
    {
        Session.Dispatch(() =>
        {
            var split = new SkySplitButton
            {
                Width = 200,
                PrimaryContent = "Save",
            };
            split.Items.Add(new SkyDropDownMenuItem { Label = "Save as…" });
            var window = Show(split);
            Assert.NotNull(split.GetVisualDescendants()
                .OfType<Button>()
                .FirstOrDefault(b => b.Name == SkySplitButton.PrimaryPartName));
            Assert.NotNull(split.GetVisualDescendants()
                .OfType<Button>()
                .FirstOrDefault(b => b.Name == SkySplitButton.DropDownPartName));
            window.Close();
        }, CancellationToken.None);
    }

    [Fact]
    public void SkyNotificationCenter_template_shows_unread_badge_after_push()
    {
        Session.Dispatch(() =>
        {
            var center = new SkyNotificationCenter { Width = 360, Height = 400 };
            var window = Show(center);
            center.Push("Build finished");
            Assert.Equal(1, center.UnreadCount);
            var badge = center.GetVisualDescendants().OfType<Border>()
                .FirstOrDefault(t => t.Name == SkyNotificationCenter.UnreadBadgePartName);
            Assert.NotNull(badge);
            Assert.True(badge!.IsVisible);
            window.Close();
        }, CancellationToken.None);
    }

    [Fact]
    public void SkyNotificationCenter_panel_toggles_with_is_panel_open()
    {
        Session.Dispatch(() =>
        {
            var center = new SkyNotificationCenter { Width = 360, Height = 400 };
            var window = Show(center);
            center.IsPanelOpen = true;
            Assert.True(center.IsPanelOpen);
            window.Close();
        }, CancellationToken.None);
    }

    private static (SkyCommandPalette Palette, Window Window) CreatePalette()
    {
        var palette = new SkyCommandPalette { Width = 640, Height = 480 };
        palette.Items.Add(new SkyCommandPaletteItem { Title = "Open file", Keywords = "browse" });
        palette.Items.Add(new SkyCommandPaletteItem { Title = "Save file", Keywords = "disk" });
        var window = Show(palette);
        return (palette, window);
    }

    private static Window Show(Control content)
    {
        var window = new Window
        {
            Width = 800,
            Height = 600,
            Content = content,
        };
        window.Show();
        content.Measure(new Size(content.Width, content.Height));
        content.Arrange(new Rect(0, 0, content.Width, content.Height));
        return window;
    }
}
