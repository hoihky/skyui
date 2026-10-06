namespace SkyUI.Controls.Timeline.Time;

/// <summary>Frame quantization using a fixed frames-per-second rate.</summary>
public sealed class TimelineFrameQuantizer : ITimelineFrameQuantizer
{
    public const double MinFps = 1;
    public const double MaxFps = 240;

    private double framesPerSecond;

    public TimelineFrameQuantizer(double framesPerSecond = 24) =>
        this.framesPerSecond = ClampFps(framesPerSecond);

    public double FramesPerSecond
    {
        get => framesPerSecond;
        set => this.framesPerSecond = ClampFps(value);
    }

    public bool IsActive => framesPerSecond > 0;

    public double FrameDurationSeconds => IsActive ? 1.0 / framesPerSecond : 0;

    public double MinClipDurationSeconds => IsActive ? FrameDurationSeconds : 0.08;

    public int ToFrame(double timeSeconds)
    {
        if (!IsActive || timeSeconds <= 0)
            return 0;
        return (int)Math.Round(timeSeconds * framesPerSecond, MidpointRounding.AwayFromZero);
    }

    public double ToSeconds(int frameIndex)
    {
        if (!IsActive || frameIndex <= 0)
            return 0;
        return frameIndex / framesPerSecond;
    }

    public double QuantizeSeconds(double timeSeconds)
    {
        if (!IsActive)
            return timeSeconds;
        var frame = ToFrame(timeSeconds);
        return ToSeconds(frame);
    }

    private static double ClampFps(double value) =>
        value < MinFps ? MinFps : value > MaxFps ? MaxFps : value;
}
