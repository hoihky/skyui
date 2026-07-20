namespace SkyUI.DataGrid;

/// <summary>Export pipeline (CSV, Excel, etc.) without coupling the grid to a file format.</summary>
public interface ISkyDataGridExporter
{
    /// <summary>Export up to <paramref name="maxRows"/> rows starting at <paramref name="startIndex"/>.</summary>
    Task ExportAsync(
        IVirtualGridDataSource dataSource,
        IReadOnlyList<SkyDataGridColumn> columns,
        Stream destination,
        long startIndex,
        long maxRows,
        CancellationToken cancellationToken = default);
}
