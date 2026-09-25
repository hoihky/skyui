namespace SkyUI.Controls;

/// <inheritdoc cref="Timeline.Model.TimelineTrack"/>
public sealed class TimelineTrackItem : Timeline.Model.TimelineTrack;

/// <inheritdoc cref="Timeline.Model.TimelineClip"/>
public sealed class TimelineClipItem : Timeline.Model.TimelineClip;

/// <inheritdoc cref="Timeline.Model.TimelineMarker"/>
public sealed class TimelineMarkerItem : Timeline.Model.TimelineMarker;

/// <summary>Inclusive-exclusive style range in seconds (either edge may be greater).</summary>
public readonly struct TimelineTimeRange
{
    public TimelineTimeRange(double a, double b)
    {
        Start = a;
        End = b;
    }

    public double Start { get; }

    public double End { get; }

    public double Min => Math.Min(Start, End);

    public double Max => Math.Max(Start, End);

    public double Length => Math.Max(0, Max - Min);
}
