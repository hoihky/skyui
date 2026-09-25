namespace SkyUI.Controls.Timeline.Editing;

/// <summary>Pure clip mutations used by the control and undo commands.</summary>
public static class TimelineClipOperations
{
    public static TimelineClipItem ClonePrototype(TimelineClipItem source) =>
        new()
        {
            TrackId = source.TrackId,
            StartTime = source.StartTime,
            Duration = source.Duration,
            Label = source.Label,
            Tag = source.Tag,
        };

    /// <summary>
    /// Splits a clip at <paramref name="splitTimeSeconds"/> (timeline time).
    /// Returns the new right-hand clip, or null if split is not valid.
    /// </summary>
    public static TimelineClipItem? SplitAt(
        TimelineClipItem clip,
        double splitTimeSeconds,
        ICollection<TimelineClipItem> clips)
    {
        var min = TimelineCoordinateSystem.MinClipDurationSeconds;
        var start = clip.StartTime;
        var end = start + clip.Duration;
        if (splitTimeSeconds <= start + min || splitTimeSeconds >= end - min)
            return null;

        var rightDuration = end - splitTimeSeconds;
        clip.Duration = splitTimeSeconds - start;

        var right = new TimelineClipItem
        {
            TrackId = clip.TrackId,
            StartTime = splitTimeSeconds,
            Duration = rightDuration,
            Label = string.IsNullOrEmpty(clip.Label) ? "Clip" : $"{clip.Label} (2)",
            Tag = clip.Tag,
        };
        clips.Add(right);
        return right;
    }

    public static bool IntersectsTimeRange(TimelineClipItem clip, double minTime, double maxTime)
    {
        var clipStart = clip.StartTime;
        var clipEnd = clip.StartTime + clip.Duration;
        return clipEnd > minTime && clipStart < maxTime;
    }

    public static bool IntersectsMarquee(
        TimelineClipItem clip,
        int trackRow,
        double minTime,
        double maxTime,
        int minRow,
        int maxRow)
    {
        if (trackRow < minRow || trackRow > maxRow)
            return false;
        return IntersectsTimeRange(clip, minTime, maxTime);
    }
}
