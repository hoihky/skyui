using SkyUI.Controls.Timeline.Model;

namespace SkyUI.Controls.Timeline.Rendering;

internal sealed class TimelineTrackHeaderFormatter
{
    public string Format(TimelineTrack track)
    {
        var name = string.IsNullOrEmpty(track.Name) ? "Track" : track.Name;
        if (!track.IsVisible)
            name += " (hidden)";
        if (track.IsLocked)
            name += " [locked]";
        return name;
    }
}
