using System.Collections.ObjectModel;
using SkyUI.Controls.Professional;

namespace SkyUI.UnitTests.Professional;

public sealed class SkyDocumentWellControllerTests
{
    [Fact]
    public void Try_add_returns_false_when_items_source_not_mutable()
    {
        var controller = new SkyDocumentWellController
        {
            ItemsSource = new[] { new SkyDocument("A") },
        };

        Assert.False(controller.TryAdd(new SkyDocument("B")));
    }

    [Fact]
    public void Try_move_returns_false_for_out_of_range_indices()
    {
        var controller = new SkyDocumentWellController
        {
            ItemsSource = new ObservableCollection<ISkyDocument> { new SkyDocument("Only") },
        };

        Assert.False(controller.TryMove(-1, 0));
        Assert.False(controller.TryMove(0, 5));
    }

    [Fact]
    public void Snapshot_ignores_non_document_items()
    {
        var controller = new SkyDocumentWellController
        {
            ItemsSource = new ObservableCollection<object>
            {
                new SkyDocument("Doc"),
                "noise",
                42,
            },
        };

        Assert.Single(controller.Documents);
        Assert.Equal("Doc", controller.Documents[0].Title);
    }

    [Fact]
    public void Index_of_returns_negative_one_when_missing()
    {
        var controller = new SkyDocumentWellController
        {
            ItemsSource = new ObservableCollection<ISkyDocument> { new SkyDocument("A") },
        };

        Assert.Equal(-1, controller.IndexOf(new SkyDocument("B")));
    }

    [Fact]
    public void Contains_reflects_index_lookup()
    {
        var doc = new SkyDocument("A");
        var controller = new SkyDocumentWellController
        {
            ItemsSource = new ObservableCollection<ISkyDocument> { doc },
        };

        Assert.True(controller.Contains(doc));
        Assert.False(controller.Contains(new SkyDocument("Other")));
    }
}
