using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using SkyUI.Controls.Timeline.Rendering;

namespace SkyUI.Controls.Timeline.Input;

internal sealed class TimelineTrackHeaderGestureInteractor : ITimelineGestureInteractor
{
    private TimelineInteractionContext? context;
    private int? reorderFrom;
    private Point pressPoint;
    private double pressYInHeaderStack;
    private bool reorderDrag;
    private Border? pressedHeader;

    public int Priority => 40;

    public void Attach(TimelineInteractionContext ctx) => context = ctx;

    public void Detach() => context = null;

    public void Dispose() => Detach();

    public void OnHeaderPressed(object? sender, PointerPressedEventArgs e)
    {
        var ctx = context;
        if (ctx == null || sender is not Control { Tag: string tid })
            return;
        reorderFrom = ctx.Tracks.ToList().FindIndex(t => t.Id == tid);
        pressPoint = ctx.HeaderStack != null ? e.GetPosition(ctx.HeaderStack) : default;
        pressYInHeaderStack = pressPoint.Y;
        pressedHeader = sender as Border;
        reorderDrag = false;
        e.Pointer.Capture((IInputElement)sender!);
    }

    public void OnHeaderMoved(object? sender, PointerEventArgs e)
    {
        var ctx = context;
        if (reorderFrom is null || ctx?.HeaderStack == null || sender is not Control border)
            return;
        if (!ReferenceEquals(e.Pointer.Captured, border))
            return;
        var now = e.GetPosition(ctx.HeaderStack);
        var dy = now.Y - pressPoint.Y;
        if (!reorderDrag && Math.Abs(dy) >= TimelineRenderMetrics.TrackReorderDragThreshold)
        {
            reorderDrag = true;
            if (pressedHeader?.Tag is string tid)
                ctx.Renderer.BeginTrackReorderDrag(ctx, pressedHeader, tid, pressYInHeaderStack);
        }

        if (reorderDrag && reorderFrom is int from)
            ctx.Renderer.UpdateTrackReorderDrag(ctx, from, now.Y);
    }

    public void OnHeaderReleased(object? sender, PointerReleasedEventArgs e)
    {
        var ctx = context;
        try
        {
            if (ctx == null || sender is not Control { Tag: string tid } || ctx.HeaderStack == null)
                return;
            if (reorderFrom is { } from && ctx.Tracks.Count > 0 && reorderDrag)
            {
                var y = e.GetPosition(ctx.HeaderStack).Y;
                var to = (int)Math.Floor(y / TimelineRenderMetrics.TrackHeight);
                to = Math.Clamp(to, 0, ctx.Tracks.Count - 1);
                if (to != from)
                {
                    var item = ctx.Tracks[from];
                    ctx.Tracks.RemoveAt(from);
                    ctx.Tracks.Insert(to, item);
                    ctx.RaiseTrackReordered(item, from, to);
                    ctx.SetSelectedTrackId(item.Id);
                    ctx.FullRebuild();
                }
                else
                    ctx.SetSelectedTrackId(tid);
            }
            else
            {
                ctx.SetSelectedTrackId(tid);
                ctx.Selection.ClearClipSelection();
            }
        }
        finally
        {
            if (ctx != null)
                ctx.Renderer.EndTrackReorderDrag(ctx);
            reorderFrom = null;
            reorderDrag = false;
            pressedHeader = null;
            e.Pointer.Capture(null);
        }
    }
}
