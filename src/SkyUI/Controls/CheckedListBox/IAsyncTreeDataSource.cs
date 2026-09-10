namespace SkyUI.Controls;

/// <summary>Lazy tree data source: children load when a node is expanded.</summary>
public interface IAsyncTreeDataSource
{
    bool HasChildren(object? item);

    bool AreChildrenLoaded(object? item);

    Task<IReadOnlyList<object?>> LoadChildrenAsync(object? item, CancellationToken cancellationToken = default);

    void ApplyLoadedChildren(object? item, IReadOnlyList<object?> children);
}
