namespace SkyUI.Controls.Professional;

/// <summary>Formats tab header text for a document (strategy for dirty markers and localization).</summary>
public interface ISkyDocumentTabTitleFormatter
{
    string Format(ISkyDocument document, string dirtySuffix);
}
