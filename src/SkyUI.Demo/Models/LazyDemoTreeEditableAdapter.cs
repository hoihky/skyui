using SkyUI.Controls;

namespace SkyUI.Demo.Models;

public sealed class LazyDemoTreeEditableAdapter : ICheckedListEditableAdapter
{
    public bool CanEdit(object? item) => item is LazyDemoTreeNode;

    public string GetEditText(object? item) =>
        item is LazyDemoTreeNode node ? node.Title : string.Empty;

    public bool TryCommitEdit(object? item, string text, out string? error)
    {
        if (item is not LazyDemoTreeNode node)
        {
            error = "Item is not editable.";
            return false;
        }

        node.Title = text.Trim();
        error = null;
        return true;
    }
}
