using System.Collections.ObjectModel;

namespace SkyUI.Controls.Timeline.Model;

/// <summary>Root timeline document: tracks, clips, markers, and transport settings.</summary>
public sealed class TimelineProject
{
    public ObservableCollection<TimelineTrackItem> Tracks { get; set; } = new();

    public ObservableCollection<TimelineClipItem> Clips { get; set; } = new();

    public ObservableCollection<TimelineMarkerItem> Markers { get; set; } = new();

    public ObservableCollection<TimelineKeyframeItem> Keyframes { get; set; } = new();

    public double Duration { get; set; } = 120;

    public TimelineTimeUnit TimeUnit { get; set; } = TimelineTimeUnit.Seconds;

    public double Fps { get; set; } = 30;
}
