using Avalonia.Controls;
using Avalonia.Input;

namespace SkyUI.Controls.Timeline.Input;

internal sealed class TimelineRulerScrubGestureInteractor : ITimelineGestureInteractor
{
    private TimelineInteractionContext? context;

    public int Priority => 60;

    public void Attach(TimelineInteractionContext ctx)
    {
        context = ctx;
        if (ctx.RulerCanvas != null)
        {
            foreach (var child in ctx.RulerCanvas.Children.OfType<Control>())
            {
                if (child is Avalonia.Controls.Shapes.Rectangle)
                    child.PointerPressed += OnRulerPressed;
            }
        }
    }

    public void Detach() => context = null;

    public void Dispose() => Detach();

    public void OnRulerPressed(object? sender, PointerPressedEventArgs e)
    {
        var ctx = context;
        if (ctx?.RulerCanvas == null || ctx.RulerScroll == null)
            return;
        var p = e.GetPosition(ctx.RulerCanvas);
        ctx.PlayheadTime = ctx.Layout.PointerTimeToSeconds(p.X, ctx.RulerScroll.Offset.X);
        e.Pointer.Capture((IInputElement)sender!);
    }

    public void OnRulerMoved(object? sender, PointerEventArgs e)
    {
        var ctx = context;
        if (ctx?.RulerCanvas == null || ctx.RulerScroll == null)
            return;
        if (e.Pointer.Captured != sender)
            return;
        var p = e.GetPosition(ctx.RulerCanvas);
        ctx.PlayheadTime = ctx.Layout.PointerTimeToSeconds(p.X, ctx.RulerScroll.Offset.X);
    }

    public void OnRulerReleased(object? sender, PointerReleasedEventArgs e) =>
        e.Pointer.Capture(null);
}
