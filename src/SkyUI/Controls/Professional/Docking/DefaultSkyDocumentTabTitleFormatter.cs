namespace SkyUI.Controls.Professional;

/// <summary>Appends <paramref name="dirtySuffix"/> when <see cref="ISkyDocument.IsDirty"/> is true.</summary>
public sealed class DefaultSkyDocumentTabTitleFormatter : ISkyDocumentTabTitleFormatter
{
    public string Format(ISkyDocument document, string dirtySuffix)
    {
        ArgumentNullException.ThrowIfNull(document);
        var title = string.IsNullOrWhiteSpace(document.Title) ? "Untitled" : document.Title.Trim();
        return document.IsDirty ? title + dirtySuffix : title;
    }
}
