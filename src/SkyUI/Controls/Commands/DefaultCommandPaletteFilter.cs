namespace SkyUI.Controls;

/// <summary>Case-insensitive match across title, subtitle, and keywords.</summary>
public sealed class DefaultCommandPaletteFilter : ICommandPaletteFilter
{
    public static DefaultCommandPaletteFilter Instance { get; } = new();

    public bool Matches(SkyCommandPaletteItem item, string? searchText)
    {
        if (!item.IsVisible)
            return false;
        if (string.IsNullOrWhiteSpace(searchText))
            return true;

        var query = searchText.Trim();
        return Contains(item.Title, query)
               || Contains(item.Subtitle, query)
               || Contains(item.Keywords, query);
    }

    private static bool Contains(string? source, string query) =>
        !string.IsNullOrEmpty(source)
        && source.Contains(query, StringComparison.OrdinalIgnoreCase);
}
