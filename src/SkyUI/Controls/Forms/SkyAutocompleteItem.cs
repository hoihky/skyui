namespace SkyUI.Controls;

/// <summary>Suggestion row for <see cref="SkyAutocomplete"/>.</summary>
public sealed class SkyAutocompleteItem
{
    public SkyAutocompleteItem()
    {
    }

    public SkyAutocompleteItem(string text, object? value = null)
    {
        Text = text;
        Value = value ?? text;
    }

    public string? Text { get; set; }

    public object? Value { get; set; }

    public string? Description { get; set; }
}
