using SkyUI.Controls;

namespace SkyUI.Controls.Timeline.Frame;

/// <summary>Maps clip timeline seconds to frame indices using the active FPS.</summary>
public interface ITimelineClipFrameMapper
{
    TimelineClipFrameSpan DescribeClip(TimelineClipItem clip);

    int ClipStartFrame(TimelineClipItem clip);

    int ClipDurationFrames(TimelineClipItem clip);

    double FrameToSeconds(int frame);

    int SecondsToFrame(double seconds);
}
