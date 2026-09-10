using System.Text;

namespace SkyUI.DataGrid;

/// <summary>Simple CSV exporter (RFC-style quoting). Uses reflection on <see cref="IVirtualGridDataSource.GetRow"/> payloads when no template.</summary>
public sealed class SkyDataGridCsvExporter : ISkyDataGridExporter
{
    public async Task ExportAsync(
        IVirtualGridDataSource dataSource,
        IReadOnlyList<SkyDataGridColumn> columns,
        Stream destination,
        long startIndex,
        long maxRows,
        CancellationToken cancellationToken = default)
    {
        await using var writer = new StreamWriter(destination, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true), bufferSize: 65536, leaveOpen: true);
        var header = string.Join(",", columns.Select(static c => Escape(c.Header)));
        await writer.WriteLineAsync(header).ConfigureAwait(false);

        var end = Math.Min(dataSource.RowCount, startIndex + maxRows);
        for (var i = startIndex; i < end; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var row = dataSource.GetRow(i);
            var cells = new string[columns.Count];
            for (var c = 0; c < columns.Count; c++)
                cells[c] = Escape(SkyDataGridCellFormatter.FormatCell(row, columns[c]));

            await writer.WriteLineAsync(string.Join(",", cells)).ConfigureAwait(false);
        }
    }

    private static string Escape(string? s)
    {
        if (string.IsNullOrEmpty(s))
            return "\"\"";
        if (s.Contains('"', StringComparison.Ordinal) || s.Contains(',', StringComparison.Ordinal) || s.Contains('\n', StringComparison.Ordinal) || s.Contains('\r', StringComparison.Ordinal))
            return $"\"{s.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        return s;
    }
}
