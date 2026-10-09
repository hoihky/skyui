using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace SkyUI.Controls.Professional;

/// <summary>Renders lightweight markdown markers used by <see cref="SkyRichTextBox"/>.</summary>
public static class SkyRichTextMarkdown
{
    public static void ApplyPreview(TextBlock target, string? markdown)
    {
        if (target.Inlines is null)
            return;
        target.Inlines.Clear();
        if (string.IsNullOrEmpty(markdown))
            return;

        var index = 0;
        while (index < markdown.Length)
        {
            if (TryReadDelimited(markdown, ref index, "**", out var bold))
            {
                AddRun(target, bold, FontWeight.SemiBold, FontStyle.Normal, underline: false);
                continue;
            }

            if (TryReadDelimited(markdown, ref index, "*", out var italic))
            {
                AddRun(target, italic, FontWeight.Normal, FontStyle.Italic, underline: false);
                continue;
            }

            if (TryReadDelimited(markdown, ref index, "_", out var underline))
            {
                AddRun(target, underline, FontWeight.Normal, FontStyle.Normal, underline: true);
                continue;
            }

            var plainStart = index;
            while (index < markdown.Length && !IsMarkerStart(markdown, index))
                index++;
            if (plainStart < index)
                AddRun(target, markdown[plainStart..index], FontWeight.Normal, FontStyle.Normal, underline: false);
            else
                index++;
        }
    }

    private static bool IsMarkerStart(string text, int index)
    {
        if (text.AsSpan(index).StartsWith("**"))
            return true;
        var c = text[index];
        return c is '*' or '_';
    }

    private static bool TryReadDelimited(string text, ref int index, string delimiter, out string inner)
    {
        inner = string.Empty;
        if (!text.AsSpan(index).StartsWith(delimiter))
            return false;

        var contentStart = index + delimiter.Length;
        var close = text.IndexOf(delimiter, contentStart, StringComparison.Ordinal);
        if (close < 0)
            return false;

        inner = text[contentStart..close];
        index = close + delimiter.Length;
        return true;
    }

    private static void AddRun(
        TextBlock target,
        string text,
        FontWeight weight,
        FontStyle style,
        bool underline)
    {
        if (text.Length == 0)
            return;
        target.Inlines!.Add(new Run
        {
            Text = text,
            FontWeight = weight,
            FontStyle = style,
            TextDecorations = underline ? TextDecorations.Underline : null,
        });
    }
}
