namespace SkyUI.Controls;

/// <summary>Decorates an item adapter with async expand-to-load behavior.</summary>
public sealed class AsyncCheckedListItemAdapter : ICheckedListItemAdapter
{
    public AsyncCheckedListItemAdapter(ICheckedListItemAdapter inner, IAsyncTreeDataSource asyncSource)
    {
        Inner = inner;
        AsyncSource = asyncSource;
    }

    public ICheckedListItemAdapter Inner { get; }

    public IAsyncTreeDataSource AsyncSource { get; }

    public bool? GetIsChecked(object? item) => Inner.GetIsChecked(item);

    public void SetIsChecked(object? item, bool? value) => Inner.SetIsChecked(item, value);

    public bool GetIsExpanded(object? item) => Inner.GetIsExpanded(item);

    public void SetIsExpanded(object? item, bool value) => Inner.SetIsExpanded(item, value);

    public IEnumerable<object?> GetChildren(object? item) =>
        AsyncSource.AreChildrenLoaded(item) ? Inner.GetChildren(item) : Array.Empty<object?>();

    public bool HasChildren(object? item) =>
        AsyncSource.HasChildren(item) || Inner.HasChildren(item);
}
