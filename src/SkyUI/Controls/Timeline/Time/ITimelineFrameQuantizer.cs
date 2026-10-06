namespace SkyUI.Controls.Timeline.Time;

/// <summary>Converts between timeline seconds and discrete animation frames.</summary>
public interface ITimelineFrameQuantizer
{
    double FramesPerSecond { get; }

    bool IsActive { get; }

    double FrameDurationSeconds { get; }

    int ToFrame(double timeSeconds);

    double ToSeconds(int frameIndex);

    double QuantizeSeconds(double timeSeconds);

    double MinClipDurationSeconds { get; }
}
