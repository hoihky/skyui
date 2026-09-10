namespace SkyUI.Controls;

/// <summary>Async typeahead data source for <see cref="SkyAutocomplete"/>.</summary>
public interface ISkyAutocompleteProvider
{
    Task<IReadOnlyList<SkyAutocompleteItem>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default);
}
