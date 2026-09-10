using Avalonia.Controls;
using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class SkyPaginationTests
{
    [Fact]
    public void Has_sky_classes()
    {
        var pagination = new SkyPagination();
        Assert.Contains("sky", pagination.Classes);
        Assert.Contains("sky-pagination", pagination.Classes);
    }

    [Fact]
    public void Slices_items_for_current_page()
    {
        var pagination = new SkyPagination
        {
            ItemsSource = Enumerable.Range(1, 12).ToArray(),
            PageSize = 5,
            CurrentPage = 2
        };

        var slice = pagination.CurrentPageItems!.Cast<int>().ToArray();
        Assert.Equal([6, 7, 8, 9, 10], slice);
        Assert.Equal(3, pagination.PageCount);
        Assert.Equal(6, pagination.RangeStart);
        Assert.Equal(10, pagination.RangeEnd);
    }

    [Fact]
    public void Navigation_commands_update_current_page()
    {
        var pagination = new SkyPagination
        {
            ItemsSource = Enumerable.Range(1, 12).ToArray(),
            PageSize = 5,
            CurrentPage = 2
        };

        pagination.NextPageCommand.Execute(null);
        Assert.Equal(3, pagination.CurrentPage);

        pagination.LastPageCommand.Execute(null);
        Assert.Equal(3, pagination.CurrentPage);

        pagination.FirstPageCommand.Execute(null);
        Assert.Equal(1, pagination.CurrentPage);

        pagination.PreviousPageCommand.Execute(null);
        Assert.Equal(1, pagination.CurrentPage);
    }

    [Fact]
    public void PageChanged_event_fires_when_page_changes()
    {
        var pagination = new SkyPagination
        {
            ItemsSource = Enumerable.Range(1, 10).ToArray(),
            PageSize = 5
        };

        var count = 0;
        pagination.PageChanged += (_, _) => count++;

        pagination.GoToNext();

        Assert.Equal(1, count);
    }

    [Fact]
    public void Target_receives_current_page_items()
    {
        var pagination = new SkyPagination
        {
            ItemsSource = Enumerable.Range(1, 8).ToArray(),
            PageSize = 3,
            Target = new ListBox()
        };

        pagination.GoToLast();
        var targetItems = pagination.Target!.ItemsSource!.Cast<int>().ToArray();

        Assert.Equal([7, 8], targetItems);
    }

    [Fact]
    public void Changing_page_size_resets_to_first_page()
    {
        var pagination = new SkyPagination
        {
            ItemsSource = Enumerable.Range(1, 20).ToArray(),
            PageSize = 5,
            CurrentPage = 3
        };

        pagination.PageSize = 10;

        Assert.Equal(1, pagination.CurrentPage);
        Assert.Equal(10, pagination.CurrentPageItems!.Cast<int>().Count());
    }
}
