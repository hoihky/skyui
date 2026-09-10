namespace SkyUI.Controls;

/// <summary>Supplies per-row overflow actions (MVVM-friendly).</summary>
public interface ICheckedListRowActionProvider
{
    IReadOnlyList<CheckedListRowAction> GetActions(object? item);
}
