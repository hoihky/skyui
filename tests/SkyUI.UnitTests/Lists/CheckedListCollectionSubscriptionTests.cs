using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class CheckedListCollectionSubscriptionTests
{
    [Fact]
    public void Reordering_nested_collection_updates_visible_rows()
    {
        var paris = new TestNode("Paris");
        var berlin = new TestNode("Berlin");
        var europe = new TestNode("Europe", new ObservableCollection<TestNode> { paris, berlin });
        var roots = new ObservableCollection<TestNode> { europe };
        var list = new CheckedListBox
        {
            ItemsSource = roots,
            ItemComparer = null,
        };

        list.RebuildAll();
        Assert.Equal("Paris", GetVisibleTitle(list, 1));

        europe.Children.RemoveAt(0);
        europe.Children.Insert(1, paris);
        list.RebuildAll();

        Assert.Equal("Berlin", GetVisibleTitle(list, 1));
        Assert.Equal("Paris", GetVisibleTitle(list, 2));
    }

    [Fact]
    public void ReorderRequested_rebuilds_visible_rows_after_handler_mutates_collection()
    {
        var paris = new TestNode("Paris");
        var berlin = new TestNode("Berlin");
        var amsterdam = new TestNode("Amsterdam");
        var europe = new TestNode("Europe", new ObservableCollection<TestNode> { paris, berlin, amsterdam });
        var roots = new ObservableCollection<TestNode> { europe };
        var list = new CheckedListBox { ItemsSource = roots, ItemComparer = null };
        list.RebuildAll();

        var adapter = new DefaultCheckedListItemAdapter();
        list.ReorderRequested += (_, e) =>
            CheckedListReorderHelper.TryReorderSibling(roots, e.SourceItem, e.TargetItem, e.Position, e.SharedParent, adapter);

        list.RaiseReorderRequested(new CheckedListReorderEventArgs(amsterdam, paris, CheckedListDropPosition.Before, europe));

        Assert.Equal("Amsterdam", GetVisibleTitle(list, 1));
        Assert.Equal("Paris", GetVisibleTitle(list, 2));
        Assert.Equal("Berlin", GetVisibleTitle(list, 3));
    }

    private static string GetVisibleTitle(CheckedListBox list, int index)
    {
        var row = list.Rows[index];
        return row.Item is TestNode node ? node.Title : string.Empty;
    }

    private sealed class TestNode : ICheckedListBoxItem
    {
        public TestNode(string title, ObservableCollection<TestNode>? children = null)
        {
            Title = title;
            Children = children ?? new ObservableCollection<TestNode>();
        }

        public string Title { get; set; } = string.Empty;

        public ObservableCollection<TestNode> Children { get; }

        public bool? IsChecked { get; set; }

        public bool IsExpanded { get; set; } = true;

        IEnumerable? ICheckedListBoxItem.Children => Children;

#pragma warning disable CS0067
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
    }
}
