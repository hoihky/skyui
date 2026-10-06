using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;

namespace SkyUI.Controls.Timeline.Input;

internal sealed class TimelineKeyframeDragGestureInteractor : ITimelineGestureInteractor
{
    private TimelineInteractionContext? context;
    private TimelineKeyframeItem? dragKeyframe;
    private double dragOriginTime;

    public int Priority => 75;

    public void Attach(TimelineInteractionContext ctx) => context = ctx;

    public void Detach() => context = null;

    public void Dispose() => Detach();

    public void OnKeyframePressed(object? sender, PointerPressedEventArgs e)
    {
        var ctx = context;
        if (ctx?.MainCanvas == null || sender is not Control { Tag: string id })
            return;
        var keyframe = ctx.Project.Keyframes.FirstOrDefault(k => k.Id == id);
        if (keyframe is null)
            return;
        dragKeyframe = keyframe;
        dragOriginTime = keyframe.Time;
        e.Pointer.Capture((IInputElement)sender);
        e.Handled = true;
    }

    public void OnKeyframeMoved(object? sender, PointerEventArgs e)
    {
        var ctx = context;
        if (dragKeyframe is null || ctx?.MainCanvas == null || ctx.MainScroll == null)
            return;
        if (!ReferenceEquals(e.Pointer.Captured, sender))
            return;
        var p = e.GetPosition(ctx.MainCanvas);
        var time = ctx.Layout.PointerTimeToSeconds(p.X, ctx.MainScroll.Offset.X);
        time = ctx.Layout.SnapTime(time, ctx.PlayheadTime, ctx.Markers, ctx.Clips);
        dragKeyframe.Time = Math.Clamp(time, 0, ctx.Duration);
        ctx.Renderer.LayoutKeyframe(ctx, dragKeyframe);
        e.Handled = true;
    }

    public void OnKeyframeReleased(object? sender, PointerReleasedEventArgs e)
    {
        var ctx = context;
        if (dragKeyframe is null || ctx is null)
        {
            e.Pointer.Capture(null);
            return;
        }

        var keyframe = dragKeyframe;
        var after = keyframe.Time;
        if (Math.Abs(after - dragOriginTime) > 1e-9)
            ctx.Host.KeyframeEditor.CommitMove(keyframe, dragOriginTime, after);

        e.Pointer.Capture(null);
        dragKeyframe = null;
        e.Handled = true;
    }
}
