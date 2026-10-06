namespace SkyUI.Controls.Timeline.Editing;

/// <summary>Creates editable clip copies for paste and clipboard operations.</summary>
public sealed class TimelineClipPrototypeFactory
{
    public TimelineClipItem CreateFrom(TimelineClipItem source) =>
        new()
        {
            TrackId = source.TrackId,
            StartTime = source.StartTime,
            Duration = source.Duration,
            Label = source.Label,
            Tag = source.Tag,
            Sprite = source.Sprite?.Clone(),
        };
}
