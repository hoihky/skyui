using System.Collections;
using Avalonia.Input;

namespace SkyUI.Controls;

/// <summary>
/// Builds parent links and flattened visible rows (SRP: tree indexing separate from UI control).
/// </summary>
internal static class CheckedListFlatIndex
{
    public static void RebuildParents(
        IEnumerable? roots,
        ICheckedListItemAdapter adapter,
        Dictionary<object, object?> parents)
    {
        parents.Clear();
        if (roots == null)
            return;
        foreach (var r in roots)
        {
            if (r is null)
                continue;
            parents[r] = null;
            IndexSubtree(r, adapter, parents);
        }
    }

    private static void IndexSubtree(object parent, ICheckedListItemAdapter adapter, Dictionary<object, object?> parents)
    {
        foreach (var c in adapter.GetChildren(parent))
        {
            if (c is null)
                continue;
            parents[c] = parent;
            IndexSubtree(c, adapter, parents);
        }
    }

    public static void AppendVisibleRows(
        IList<CheckedListRowModel> target,
        IEnumerable? roots,
        ICheckedListItemAdapter adapter,
        IComparer<object?>? comparer,
        Action requestRebuild,
        Action<CheckedListRowModel, bool?> checkCommitted,
        Action<CheckedListRowModel, PointerReleasedEventArgs> pointerPressed,
        Action<CheckedListRowModel>? expandRequested = null,
        Func<object?, bool>? isLoadingChildren = null)
    {
        target.Clear();
        if (roots == null)
            return;
        var list = new List<object?>();
        foreach (var x in roots)
            list.Add(x);
        SortInPlace(list, comparer);
        foreach (var item in list)
        {
            if (item is null)
                continue;
            AppendSubtree(target, item, 0, adapter, comparer, requestRebuild, checkCommitted, pointerPressed, expandRequested, isLoadingChildren);
        }
    }

    private static void AppendSubtree(
        IList<CheckedListRowModel> target,
        object item,
        int depth,
        ICheckedListItemAdapter adapter,
        IComparer<object?>? comparer,
        Action requestRebuild,
        Action<CheckedListRowModel, bool?> checkCommitted,
        Action<CheckedListRowModel, PointerReleasedEventArgs> pointerPressed,
        Action<CheckedListRowModel>? expandRequested,
        Func<object?, bool>? isLoadingChildren)
    {
        var has = adapter.HasChildren(item);
        var row = new CheckedListRowModel(item, depth, has, adapter, requestRebuild, checkCommitted, pointerPressed, expandRequested)
        {
            IsLoadingChildren = isLoadingChildren?.Invoke(item) ?? false,
        };
        target.Add(row);
        if (!has || !adapter.GetIsExpanded(item))
            return;
        var children = new List<object?>();
        foreach (var c in adapter.GetChildren(item))
            children.Add(c);
        SortInPlace(children, comparer);
        foreach (var c in children)
        {
            if (c is null)
                continue;
            AppendSubtree(target, c, depth + 1, adapter, comparer, requestRebuild, checkCommitted, pointerPressed, expandRequested, isLoadingChildren);
        }
    }

    private static void SortInPlace(List<object?> list, IComparer<object?>? comparer)
    {
        // Comparer<object?>.Default requires IComparable on the runtime types; arbitrary VM/item types often do not.
        if (comparer == null)
            return;
        list.Sort(comparer.Compare);
    }
}
