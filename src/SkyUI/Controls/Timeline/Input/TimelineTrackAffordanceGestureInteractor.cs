using Avalonia.Controls;
using Avalonia.Input;
using SkyUI.Controls.Timeline.Model;
using SkyUI.Controls.Timeline.Rendering;

namespace SkyUI.Controls.Timeline.Input;

internal sealed class TimelineTrackAffordanceGestureInteractor : ITimelineGestureInteractor
{
    private TimelineInteractionContext? context;

    public int Priority => 85;

    public void Attach(TimelineInteractionContext ctx) => context = ctx;

    public void Detach() => context = null;

    public void Dispose() => Detach();

    public void OnAffordancePressed(object? sender, PointerPressedEventArgs e)
    {
        var ctx = context;
        if (ctx is null || sender is not Control { Tag: string tag })
            return;
        if (!TimelineTrackHeaderTags.TryParse(tag, out var kind, out var trackId))
            return;
        var track = ctx.Tracks.FirstOrDefault(t => t.Id == trackId);
        if (track is null)
            return;
        if (kind == "vis")
            ctx.Host.ToggleTrackVisibility(track);
        else if (kind == "lock")
            ctx.Host.ToggleTrackLocked(track);
        e.Handled = true;
    }
}
