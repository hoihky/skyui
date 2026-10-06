using SkyUI.Controls.Timeline.Input;
using SkyUI.Controls.Timeline.Layout;

namespace SkyUI.Controls.Timeline.Navigation;

/// <summary>Keeps the playhead inside the horizontal viewport during playback.</summary>
public sealed class TimelinePlayheadScrollFollower
{
    private const double MarginPixels = 48;

    public void Follow(TimelineInteractionContext ctx)
    {
        var scroll = ctx.MainScroll;
        if (scroll is null)
            return;
        var playheadX = ctx.Layout.TimeToPixel(ctx.PlayheadTime);
        var left = scroll.Offset.X;
        var right = left + scroll.Viewport.Width;
        if (playheadX < left + MarginPixels)
            scroll.Offset = new Avalonia.Vector(Math.Max(0, playheadX - MarginPixels), scroll.Offset.Y);
        else if (playheadX > right - MarginPixels)
            scroll.Offset = new Avalonia.Vector(playheadX - scroll.Viewport.Width + MarginPixels, scroll.Offset.Y);
        if (ctx.RulerScroll is not null)
            ctx.RulerScroll.Offset = new Avalonia.Vector(scroll.Offset.X, ctx.RulerScroll.Offset.Y);
    }
}
