using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;

namespace SkyUI.Controls.Timeline.Input;

internal sealed class TimelineMarkerDragGestureInteractor : ITimelineGestureInteractor
{
    private TimelineInteractionContext? context;
    private TimelineMarkerItem? dragMarker;
    private double dragOriginTime;

    public int Priority => 70;

    public void Attach(TimelineInteractionContext ctx) => context = ctx;

    public void Detach() => context = null;

    public void Dispose() => Detach();

    public void OnMarkerPressed(object? sender, PointerPressedEventArgs e)
    {
        var ctx = context;
        if (ctx?.RulerCanvas == null || ctx.RulerScroll == null || sender is not Control { Tag: string id })
            return;
        var marker = ctx.Markers.FirstOrDefault(m => m.Id == id);
        if (marker is null)
            return;
        dragMarker = marker;
        dragOriginTime = marker.Time;
        e.Pointer.Capture((IInputElement)sender);
        e.Handled = true;
    }

    public void OnMarkerMoved(object? sender, PointerEventArgs e)
    {
        var ctx = context;
        if (dragMarker is null || ctx?.RulerCanvas == null || ctx.RulerScroll == null)
            return;
        if (!ReferenceEquals(e.Pointer.Captured, sender))
            return;
        var p = e.GetPosition(ctx.RulerCanvas);
        var time = ctx.Layout.PointerTimeToSeconds(p.X, ctx.RulerScroll.Offset.X);
        var markersForSnap = ctx.Markers.Where(m => m.Id != dragMarker.Id).ToList();
        time = ctx.Layout.SnapTime(time, ctx.PlayheadTime, markersForSnap, ctx.Clips, excludeClipId: null);
        dragMarker.Time = Math.Clamp(time, 0, ctx.Duration);
        e.Handled = true;
    }

    public void OnMarkerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var ctx = context;
        if (dragMarker is null || ctx is null)
        {
            e.Pointer.Capture(null);
            return;
        }

        var marker = dragMarker;
        var after = marker.Time;
        if (Math.Abs(after - dragOriginTime) > 1e-9)
            ctx.UndoStack.Execute(new Commands.TimelineMoveMarkerCommand(marker, dragOriginTime, after));

        e.Pointer.Capture(null);
        dragMarker = null;
        e.Handled = true;
    }
}
