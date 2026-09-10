using System.Collections;

namespace SkyUI.Controls;

/// <summary>MVVM helper to apply sibling reorder results to live collections.</summary>
public static class CheckedListReorderHelper
{
    public static bool TryReorderSibling(
        IEnumerable? roots,
        object? sourceItem,
        object? targetItem,
        CheckedListDropPosition position,
        object? sharedParent,
        ICheckedListItemAdapter adapter)
    {
        if (sourceItem is null || targetItem is null)
            return false;

        var siblings = ResolveSiblingList(roots, sharedParent, adapter);
        if (siblings is null)
            return false;

        var sourceIndex = IndexOf(siblings, sourceItem);
        var targetIndex = IndexOf(siblings, targetItem);
        if (sourceIndex < 0 || targetIndex < 0)
            return false;

        siblings.RemoveAt(sourceIndex);
        var insertIndex = position == CheckedListDropPosition.Before ? targetIndex : targetIndex + 1;
        if (sourceIndex < insertIndex)
            insertIndex--;

        insertIndex = Math.Clamp(insertIndex, 0, siblings.Count);
        siblings.Insert(insertIndex, sourceItem);
        return true;
    }

    private static IList? ResolveSiblingList(IEnumerable? roots, object? sharedParent, ICheckedListItemAdapter adapter)
    {
        if (sharedParent is null)
            return ToMutableList(roots);

        if (sharedParent is ICheckedListBoxItem node && node.Children is IList list)
            return list;

        return ToMutableList(adapter.GetChildren(sharedParent));
    }

    private static int IndexOf(IList siblings, object item)
    {
        for (var index = 0; index < siblings.Count; index++)
        {
            if (ReferenceEquals(siblings[index], item))
                return index;
        }

        return -1;
    }

    private static IList? ToMutableList(IEnumerable? source) =>
        source as IList;
}
