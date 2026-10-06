namespace SkyUI.Controls.Timeline.Integration;

/// <summary>Reads transport state from a bound <see cref="VideoTimeline"/>.</summary>
public sealed class TimelineMediaClock : ITimelineMediaClock
{
    private VideoTimeline? timeline;

    public void Bind(VideoTimeline control) => timeline = control;

    public void Unbind() => timeline = null;

    public double FramesPerSecond => timeline?.Fps ?? 24;

    public bool IsPlaying => timeline?.IsPlaying ?? false;

    public int CurrentFrame => timeline?.PlayheadFrame ?? 0;

    public double CurrentTimeSeconds => timeline?.PlayheadTime ?? 0;

    public TimeSpan FrameDuration
    {
        get
        {
            var fps = FramesPerSecond;
            return fps > 0 ? TimeSpan.FromSeconds(1.0 / fps) : TimeSpan.FromMilliseconds(33);
        }
    }
}
