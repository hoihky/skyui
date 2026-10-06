using Avalonia.Input;

namespace SkyUI.Controls.Timeline.Input;

/// <summary>Keyboard shortcuts for hold-frame and keyframe edits.</summary>
public sealed class TimelineKeyboardClipEditing
{
    public bool TryHandleKeyDown(TimelineInteractionContext context, KeyEventArgs e)
    {
        if (e.Key is Key.OemCloseBrackets or Key.OemOpenBrackets)
        {
            var delta = e.Key == Key.OemCloseBrackets ? 1 : -1;
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                delta *= 5;
            if (!context.Host.AdjustSelectedClipHoldFrames(delta))
                return false;
            e.Handled = true;
            return true;
        }

        if (e.Key == Key.K && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            if (!context.Host.SetOpacityKeyframeAtPlayheadForSelectedTrack())
                return false;
            e.Handled = true;
            return true;
        }

        return false;
    }
}
