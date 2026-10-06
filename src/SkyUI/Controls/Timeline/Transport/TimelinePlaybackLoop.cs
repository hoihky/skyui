namespace SkyUI.Controls.Timeline.Transport;

/// <summary>Computes the next playhead time with optional loop in/out from a time range.</summary>
public sealed class TimelinePlaybackLoop
{
    public bool IsEnabled { get; set; }

    public double LoopStartSeconds { get; set; }

    public double LoopEndSeconds { get; set; }

    public bool HasValidLoop => LoopEndSeconds > LoopStartSeconds + 1e-9;

    public void SetFromRange(TimelineTimeRange range)
    {
        LoopStartSeconds = range.Min;
        LoopEndSeconds = range.Max;
    }

    public double Advance(double currentSeconds, double stepSeconds, double timelineDuration)
    {
        var next = currentSeconds + stepSeconds;
        if (!IsEnabled || !HasValidLoop)
            return next;

        if (next > LoopEndSeconds)
            return LoopStartSeconds + (next - LoopEndSeconds);

        if (next < LoopStartSeconds)
            return LoopEndSeconds - (LoopStartSeconds - next);

        return next;
    }

    public bool ShouldStopAtTimelineEnd(double nextSeconds, double timelineDuration) =>
        (!IsEnabled || !HasValidLoop) && nextSeconds >= timelineDuration - 1e-9;
}
