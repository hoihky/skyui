namespace SkyUI.Controls.Timeline.Model;

/// <summary>
/// Keyframe on a property lane (Phase 6 sprite/video tooling).
/// Stored on the project for future extensibility; not yet rendered by <see cref="VideoTimeline"/>.
/// </summary>
public sealed class TimelineKeyframe
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    public string TrackId { get; set; } = "";

    public string PropertyName { get; set; } = "";

    public double Time { get; set; }

    public object? Value { get; set; }

    public object? Tag { get; set; }
}
