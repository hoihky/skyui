namespace SkyUI.Controls.Timeline.Frame;

/// <summary>Inclusive start frame and duration in timeline frames.</summary>
public readonly struct TimelineClipFrameSpan
{
    public TimelineClipFrameSpan(int startFrame, int durationFrames)
    {
        StartFrame = startFrame;
        DurationFrames = durationFrames < 1 ? 1 : durationFrames;
    }

    public int StartFrame { get; }

    public int DurationFrames { get; }

    public int EndFrameExclusive => StartFrame + DurationFrames;

    public bool ContainsFrame(int frame) => frame >= StartFrame && frame < EndFrameExclusive;
}
