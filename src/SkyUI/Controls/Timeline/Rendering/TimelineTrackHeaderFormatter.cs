using SkyUI.Controls.Timeline.Model;

namespace SkyUI.Controls.Timeline.Rendering;

internal sealed class TimelineTrackHeaderFormatter
{
    public string Format(TimelineTrack track)
    {
        var name = FormatNameOnly(track);
        if (!track.IsVisible)
            name += " (hidden)";
        if (track.IsLocked)
            name += " [locked]";
        return name;
    }

    public string FormatNameOnly(TimelineTrack track) =>
        string.IsNullOrEmpty(track.Name) ? "Track" : track.Name;
}
