using SkyUI.Controls;

namespace SkyUI.Demo.Models;

public sealed class LazyDemoTreeDataSource : IAsyncTreeDataSource
{
    public bool HasChildren(object? item) =>
        item is LazyDemoTreeNode node && (node.HasLazyChildren || node.Children.Count > 0);

    public bool AreChildrenLoaded(object? item) =>
        item is LazyDemoTreeNode node && (!node.HasLazyChildren || node.ChildrenLoaded);

    public async Task<IReadOnlyList<object?>> LoadChildrenAsync(object? item, CancellationToken cancellationToken = default)
    {
        if (item is not LazyDemoTreeNode node)
            return Array.Empty<object?>();

        await Task.Delay(600, cancellationToken).ConfigureAwait(false);
        return
        [
            new LazyDemoTreeNode($"{node.Title} / Child A"),
            new LazyDemoTreeNode($"{node.Title} / Child B"),
            new LazyDemoTreeNode($"{node.Title} / Child C", hasLazyChildren: true),
        ];
    }

    public void ApplyLoadedChildren(object? item, IReadOnlyList<object?> children)
    {
        if (item is not LazyDemoTreeNode node)
            return;

        node.Children.Clear();
        foreach (var child in children)
        {
            if (child is LazyDemoTreeNode lazyChild)
                node.Children.Add(lazyChild);
        }

        node.ChildrenLoaded = true;
    }
}
