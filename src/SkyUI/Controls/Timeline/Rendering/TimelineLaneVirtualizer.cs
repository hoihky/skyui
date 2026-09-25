namespace SkyUI.Controls.Timeline.Rendering;

/// <summary>
/// Computes visible track row range for lane virtualization (SkyVirtualDataGrid-style windowing).
/// </summary>
internal static class TimelineLaneVirtualizer
{
    public static (int FirstRow, int LastRow) GetVisibleRowRange(
        double scrollOffsetY,
        double viewportHeight,
        int trackCount,
        double trackHeight,
        int overscanRows = TimelineRenderMetrics.LaneOverscanRows)
    {
        if (trackCount <= 0)
            return (0, -1);

        if (viewportHeight <= 0 || double.IsNaN(viewportHeight))
            return (0, trackCount - 1);

        var first = (int)Math.Floor(scrollOffsetY / trackHeight) - overscanRows;
        var last = (int)Math.Ceiling((scrollOffsetY + viewportHeight) / trackHeight) + overscanRows;
        first = Math.Clamp(first, 0, trackCount - 1);
        last = Math.Clamp(last, 0, trackCount - 1);
        return (first, last);
    }

    public static double MeasureContentHeight(int trackCount, double trackHeight, double viewportHeight)
    {
        var baseH = Math.Max(trackCount, 1) * trackHeight;
        return Math.Max(baseH, viewportHeight > 0 ? viewportHeight : baseH);
    }
}
