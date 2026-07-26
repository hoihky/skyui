namespace SkyUI.DataGrid;

/// <summary>
/// Windowed data access for a virtual grid: only visible rows call <see cref="GetRow"/>; no materialized list required.
/// Implementations should be fast and thread-safe for reads if background threads call <see cref="SkyVirtualDataGrid.Post"/> to marshal UI refresh.
/// </summary>
public interface IVirtualGridDataSource
{
    /// <summary>Total logical rows (may be very large).</summary>
    long RowCount { get; }

    /// <summary>Resolve the row payload for binding (called on the UI thread by the grid).</summary>
    object? GetRow(long index);

    /// <summary>Raised when <see cref="RowCount"/> or bulk content invalidates visible rows.</summary>
    event EventHandler? StructureChanged;

    /// <summary>
    /// Apply sort from the grid (after column <see cref="SkyDataGridColumn.SortDirection"/> is updated).
    /// Default: no-op. Implement for in-memory or server-backed ordering, then raise <see cref="StructureChanged"/>.
    /// </summary>
    void ApplySort(SkyDataGridColumn? column, SkyDataGridSortDirection direction) { }
}
