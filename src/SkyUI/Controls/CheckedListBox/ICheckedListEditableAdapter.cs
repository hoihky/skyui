namespace SkyUI.Controls;

/// <summary>Optional adapter extension for inline row editing.</summary>
public interface ICheckedListEditableAdapter
{
    bool CanEdit(object? item);

    string GetEditText(object? item);

    bool TryCommitEdit(object? item, string text, out string? error);
}
