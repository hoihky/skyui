namespace SkyUI.Controls.Timeline.Layout;

/// <summary>Default layout engine composing coordinate math and snap.</summary>
public sealed class TimelineLayoutEngine : ITimelineLayoutEngine
{
    private readonly TimelineSnapEngine snapEngine = new();

    public double Duration { get; set; } = 120;

    public double PixelsPerSecond { get; set; } = 40;

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
        TimelineCoordinateSystem.NiceTickStep(Duration, widthPixels);

    public string FormatTimeLabel(double seconds) =>
        TimelineCoordinateSystem.FormatTimeLabel(seconds);

    public double SnapTime(
        double proposedTimeSeconds,
        double playheadTimeSeconds,
        IReadOnlyList<TimelineMarkerItem> markers,
        IReadOnlyList<TimelineClipItem> clips,
        string? excludeClipId = null) =>
        snapEngine.SnapTime(proposedTimeSeconds, playheadTimeSeconds, markers, clips, excludeClipId);

    public double SnapClipStart(
        double proposedStartSeconds,
        double clipDurationSeconds,
        double playheadTimeSeconds,
        IReadOnlyList<TimelineMarkerItem> markers,
        IReadOnlyList<TimelineClipItem> clips,
        string? movingClipId = null) =>
        snapEngine.SnapClipStart(
            proposedStartSeconds,
            clipDurationSeconds,
            playheadTimeSeconds,
            markers,
            clips,
            movingClipId);

    public void RegisterSnapTargetProvider(ITimelineSnapTargetProvider provider) =>
        snapEngine.RegisterProvider(provider);

    public void UnregisterSnapTargetProvider(ITimelineSnapTargetProvider provider) =>
        snapEngine.UnregisterProvider(provider);
}
