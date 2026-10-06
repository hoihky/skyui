using Avalonia.Input;

namespace SkyUI.Controls.Timeline.Input;

/// <summary>J/K/L style transport shortcuts (step back, pause, play / step forward).</summary>
public sealed class TimelineKeyboardShuttle
{
    public bool TryHandleKeyDown(TimelineInteractionContext context, KeyEventArgs e)
    {
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            return false;

        if (e.Key == Key.J)
        {
            var delta = e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? -10 : -1;
            context.Host.StepPlayheadFrames(delta);
            e.Handled = true;
            return true;
        }

        if (e.Key == Key.K)
        {
            context.Host.StopPlayback();
            e.Handled = true;
            return true;
        }

        if (e.Key == Key.L)
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                context.Host.StepPlayheadFrames(10);
            }
            else if (context.Control.IsPlaying)
            {
                context.Host.StepPlayheadFrames(1);
            }
            else
            {
                context.Host.Play();
            }

            e.Handled = true;
            return true;
        }

        return false;
    }
}
