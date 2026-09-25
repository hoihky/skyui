namespace SkyUI.Controls;

/// <summary>Time ↔ pixel conversions and clamping for timeline layout.</summary>
public static class TimelineCoordinateSystem
{
    public const double MinClipDurationSeconds = 0.08;
    public const double MinPixelsPerSecond = 6;
    public const double MaxPixelsPerSecond = 640;
    public const double MinDurationSeconds = 0.01;

    public static double ClampDuration(double duration) =>
        duration < MinDurationSeconds ? MinDurationSeconds : duration;

    public static double ClampPixelsPerSecond(double pixelsPerSecond) =>
        Math.Clamp(pixelsPerSecond, MinPixelsPerSecond, MaxPixelsPerSecond);

    public static double ClampPlayhead(double timeSeconds, double durationSeconds)
    {
        if (timeSeconds < 0)
            return 0;
        return timeSeconds > durationSeconds ? durationSeconds : timeSeconds;
    }

    public static double TimeToPixel(double timeSeconds, double pixelsPerSecond) =>
        timeSeconds * pixelsPerSecond;

    public static double PixelToTime(double pixelX, double pixelsPerSecond)
    {
        if (pixelsPerSecond <= 0)
            return 0;
        return pixelX / pixelsPerSecond;
    }

    public static double PointerTimeToSeconds(double canvasX, double scrollOffsetX, double pixelsPerSecond) =>
        PixelToTime(canvasX + scrollOffsetX, pixelsPerSecond);

    public static double ClampClipStart(double startTime, double duration, double timelineDuration)
    {
        var maxStart = Math.Max(0, timelineDuration - duration);
        return Math.Clamp(startTime, 0, maxStart);
    }

    public static double ContentWidth(double durationSeconds, double pixelsPerSecond, double minimumWidth = 400) =>
        Math.Max(durationSeconds * pixelsPerSecond, minimumWidth);

    public static double NiceTickStep(double durationSeconds, double widthPixels)
    {
        if (widthPixels < 120)
            return Math.Max(1, durationSeconds / 4);

        var targetLabels = Math.Max(4, widthPixels / 90);
        var raw = durationSeconds / targetLabels;
        var pow = Math.Pow(10, Math.Floor(Math.Log10(Math.Max(raw, 0.001))));
        var normalized = raw / pow;
        var multiplier = normalized <= 1 ? 1 : normalized <= 2 ? 2 : normalized <= 5 ? 5 : 10;
        return multiplier * pow;
    }

    public static string FormatTimeLabel(double seconds)
    {
        var s = (int)Math.Floor(seconds % 60);
        var m = (int)Math.Floor(seconds / 60) % 60;
        var h = (int)Math.Floor(seconds / 3600);
        return h > 0 ? $"{h}:{m:00}:{s:00}" : $"{m}:{s:00}";
    }
}
