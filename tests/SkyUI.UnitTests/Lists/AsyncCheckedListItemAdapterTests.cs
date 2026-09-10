using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class AsyncCheckedListItemAdapterTests
{
    [Fact]
    public void HasChildren_before_load_uses_async_source()
    {
        var node = new TestLazyNode("Root", hasLazyChildren: true);
        var source = new TestAsyncTreeDataSource();
        var adapter = new AsyncCheckedListItemAdapter(new DefaultCheckedListItemAdapter(), source);

        Assert.True(adapter.HasChildren(node));
        Assert.False(source.AreChildrenLoaded(node));
        Assert.Empty(adapter.GetChildren(node));
    }

    [Fact]
    public async Task ApplyLoadedChildren_exposes_children_after_load()
    {
        var node = new TestLazyNode("Root", hasLazyChildren: true);
        var source = new TestAsyncTreeDataSource();
        var adapter = new AsyncCheckedListItemAdapter(new DefaultCheckedListItemAdapter(), source);
        var children = await source.LoadChildrenAsync(node);

        source.ApplyLoadedChildren(node, children);

        Assert.True(source.AreChildrenLoaded(node));
        Assert.Equal(2, adapter.GetChildren(node).Count());
    }

    private sealed class TestLazyNode : ICheckedListBoxItem
    {
        public TestLazyNode(string title, bool hasLazyChildren = false)
        {
            Title = title;
            HasLazyChildren = hasLazyChildren;
            ChildNodes = new ObservableCollection<TestLazyNode>();
        }

        public string Title { get; set; }

        public bool HasLazyChildren { get; }

        public bool ChildrenLoaded { get; set; }

        public ObservableCollection<TestLazyNode> ChildNodes { get; }

        public bool? IsChecked { get; set; }

        public bool IsExpanded { get; set; }

        IEnumerable? ICheckedListBoxItem.Children => ChildNodes;

#pragma warning disable CS0067
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
    }

    private sealed class TestAsyncTreeDataSource : IAsyncTreeDataSource
    {
        public bool HasChildren(object? item) =>
            item is TestLazyNode node && (node.HasLazyChildren || node.ChildNodes.Count > 0);

        public bool AreChildrenLoaded(object? item) =>
            item is TestLazyNode node && (!node.HasLazyChildren || node.ChildrenLoaded);

        public Task<IReadOnlyList<object?>> LoadChildrenAsync(object? item, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<object?>>([new TestLazyNode("Child A"), new TestLazyNode("Child B")]);

        public void ApplyLoadedChildren(object? item, IReadOnlyList<object?> children)
        {
            if (item is not TestLazyNode node)
                return;

            node.ChildNodes.Clear();
            foreach (var child in children.OfType<TestLazyNode>())
                node.ChildNodes.Add(child);
            node.ChildrenLoaded = true;
        }
    }
}
