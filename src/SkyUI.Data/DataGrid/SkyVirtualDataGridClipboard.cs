using SkyUI.Controls;

namespace SkyUI.DataGrid;

/// <summary>Clipboard helpers for <see cref="SkyVirtualDataGrid"/>.</summary>
public static class SkyVirtualDataGridClipboard
{
    public static async Task CopySelectionAsync(
        SkyVirtualDataGrid grid,
        bool includeHeaders = true,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var rows = BuildSelectionRows(grid, includeHeaders);
        if (rows.Count == 0)
            return;

        await SkyClipboard.SetTabularAsync(rows, grid).ConfigureAwait(false);
    }

    public static IReadOnlyList<IReadOnlyList<string?>> BuildSelectionRows(
        SkyVirtualDataGrid grid,
        bool includeHeaders = true)
    {
        if (grid.DataSource is null || grid.Columns.Count == 0)
            return Array.Empty<IReadOnlyList<string?>>();

        var rows = new List<IReadOnlyList<string?>>();
        if (includeHeaders)
            rows.Add(grid.Columns.Select(static column => column.Header).ToArray());

        if (grid.SelectedRowIndex is not long rowIndex)
            return rows;

        var row = grid.DataSource.GetRow(rowIndex);
        if (row is null)
            return rows;

        rows.Add(grid.Columns.Select(column => SkyDataGridCellFormatter.FormatCell(row, column)).ToArray());
        return rows;
    }
}
