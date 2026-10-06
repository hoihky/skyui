using Avalonia.Media;

namespace SkyUI.Controls.Timeline.Rendering;

internal sealed class TimelineAccentColorParser
{
    public bool TryParseBrush(string? value, out IBrush? brush)
    {
        brush = null;
        if (string.IsNullOrWhiteSpace(value))
            return false;
        var text = value.Trim();
        if (!text.StartsWith('#'))
            text = "#" + text;
        if (!Color.TryParse(text, out var color))
            return false;
        brush = new SolidColorBrush(color);
        return true;
    }
}
