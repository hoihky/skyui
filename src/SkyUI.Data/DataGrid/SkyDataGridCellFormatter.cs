using System.Globalization;
using System.Reflection;

namespace SkyUI.DataGrid;

/// <summary>Shared cell text resolution for export and clipboard.</summary>
public static class SkyDataGridCellFormatter
{
    public static string FormatCell(object? row, SkyDataGridColumn column)
    {
        var value = ResolveValue(row, column.BindingPath);
        return FormatValue(value);
    }

    public static object? ResolveValue(object? row, string? bindingPath)
    {
        if (row is null || string.IsNullOrEmpty(bindingPath))
            return row;

        object? current = row;
        foreach (var segment in bindingPath.Split('.'))
        {
            if (current is null)
                return null;

            var property = current.GetType().GetProperty(
                segment,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            current = property?.GetValue(current);
        }

        return current;
    }

    public static string FormatValue(object? value) =>
        value switch
        {
            null => string.Empty,
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty,
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty,
        };
}
