using SkyUI.Controls.Timeline.Layout;
using SkyUI.Controls.Timeline.Time;

namespace SkyUI.Controls.Timeline.Transport;

/// <summary>Frame stepping, playhead quantization, and playback advance for the timeline.</summary>
public sealed class TimelineTransportController
{
    private readonly TimelinePlaybackLoop playbackLoop = new();
    private readonly TimelineTimePresentation timePresentation;

    public TimelineTransportController(TimelineTimePresentation timePresentation) =>
        this.timePresentation = timePresentation;

    public TimelinePlaybackLoop PlaybackLoop => playbackLoop;

    public void ConfigureLoopRegion(bool useTimeRangeLoop, TimelineTimeRange range)
    {
        playbackLoop.IsEnabled = useTimeRangeLoop && range.Length > 0;
        if (playbackLoop.IsEnabled)
            playbackLoop.SetFromRange(range);
    }

    public double QuantizePlayhead(double timeSeconds, double durationSeconds)
    {
        var clamped = Math.Clamp(timeSeconds, 0, durationSeconds);
        return timePresentation.QuantizeTime(clamped);
    }

    public double StepPlayheadByFrames(double currentSeconds, int frameDelta, double durationSeconds)
    {
        if (frameDelta == 0)
            return currentSeconds;

        if (timePresentation.UsesFrameQuantization)
        {
            var frame = timePresentation.PlayheadFrame(currentSeconds) + frameDelta;
            if (frame < 0)
                frame = 0;
            var seconds = timePresentation.FrameToTime(frame);
            return Math.Clamp(seconds, 0, durationSeconds);
        }

        var step = frameDelta / Math.Max(timePresentation.FramesPerSecond, 1);
        return QuantizePlayhead(currentSeconds + step, durationSeconds);
    }

    public double AdvancePlayback(double currentSeconds, double stepSeconds, double durationSeconds)
    {
        var next = playbackLoop.Advance(currentSeconds, stepSeconds, durationSeconds);
        next = Math.Clamp(next, 0, durationSeconds);
        return timePresentation.QuantizeTime(next);
    }

    public TimeSpan PlaybackTickInterval
    {
        get
        {
            if (timePresentation.UsesFrameQuantization)
                return TimeSpan.FromSeconds(timePresentation.FrameQuantizer.FrameDurationSeconds);
            return TimeSpan.FromMilliseconds(33);
        }
    }

    public double PlaybackStepSeconds =>
        timePresentation.UsesFrameQuantization
            ? timePresentation.FrameQuantizer.FrameDurationSeconds
            : PlaybackTickInterval.TotalSeconds;
}
