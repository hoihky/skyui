using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class CheckedListReorderHelperTests
{
    [Fact]
    public void TryReorderSibling_moves_item_after_target()
    {
        var roots = new ObservableCollection<string> { "A", "B", "C" };
        var adapter = new DefaultCheckedListItemAdapter();

        var moved = CheckedListReorderHelper.TryReorderSibling(
            roots,
            "A",
            "C",
            CheckedListDropPosition.After,
            sharedParent: null,
            adapter);

        Assert.True(moved);
        Assert.Equal(["B", "C", "A"], roots);
    }

    [Fact]
    public void TryReorderSibling_moves_item_before_target()
    {
        var roots = new ObservableCollection<string> { "A", "B", "C" };
        var adapter = new DefaultCheckedListItemAdapter();

        var moved = CheckedListReorderHelper.TryReorderSibling(
            roots,
            "C",
            "A",
            CheckedListDropPosition.Before,
            sharedParent: null,
            adapter);

        Assert.True(moved);
        Assert.Equal(["C", "A", "B"], roots);
    }

    [Fact]
    public void TryReorderSibling_moves_nested_item_using_parent_children_list()
    {
        var europe = new TestNode("Europe", new ObservableCollection<TestNode>
        {
            new("Paris"),
            new("Berlin"),
            new("Amsterdam"),
        });
        var roots = new ObservableCollection<TestNode> { europe };
        var adapter = new DefaultCheckedListItemAdapter();

        var moved = CheckedListReorderHelper.TryReorderSibling(
            roots,
            europe.Children[2],
            europe.Children[0],
            CheckedListDropPosition.Before,
            europe,
            adapter);

        Assert.True(moved);
        Assert.Equal(["Amsterdam", "Paris", "Berlin"], europe.Children.Select(c => c.Title));
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
