using System.Collections;

namespace SkyUI.Controls;

/// <summary>
/// Default adapter for <see cref="ICheckedListBoxItem"/> (LSP: replace with a custom adapter for other models).
/// </summary>
public sealed class DefaultCheckedListItemAdapter : ICheckedListItemAdapter
{
    public bool? GetIsChecked(object? item) =>
        item is ICheckedListBoxItem i ? i.IsChecked : false;

    public void SetIsChecked(object? item, bool? value)
    {
        if (item is ICheckedListBoxItem i)
            i.IsChecked = value;
    }

    public bool GetIsExpanded(object? item) =>
        item is ICheckedListBoxItem i ? i.IsExpanded : true;

    public void SetIsExpanded(object? item, bool value)
    {
        if (item is ICheckedListBoxItem i)
            i.IsExpanded = value;
    }

    public IEnumerable<object?> GetChildren(object? item)
    {
        if (item is not ICheckedListBoxItem i || i.Children == null)
            yield break;
        foreach (var c in i.Children)
            yield return c;
    }

    public bool HasChildren(object? item)
    {
        if (item is not ICheckedListBoxItem i || i.Children == null)
            return false;
        foreach (var _ in i.Children)
            return true;
        return false;
    }
}
