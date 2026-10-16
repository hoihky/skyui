using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace SkyUI.Controls.Professional;

/// <summary>Ensures pane <see cref="SkyDockPane.Content"/> is attached to at most one presenter.</summary>
internal static class SkyDockContentHostHelper
{
    private static readonly AttachedProperty<ContentPresenter?> ActiveHostProperty =
        AvaloniaProperty.RegisterAttached<Control, ContentPresenter?>("ActiveContentHost", typeof(SkyDockContentHostHelper));

    public static void Assign(ContentPresenter? host, object? content)
    {
        if (host is null)
            return;

        if (content is null)
        {
            if (host.Content is Control previous)
                previous.ClearValue(ActiveHostProperty);
            host.Content = null;
            return;
        }

        if (ReferenceEquals(host.Content, content))
            return;

        if (host.Content is Control replacedOnHost && !ReferenceEquals(replacedOnHost, content))
            replacedOnHost.ClearValue(ActiveHostProperty);

        host.Content = null;

        if (content is Control control)
        {
            var previousHost = control.GetValue(ActiveHostProperty);
            if (previousHost is not null && !ReferenceEquals(previousHost, host))
                previousHost.Content = null;

            control.SetValue(ActiveHostProperty, host);
        }

        host.Content = content;
    }
}
