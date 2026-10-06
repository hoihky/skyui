using SkyUI.Controls.Timeline.Input;
using SkyUI.Controls.Timeline.Layout;

namespace SkyUI.Controls.Timeline.Navigation;

public sealed class TimelineViewportNavigator
{
    private readonly TimelineViewportZoomPlanner zoomPlanner = new();
    private readonly TimelinePlayheadScrollFollower playheadFollower = new();

    public void ZoomToTimeRange(TimelineInteractionContext ctx, double minSeconds, double maxSeconds)
    {
        var span = Math.Max(1e-3, maxSeconds - minSeconds);
        var width = ctx.MainScroll?.Viewport.Width ?? 400;
        var pps = zoomPlanner.ComputePixelsPerSecond(
            span,
            width,
            TimelineCoordinateSystem.MinPixelsPerSecond,
            TimelineCoordinateSystem.MaxPixelsPerSecond);
        ctx.Control.SetPixelsPerSecondFromHost(pps);
        ScrollTimeIntoView(ctx, minSeconds);
    }

    public void ZoomToFit(TimelineInteractionContext ctx)
    {
        ZoomToTimeRange(ctx, 0, ctx.Duration);
    }

    public void ZoomToSelection(TimelineInteractionContext ctx)
    {
        var clips = ctx.Host.GetSelectedClips();
        if (clips.Count == 0)
        {
            if (ctx.HasTimeRangeSelection)
            {
                ZoomToTimeRange(ctx, ctx.TimeRangeSelection.Min, ctx.TimeRangeSelection.Max);
                return;
            }

            ZoomToFit(ctx);
            return;
        }

        var min = clips.Min(c => c.StartTime);
        var max = clips.Max(c => c.StartTime + c.Duration);
        ZoomToTimeRange(ctx, min, max);
    }

    public void FollowPlayhead(TimelineInteractionContext ctx) => playheadFollower.Follow(ctx);

    private static void ScrollTimeIntoView(TimelineInteractionContext ctx, double timeSeconds)
    {
        var scroll = ctx.MainScroll;
        if (scroll is null)
            return;
        var x = ctx.Layout.TimeToPixel(timeSeconds);
        scroll.Offset = new Avalonia.Vector(Math.Max(0, x - 40), scroll.Offset.Y);
        if (ctx.RulerScroll is not null)
            ctx.RulerScroll.Offset = new Avalonia.Vector(scroll.Offset.X, ctx.RulerScroll.Offset.Y);
    }
}
