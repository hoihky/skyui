using SkyUI.Controls.Timeline.Model;

namespace SkyUI.Controls.Timeline.Layout;

/// <summary>Time ↔ pixel layout, clamping, ruler math, and magnetic snap.</summary>
public interface ITimelineLayoutEngine
{
    double Duration { get; set; }

    double PixelsPerSecond { get; set; }

    TimelineSnapSettings SnapSettings { get; }

    double ContentWidth(double minimumWidth = 400);

    double TimeToPixel(double timeSeconds);

    double PixelToTime(double pixelX);

    double PointerTimeToSeconds(double canvasX, double scrollOffsetX);

    double ClampPlayhead(double timeSeconds);

    double ClampClipStart(double startTime, double clipDuration);

    double NiceTickStep(double widthPixels);

    string FormatTimeLabel(double seconds);

    double SnapTime(
        double proposedTimeSeconds,
        double playheadTimeSeconds,
        IReadOnlyList<TimelineMarkerItem> markers,
        IReadOnlyList<TimelineClipItem> clips,
        string? excludeClipId = null);

    double SnapClipStart(
        double proposedStartSeconds,
        double clipDurationSeconds,
        double playheadTimeSeconds,
        IReadOnlyList<TimelineMarkerItem> markers,
        IReadOnlyList<TimelineClipItem> clips,
        string? movingClipId = null);

    void RegisterSnapTargetProvider(ITimelineSnapTargetProvider provider);

    void UnregisterSnapTargetProvider(ITimelineSnapTargetProvider provider);
}
