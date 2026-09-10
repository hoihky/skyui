using SkyUI.Controls;

namespace SkyUI.Demo.Models;

public sealed class DemoCheckedListEditableAdapter : ICheckedListEditableAdapter
{
    public bool CanEdit(object? item) => item is DemoCheckedListNode;

    public string GetEditText(object? item) =>
        item is DemoCheckedListNode node ? node.Title : string.Empty;

    public bool TryCommitEdit(object? item, string text, out string? error)
    {
        if (item is not DemoCheckedListNode node)
        {
            error = "Item is not editable.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            error = "Title is required.";
            return false;
        }

        node.Title = text.Trim();
        error = null;
        return true;
    }
}
