using System.Collections;

namespace SkyUI.Controls;

/// <summary>
/// Maps arbitrary model instances to hierarchy/check/expand state (DIP: CheckedListBox depends on this, not concrete VMs).
/// </summary>
public interface ICheckedListItemAdapter
{
    bool? GetIsChecked(object? item);

    void SetIsChecked(object? item, bool? value);

    bool GetIsExpanded(object? item);

    void SetIsExpanded(object? item, bool value);

    /// <summary>Returns child items for the given parent, or null/empty if leaf.</summary>
    IEnumerable<object?> GetChildren(object? item);

    bool HasChildren(object? item);
}
