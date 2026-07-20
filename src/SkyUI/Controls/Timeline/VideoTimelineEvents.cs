namespace SkyUI.Controls;

public sealed class TimelineTimeEventArgs : EventArgs
{
    public TimelineTimeEventArgs(double timeSeconds) => TimeSeconds = timeSeconds;
    public double TimeSeconds { get; }
}

public sealed class TimelineRangeEventArgs : EventArgs
{
    public TimelineRangeEventArgs(TimelineTimeRange range) => Range = range;
    public TimelineTimeRange Range { get; }
}

public sealed class TimelineClipEventArgs : EventArgs
{
    public TimelineClipEventArgs(TimelineClipItem clip) => Clip = clip;
    public TimelineClipItem Clip { get; }
}

public sealed class TimelineClipsEventArgs : EventArgs
{
    public TimelineClipsEventArgs(IReadOnlyList<TimelineClipItem> clips) => Clips = clips;
    public IReadOnlyList<TimelineClipItem> Clips { get; }
}

public sealed class TimelineMarkerEventArgs : EventArgs
{
    public TimelineMarkerEventArgs(TimelineMarkerItem marker) => Marker = marker;
    public TimelineMarkerItem Marker { get; }
}

public sealed class TimelineTrackReorderEventArgs : EventArgs
{
    public TimelineTrackReorderEventArgs(TimelineTrackItem track, int oldIndex, int newIndex)
    {
        Track = track;
        OldIndex = oldIndex;
        NewIndex = newIndex;
    }

    public TimelineTrackItem Track { get; }
    public int OldIndex { get; }
    public int NewIndex { get; }
}

public sealed class TimelineTrackEventArgs : EventArgs
{
    public TimelineTrackEventArgs(TimelineTrackItem track) => Track = track;
    public TimelineTrackItem Track { get; }
}
