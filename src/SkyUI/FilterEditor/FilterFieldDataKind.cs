namespace SkyUI.FilterEditor;

/// <summary>Semantic type of a bindable field (drives editor UI and validation; OCP: extend enum or use string tags later).</summary>
public enum FilterFieldDataKind
{
    String,
    Number,
    Boolean,
    DateTime,
    Guid,
    Other,
}
