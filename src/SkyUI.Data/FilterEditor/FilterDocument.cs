using System.Collections.ObjectModel;

namespace SkyUI.FilterEditor;

/// <summary>Mutable filter tree + field catalog (document pattern; independent of Avalonia).</summary>
public sealed class FilterDocument
{
    public FilterDocument()
    {
        Root = new FilterGroupNode { LogicalKind = FilterLogicalKind.And };
    }

    public FilterGroupNode Root { get; }

    public ObservableCollection<FilterFieldDescriptor> Fields { get; } = new();

    /// <summary>Raised after any structural or semantic change worth re-running exporters / queries.</summary>
    public event EventHandler? StructureChanged;

    public void RaiseStructureChanged() => StructureChanged?.Invoke(this, EventArgs.Empty);
}
