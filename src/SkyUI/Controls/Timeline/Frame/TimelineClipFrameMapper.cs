using SkyUI.Controls;
using SkyUI.Controls.Timeline.Time;

namespace SkyUI.Controls.Timeline.Frame;

public sealed class TimelineClipFrameMapper : ITimelineClipFrameMapper
{
    private readonly TimelineTimePresentation timePresentation;

    public TimelineClipFrameMapper(TimelineTimePresentation timePresentation) =>
        this.timePresentation = timePresentation;

    public TimelineClipFrameSpan DescribeClip(TimelineClipItem clip)
    {
        var start = ClipStartFrame(clip);
        var end = SecondsToFrame(clip.StartTime + clip.Duration);
        var duration = Math.Max(1, end - start);
        return new TimelineClipFrameSpan(start, duration);
    }

    public int ClipStartFrame(TimelineClipItem clip) => SecondsToFrame(clip.StartTime);

    public int ClipDurationFrames(TimelineClipItem clip) => DescribeClip(clip).DurationFrames;

    public double FrameToSeconds(int frame) => timePresentation.FrameToTime(frame);

    public int SecondsToFrame(double seconds) => timePresentation.FrameQuantizer.ToFrame(seconds);
}
