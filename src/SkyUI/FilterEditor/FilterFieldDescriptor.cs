namespace SkyUI.FilterEditor;

/// <summary>Describes a filterable property on items in a bound collection (ISP: metadata separate from UI).</summary>
public sealed class FilterFieldDescriptor
{
    public FilterFieldDescriptor(string propertyPath, string displayName, FilterFieldDataKind dataKind)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        PropertyPath = propertyPath;
        DisplayName = displayName;
        DataKind = dataKind;
    }

    /// <summary>Path used for binding / serialization (e.g. <c>City</c> or <c>Address.Zip</c>).</summary>
    public string PropertyPath { get; }

    public string DisplayName { get; }

    public FilterFieldDataKind DataKind { get; }
}
