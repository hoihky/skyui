namespace SkyUI.Controls.Timeline.Navigation;

public sealed class TimelineViewportZoomPlanner
{
    public double ComputePixelsPerSecond(
        double timeSpanSeconds,
        double viewportWidthPixels,
        double minPixelsPerSecond,
        double maxPixelsPerSecond,
        double viewportFillRatio = 0.9)
    {
        if (timeSpanSeconds <= 1e-6 || viewportWidthPixels <= 0)
            return minPixelsPerSecond;
        var target = viewportWidthPixels * viewportFillRatio / timeSpanSeconds;
        return Math.Clamp(target, minPixelsPerSecond, maxPixelsPerSecond);
    }
}
