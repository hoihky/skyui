using SkyUI.Controls.Timeline.Frame;

namespace SkyUI.Controls.Timeline.Editing;

/// <summary>Adjusts sprite clip duration in whole-frame hold steps.</summary>
public sealed class TimelineClipHoldEditor
{
    private readonly ITimelineClipFrameMapper frameMapper;

    public TimelineClipHoldEditor(ITimelineClipFrameMapper frameMapper) =>
        this.frameMapper = frameMapper;

    public double ComputeDurationAfterHoldDelta(
        TimelineClipItem clip,
        int deltaFrames,
        double timelineDuration)
    {
        if (deltaFrames == 0)
            return clip.Duration;

        var span = frameMapper.DescribeClip(clip);
        var newDurationFrames = Math.Max(1, span.DurationFrames + deltaFrames);
        var endFrame = span.StartFrame + newDurationFrames;
        var endTime = frameMapper.FrameToSeconds(endFrame);
        var maxDuration = Math.Max(0, timelineDuration - clip.StartTime);
        var startFrame = frameMapper.ClipStartFrame(clip);
        var oneFrame = frameMapper.FrameToSeconds(startFrame + 1) - frameMapper.FrameToSeconds(startFrame);
        if (oneFrame <= 0)
            oneFrame = 1.0 / 24;
        return Math.Max(oneFrame, Math.Min(endTime - clip.StartTime, maxDuration));
    }

    public void ApplyHoldDelta(TimelineClipItem clip, int deltaFrames, double timelineDuration)
    {
        clip.Duration = ComputeDurationAfterHoldDelta(clip, deltaFrames, timelineDuration);
        if (clip.Sprite is not null)
            clip.Sprite.HoldFrames = frameMapper.DescribeClip(clip).DurationFrames;
    }
}
