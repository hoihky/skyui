using Avalonia.Input;

namespace SkyUI.Controls.Timeline.Input;

/// <summary>Keyboard shortcuts for frame stepping and play/pause.</summary>
public sealed class TimelineKeyboardTransport
{
    public bool TryHandleKeyDown(TimelineInteractionContext context, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            context.Host.TogglePlayPause();
            e.Handled = true;
            return true;
        }

        if (e.Key is Key.Left or Key.Right)
        {
            var delta = e.Key == Key.Left ? -1 : 1;
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                delta *= 10;
            context.Host.StepPlayheadFrames(delta);
            e.Handled = true;
            return true;
        }

        return false;
    }
}
