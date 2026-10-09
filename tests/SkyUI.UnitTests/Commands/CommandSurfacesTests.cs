using System.Windows.Input;
using SkyUI.Controls;

namespace SkyUI.UnitTests.Commands;

public sealed class CommandSurfacesTests
{
    [Fact]
    public void DefaultCommandPaletteFilter_matches_title_subtitle_and_keywords()
    {
        var filter = DefaultCommandPaletteFilter.Instance;
        var item = new SkyCommandPaletteItem
        {
            Title = "Save project",
            Subtitle = "Write to disk",
            Keywords = "export backup",
        };

        Assert.True(filter.Matches(item, "save"));
        Assert.True(filter.Matches(item, "disk"));
        Assert.True(filter.Matches(item, "backup"));
        Assert.False(filter.Matches(item, "print"));
    }

    [Fact]
    public void DefaultCommandPaletteFilter_empty_query_matches_visible_items()
    {
        var filter = DefaultCommandPaletteFilter.Instance;
        var item = new SkyCommandPaletteItem { Title = "Open" };
        Assert.True(filter.Matches(item, null));
        Assert.True(filter.Matches(item, "   "));
    }

    [Fact]
    public void DefaultCommandPaletteFilter_hides_invisible_items()
    {
        var filter = DefaultCommandPaletteFilter.Instance;
        var item = new SkyCommandPaletteItem { Title = "Hidden", IsVisible = false };
        Assert.False(filter.Matches(item, null));
    }

    [Fact]
    public void SkyCommandPalette_rebuilds_filtered_items_when_search_changes()
    {
        var palette = new SkyCommandPalette();
        palette.Items.Add(new SkyCommandPaletteItem { Title = "Alpha" });
        palette.Items.Add(new SkyCommandPaletteItem { Title = "Beta" });
        palette.SearchText = "be";

        Assert.Single(palette.FilteredItems.Cast<SkyCommandPaletteItem>());
        Assert.Equal("Beta", ((SkyCommandPaletteItem)palette.FilteredItems[0]!).Title);
    }

    [Fact]
    public void SkyCommandPalette_open_close_toggles_is_open()
    {
        var palette = new SkyCommandPalette();
        palette.Open();
        Assert.True(palette.IsOpen);
        palette.Close();
        Assert.False(palette.IsOpen);
    }

    [Fact]
    public void SkyCommandPalette_executes_command_and_raises_event()
    {
        var palette = new SkyCommandPalette();
        var executed = false;
        palette.Items.Add(new SkyCommandPaletteItem
        {
            Title = "Run",
            Command = new RelayCommand(() => executed = true),
        });
        var item = (SkyCommandPaletteItem)palette.FilteredItems[0]!;
        palette.Execute(item);

        Assert.True(executed);
    }

    [Fact]
    public void SkyCommandPalette_custom_filter_is_used()
    {
        var palette = new SkyCommandPalette
        {
            Filter = new PrefixOnlyFilter(),
        };
        palette.Items.Add(new SkyCommandPaletteItem { Title = "Save" });
        palette.Items.Add(new SkyCommandPaletteItem { Title = "Open" });
        palette.SearchText = "Sa";
        Assert.Single(palette.FilteredItems.Cast<SkyCommandPaletteItem>());
    }

    [Fact]
    public void SkyCommandPalette_items_source_populates_items()
    {
        var palette = new SkyCommandPalette
        {
            ItemsSource = new[]
            {
                new SkyCommandPaletteItem { Title = "One" },
                new SkyCommandPaletteItem { Title = "Two" },
            },
        };
        Assert.Equal(2, palette.Items.Count);
    }

    [Fact]
    public void SkyDropDownButton_exposes_item_collection()
    {
        var button = new SkyDropDownButton();
        button.Items.Add(new SkyDropDownMenuItem { Label = "One" });
        Assert.Single(button.Items.Cast<SkyDropDownMenuItem>());
    }

    [Fact]
    public void SkyDropDownButton_items_source_populates_menu_items()
    {
        var button = new SkyDropDownButton
        {
            ItemsSource = new[] { new SkyDropDownMenuItem { Label = "A" } },
        };
        Assert.Single(button.Items.Cast<SkyDropDownMenuItem>());
    }

    [Fact]
    public void SkySplitButton_primary_command_property_round_trips()
    {
        var command = new RelayCommand(() => { });
        var button = new SkySplitButton { PrimaryCommand = command };
        Assert.Same(command, button.PrimaryCommand);
    }

    [Fact]
    public void SkyNotificationCenter_push_increments_unread_count()
    {
        var center = new SkyNotificationCenter();
        center.Push("Hello");
        center.Push("World");
        Assert.Equal(2, center.UnreadCount);
    }

    [Fact]
    public void SkyNotificationCenter_mark_read_reduces_unread_count()
    {
        var center = new SkyNotificationCenter();
        center.Push("One");
        var id = center.Notifications[0].Id;
        center.MarkRead(id);
        Assert.Equal(0, center.UnreadCount);
        Assert.True(center.Notifications[0].IsRead);
    }

    [Fact]
    public void SkyNotificationCenter_mark_all_read_clears_unread()
    {
        var center = new SkyNotificationCenter();
        center.Push("A");
        center.Push("B");
        center.MarkAllRead();
        Assert.Equal(0, center.UnreadCount);
    }

    [Fact]
    public void SkyNotificationCenter_dismiss_removes_entry()
    {
        var center = new SkyNotificationCenter();
        center.Push("Remove me");
        var id = center.Notifications[0].Id;
        center.Dismiss(id);
        Assert.Empty(center.Notifications);
    }

    [Fact]
    public void SkyNotificationCenter_clear_all_empties_collection()
    {
        var center = new SkyNotificationCenter();
        center.Push("A");
        center.ClearAll();
        Assert.Empty(center.Notifications);
        Assert.Equal(0, center.UnreadCount);
    }

    [Fact]
    public void SkyNotificationCenter_push_ignores_blank_title()
    {
        var center = new SkyNotificationCenter();
        center.Push("  ");
        Assert.Empty(center.Notifications);
    }

    [Fact]
    public void SkyNotificationEntry_assigns_unique_id_by_default()
    {
        var a = new SkyNotificationEntry();
        var b = new SkyNotificationEntry();
        Assert.NotEqual(a.Id, b.Id);
    }

    private sealed class PrefixOnlyFilter : ICommandPaletteFilter
    {
        public bool Matches(SkyCommandPaletteItem item, string? searchText) =>
            item.IsVisible
            && !string.IsNullOrEmpty(searchText)
            && item.Title.StartsWith(searchText, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class RelayCommand : ICommand
    {
        private readonly Action execute;

        public RelayCommand(Action execute) => this.execute = execute;

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => execute();
    }
}
