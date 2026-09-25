using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace SkyUI.Controls.Timeline.Input;

internal sealed class TimelineZoomGestureInteractor : ITimelineGestureInteractor
{
    private TimelineInteractionContext? context;

    public int Priority => 10;

    public void Attach(TimelineInteractionContext ctx)
    {
        context = ctx;
        ctx.Control.AddHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged, RoutingStrategies.Bubble);
    }

    public void Detach()
    {
        if (context != null)
            context.Control.RemoveHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged);
        context = null;
    }

    public void Dispose() => Detach();

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        var ctx = context;
        if (ctx?.MainScroll == null || ctx.MainCanvas == null)
            return;
        if (!e.KeyModifiers.HasFlag(KeyModifiers.Control))
            return;

        e.Handled = true;
        var old = ctx.PixelsPerSecond;
        var factor = e.Delta.Y > 0 ? 1.12 : 1 / 1.12;
        var pos = e.GetPosition(ctx.MainCanvas);
        var contentX = ctx.MainScroll.Offset.X + pos.X;
        var time = contentX / old;
        var next = TimelineCoordinateSystem.ClampPixelsPerSecond(old * factor);
        ctx.Control.SetPixelsPerSecondFromHost(next);
        var newContentX = time * next;
        var newOffX = newContentX - pos.X;
        ctx.MainScroll.Offset = new Vector(Math.Max(0, newOffX), ctx.MainScroll.Offset.Y);
        if (ctx.RulerScroll != null)
            ctx.RulerScroll.Offset = new Vector(ctx.MainScroll.Offset.X, ctx.RulerScroll.Offset.Y);
    }
}
