using System.Text;

namespace SkyUI.Controls;

/// <summary>Tab-separated value formatting for clipboard and grid export.</summary>
public static class SkyTabularFormat
{
    public static string ToTsv(IReadOnlyList<IReadOnlyList<string?>> rows)
    {
        var builder = new StringBuilder();
        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            if (rowIndex > 0)
                builder.AppendLine();

            var row = rows[rowIndex];
            for (var columnIndex = 0; columnIndex < row.Count; columnIndex++)
            {
                if (columnIndex > 0)
                    builder.Append('\t');

                builder.Append(EscapeCell(row[columnIndex]));
            }
        }

        return builder.ToString();
    }

    public static IReadOnlyList<string[]>? TryParseTsv(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return null;

        var normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        var lines = normalized.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length == 0)
            return null;

        var rows = new string[lines.Length][];
        for (var index = 0; index < lines.Length; index++)
            rows[index] = ParseRow(lines[index]);

        return rows;
    }

    private static string[] ParseRow(string line)
    {
        var cells = new List<string>();
        var builder = new StringBuilder();
        var inQuotes = false;

        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (character == '"')
            {
                if (inQuotes && index + 1 < line.Length && line[index + 1] == '"')
                {
                    builder.Append('"');
                    index++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (!inQuotes && character == '\t')
            {
                cells.Add(builder.ToString());
                builder.Clear();
                continue;
            }

            builder.Append(character);
        }

        cells.Add(builder.ToString());
        return cells.ToArray();
    }

    private static string EscapeCell(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains('\t', StringComparison.Ordinal)
            || value.Contains('\n', StringComparison.Ordinal)
            || value.Contains('\r', StringComparison.Ordinal)
            || value.Contains('"', StringComparison.Ordinal))
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }
}
