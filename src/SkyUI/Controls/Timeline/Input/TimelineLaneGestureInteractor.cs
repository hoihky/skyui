using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SkyUI.Controls.Timeline.Editing;

namespace SkyUI.Controls.Timeline.Input;

internal sealed class TimelineLaneGestureInteractor : ITimelineGestureInteractor
{
    private TimelineInteractionContext? context;
    private bool rangeDrag;
    private bool rangeActive;
    private bool marqueeClipDrag;
    private Point rangePressPoint;
    private double rangeStartTime;
    private double marqueeStartTime;
    private int marqueeStartRow;
    private double marqueeMinTime;
    private double marqueeMaxTime;
    private int marqueeMinRow;
    private int marqueeMaxRow;

    public int Priority => 50;

    public void Attach(TimelineInteractionContext ctx) => context = ctx;

    public void Detach() => context = null;

    public void Dispose() => Detach();

    public void OnLanePressed(object? sender, PointerPressedEventArgs e)
    {
        var ctx = context;
        if (ctx == null || sender is not Control { Tag: string tid })
            return;
        ctx.SetSelectedTrackId(tid);
        ctx.Selection.ClearClipSelection();
        ctx.PlayheadTime = ctx.TimeFromMainPointer(e.GetPosition(ctx.MainCanvas!));
        e.Handled = true;
    }

    public void OnBackgroundPressed(object? sender, PointerPressedEventArgs e)
    {
        var ctx = context;
        if (ctx?.MainCanvas == null)
            return;

        var py = e.GetPosition(ctx.MainCanvas).Y;
        var row = (int)Math.Floor(py / Rendering.TimelineRenderMetrics.TrackHeight);
        row = Math.Clamp(row, 0, Math.Max(0, ctx.Tracks.Count - 1));

        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            marqueeClipDrag = true;
            rangeDrag = false;
            marqueeStartTime = ctx.TimeFromMainPointer(e.GetPosition(ctx.MainCanvas));
            marqueeStartRow = row;
            marqueeMinTime = marqueeMaxTime = marqueeStartTime;
            marqueeMinRow = marqueeMaxRow = marqueeStartRow;
            e.Pointer.Capture((IInputElement)sender!);
            e.Handled = true;
            return;
        }

        if (ctx.Tracks.Count > 0)
            ctx.SetSelectedTrackId(ctx.Tracks[row].Id);

        rangeDrag = true;
        rangeActive = false;
        marqueeClipDrag = false;
        rangePressPoint = e.GetPosition(ctx.MainCanvas);
        rangeStartTime = ctx.TimeFromMainPointer(e.GetPosition(ctx.MainCanvas));
        ctx.Selection.ClearClipSelection();
        ctx.PlayheadTime = rangeStartTime;
        e.Pointer.Capture((IInputElement)sender!);
    }

    public void OnBackgroundMoved(object? sender, PointerEventArgs e)
    {
        var ctx = context;
        if (ctx?.MainCanvas == null)
            return;

        if (marqueeClipDrag)
        {
            var marqueeTime = ctx.TimeFromMainPointer(e.GetPosition(ctx.MainCanvas));
            marqueeMinTime = Math.Min(marqueeStartTime, marqueeTime);
            marqueeMaxTime = Math.Max(marqueeStartTime, marqueeTime);
            var py = e.GetPosition(ctx.MainCanvas).Y;
            var row = (int)Math.Floor(py / Rendering.TimelineRenderMetrics.TrackHeight);
            row = Math.Clamp(row, 0, Math.Max(0, ctx.Tracks.Count - 1));
            marqueeMinRow = Math.Min(marqueeStartRow, row);
            marqueeMaxRow = Math.Max(marqueeStartRow, row);
            ApplyMarqueeSelection(ctx);
            e.Handled = true;
            return;
        }

        if (!rangeDrag)
            return;

        var p = e.GetPosition(ctx.MainCanvas);
        if (!rangeActive)
        {
            var dx = p.X - rangePressPoint.X;
            var dy = p.Y - rangePressPoint.Y;
            if (dx * dx + dy * dy < 9)
                return;
            rangeActive = true;
            ctx.HasTimeRangeSelection = true;
            ctx.TimeRangeSelection = new TimelineTimeRange(rangeStartTime, rangeStartTime);
            ctx.RaiseTimeRangeChanged(ctx.TimeRangeSelection);
        }

        var t = ctx.TimeFromMainPointer(e.GetPosition(ctx.MainCanvas));
        ctx.TimeRangeSelection = new TimelineTimeRange(rangeStartTime, t);
        ctx.UpdateOverlays();
        ctx.RaiseTimeRangeChanged(ctx.TimeRangeSelection);
    }

    public void OnBackgroundReleased(object? sender, PointerReleasedEventArgs e)
    {
        var ctx = context;
        if (ctx == null)
            return;

        if (marqueeClipDrag)
        {
            marqueeClipDrag = false;
            ApplyMarqueeSelection(ctx);
            e.Pointer.Capture(null);
            e.Handled = true;
            return;
        }

        if (rangeDrag)
        {
            rangeDrag = false;
            if (!rangeActive)
                ctx.Host.ClearTimeRangeSelection();
            e.Pointer.Capture(null);
        }
    }

    public void OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        var ctx = context;
        if (ctx == null)
            return;
        var t = ctx.TimeFromMainPointer(e.GetPosition(ctx.MainCanvas!));
        var label = $"Note @ {ctx.Layout.FormatTimeLabel(t)}";
        ctx.Host.AddMarker(t, label);
        e.Handled = true;
    }

    private void ApplyMarqueeSelection(TimelineInteractionContext ctx)
    {
        var ids = new List<string>();
        foreach (var clip in ctx.Clips)
        {
            var row = ctx.TrackRowIndex(clip.TrackId);
            if (row < 0)
                continue;
            if (TimelineClipOperations.IntersectsMarquee(
                    clip, row, marqueeMinTime, marqueeMaxTime, marqueeMinRow, marqueeMaxRow))
                ids.Add(clip.Id);
        }

        ctx.Selection.SetClipSelection(ids);
        ctx.SetSelectedTrackId(null);
    }
}
