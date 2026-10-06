namespace SkyUI.Controls.Timeline.Integration;

/// <summary>Frame-accurate playback clock surfaced from <see cref="VideoTimeline"/>.</summary>
public interface ITimelineMediaClock
{
    double FramesPerSecond { get; }

    bool IsPlaying { get; }

    int CurrentFrame { get; }

    double CurrentTimeSeconds { get; }

    TimeSpan FrameDuration { get; }
}
