using SkyUI.Controls;
using SkyUI.Controls.Timeline.Model;

namespace SkyUI.Controls.Timeline.Time;

/// <summary>
/// Owns frame quantization, ruler formatting, and snap grid alignment for the active time unit.
/// </summary>
public sealed class TimelineTimePresentation
{
    private readonly TimelineFrameQuantizer frameQuantizer = new();
    private readonly TimelineRulerTickPlanner tickPlanner = new();
    private readonly SecondsRulerLabelFormatter secondsLabels = new();
    private FrameRulerLabelFormatter? frameLabels;
    private TimecodeRulerLabelFormatter? timecodeLabels;

    public TimelineTimeUnit TimeUnit { get; set; } = TimelineTimeUnit.Seconds;

    public bool PreferTimecodeLabels { get; set; }

    public ITimelineFrameQuantizer FrameQuantizer => frameQuantizer;

    public double FramesPerSecond
    {
        get => frameQuantizer.FramesPerSecond;
        set => frameQuantizer.FramesPerSecond = value;
    }

    public bool UsesFrameQuantization =>
        TimeUnit == TimelineTimeUnit.Frames && frameQuantizer.IsActive;

    public ITimelineRulerLabelFormatter RulerLabels =>
        UsesFrameQuantization
            ? PreferTimecodeLabels
                ? timecodeLabels ??= new TimecodeRulerLabelFormatter(frameQuantizer)
                : frameLabels ??= new FrameRulerLabelFormatter(frameQuantizer)
            : secondsLabels;

    public double QuantizeTime(double timeSeconds) =>
        UsesFrameQuantization ? frameQuantizer.QuantizeSeconds(timeSeconds) : timeSeconds;

    public int PlayheadFrame(double playheadSeconds) => frameQuantizer.ToFrame(playheadSeconds);

    public double FrameToTime(int frame) => frameQuantizer.ToSeconds(frame);

    public double MinClipDurationSeconds => UsesFrameQuantization
        ? frameQuantizer.MinClipDurationSeconds
        : 0.08;

    public double RulerTickStepSeconds(double durationSeconds, double widthPixels) =>
        tickPlanner.ComputeTickStepSeconds(TimeUnit, frameQuantizer, durationSeconds, widthPixels);

    public void ApplyFrameSnapTo(TimelineSnapSettings snapSettings)
    {
        if (!UsesFrameQuantization)
            return;
        snapSettings.SnapToGrid = true;
        snapSettings.GridIntervalSeconds = frameQuantizer.FrameDurationSeconds;
    }
}
