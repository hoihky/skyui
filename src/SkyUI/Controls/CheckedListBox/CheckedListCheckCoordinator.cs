using System.Linq;

namespace SkyUI.Controls;

/// <summary>
/// Tri-state aggregation and optional downward cascade (SRP: isolated from UI and flattening).
/// </summary>
internal static class CheckedListCheckCoordinator
{
    public static void ApplyAfterItemChanged(
        object? changedItem,
        bool? newValue,
        ICheckedListItemAdapter adapter,
        Dictionary<object, object?> parents,
        bool cascadeToChildren,
        bool threeStateParents)
    {
        if (changedItem is null)
            return;
        if (cascadeToChildren && newValue is true or false)
            CascadeDown(changedItem, newValue.Value, adapter);
        BubbleAncestors(changedItem, adapter, parents, threeStateParents);
    }

    private static void CascadeDown(object parent, bool value, ICheckedListItemAdapter adapter)
    {
        foreach (var c in adapter.GetChildren(parent))
        {
            if (c is null)
                continue;
            adapter.SetIsChecked(c, value);
            CascadeDown(c, value, adapter);
        }
    }

    private static void BubbleAncestors(
        object item,
        ICheckedListItemAdapter adapter,
        Dictionary<object, object?> parents,
        bool threeStateParents)
    {
        var current = parents.TryGetValue(item, out var p) ? p : null;
        while (current != null)
        {
            UpdateParentState(current, adapter, threeStateParents);
            current = parents.TryGetValue(current, out var pp) ? pp : null;
        }
    }

    private static void UpdateParentState(object parent, ICheckedListItemAdapter adapter, bool threeStateParents)
    {
        var ch = adapter.GetChildren(parent).Where(static x => x != null).Cast<object>().ToList();
        if (ch.Count == 0)
            return;
        var allTrue = ch.TrueForAll(x => adapter.GetIsChecked(x) == true);
        var allFalse = ch.TrueForAll(x => adapter.GetIsChecked(x) == false);
        bool? next;
        if (threeStateParents)
        {
            if (allTrue)
                next = true;
            else if (allFalse)
                next = false;
            else
                next = null;
        }
        else
        {
            next = allTrue ? true : false;
        }

        adapter.SetIsChecked(parent, next);
    }
}
