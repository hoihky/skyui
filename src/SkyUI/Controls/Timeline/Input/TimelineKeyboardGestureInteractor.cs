using Avalonia.Input;
using Avalonia.Interactivity;

namespace SkyUI.Controls.Timeline.Input;

internal sealed class TimelineKeyboardGestureInteractor : ITimelineGestureInteractor
{
    private readonly TimelineKeyboardTransport transportKeys = new();
    private readonly TimelineKeyboardClipEditing clipEditingKeys = new();
    private TimelineInteractionContext? context;

    public int Priority => 100;

    public void Attach(TimelineInteractionContext ctx)
    {
        context = ctx;
        ctx.Control.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
    }

    public void Detach()
    {
        if (context != null)
            context.Control.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
        context = null;
    }

    public void Dispose() => Detach();

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        var ctx = context;
        if (ctx == null || (!ctx.Control.IsFocused && !ctx.Control.Focus()))
            return;

        if (transportKeys.TryHandleKeyDown(ctx, e))
            return;

        if (clipEditingKeys.TryHandleKeyDown(ctx, e))
            return;

        if (e.Key == Key.Delete || e.Key == Key.Back)
        {
            if (ctx.Selection.SelectedClipIds.Count > 0)
                ctx.Host.DeleteSelectedClips();
            else
                ctx.Host.RemoveSelectedTrack();
            e.Handled = true;
            return;
        }

        if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.Z)
        {
            ctx.UndoStack.Undo();
            e.Handled = true;
            return;
        }

        if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.Y)
        {
            ctx.UndoStack.Redo();
            e.Handled = true;
        }

        if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.C)
        {
            ctx.Host.CopySelection();
            e.Handled = true;
            return;
        }

        if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.X)
        {
            ctx.Host.CutSelection();
            e.Handled = true;
            return;
        }

        if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.V)
        {
            ctx.Host.PasteClipboardAtPlayhead(ctx.PlayheadTime);
            e.Handled = true;
        }
    }
}
