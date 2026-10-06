using SkyUI.Controls.Timeline.Model;
using SkyUI.Controls.Timeline.Time;

namespace SkyUI.Controls.Timeline.Layout;

/// <summary>Default layout engine composing coordinate math, time presentation, and snap.</summary>
public sealed class TimelineLayoutEngine : ITimelineLayoutEngine
{
    private readonly TimelineSnapEngine snapEngine = new();
    private readonly TimelineTimePresentation timePresentation = new();

    public double Duration { get; set; } = 120;

    public double PixelsPerSecond { get; set; } = 40;

    public TimelineTimePresentation TimePresentation => timePresentation;

    public TimelineSnapSettings SnapSettings => snapEngine.Settings;

    public double ContentWidth(double minimumWidth = 400) =>
        TimelineCoordinateSystem.ContentWidth(Duration, PixelsPerSecond, minimumWidth);

    public double TimeToPixel(double timeSeconds) =>
        TimelineCoordinateSystem.TimeToPixel(timeSeconds, PixelsPerSecond);

    public double PixelToTime(double pixelX) =>
        TimelineCoordinateSystem.PixelToTime(pixelX, PixelsPerSecond);

    public double PointerTimeToSeconds(double canvasX, double scrollOffsetX) =>
        TimelineCoordinateSystem.PointerTimeToSeconds(canvasX, scrollOffsetX, PixelsPerSecond);

    public double ClampPlayhead(double timeSeconds) =>
        TimelineCoordinateSystem.ClampPlayhead(timeSeconds, Duration);

    public double ClampClipStart(double startTime, double clipDuration) =>
        TimelineCoordinateSystem.ClampClipStart(startTime, clipDuration, Duration);

    public double NiceTickStep(double widthPixels) =>
        timePresentation.RulerTickStepSeconds(Duration, widthPixels);

    public string FormatTimeLabel(double seconds) =>
        timePresentation.RulerLabels.Format(seconds);

    public double SnapTime(
        double proposedTimeSeconds,
        double playheadTimeSeconds,
        IReadOnlyList<TimelineMarkerItem> markers,
        IReadOnlyList<TimelineClipItem> clips,
        string? excludeClipId = null)
    {
        var snapped = snapEngine.SnapTime(proposedTimeSeconds, playheadTimeSeconds, markers, clips, excludeClipId);
        return ApplyFrameQuantizationWhenSnapping(snapped);
    }

    public double SnapClipStart(
        double proposedStartSeconds,
        double clipDurationSeconds,
        double playheadTimeSeconds,
        IReadOnlyList<TimelineMarkerItem> markers,
        IReadOnlyList<TimelineClipItem> clips,
        string? movingClipId = null)
    {
        var snapped = snapEngine.SnapClipStart(
            proposedStartSeconds,
            clipDurationSeconds,
            playheadTimeSeconds,
            markers,
            clips,
            movingClipId);
        return ApplyFrameQuantizationWhenSnapping(snapped);
    }

    private double ApplyFrameQuantizationWhenSnapping(double seconds) =>
        snapEngine.Settings.IsEnabled && timePresentation.UsesFrameQuantization
            ? timePresentation.QuantizeTime(seconds)
            : seconds;

    public void RegisterSnapTargetProvider(ITimelineSnapTargetProvider provider) =>
        snapEngine.RegisterProvider(provider);

    public void UnregisterSnapTargetProvider(ITimelineSnapTargetProvider provider) =>
        snapEngine.UnregisterProvider(provider);

    public void SyncTimeModeFromProject(TimelineProject project)
    {
        timePresentation.TimeUnit = project.TimeUnit;
        timePresentation.FramesPerSecond = project.Fps;
        timePresentation.ApplyFrameSnapTo(snapEngine.Settings);
    }
}
