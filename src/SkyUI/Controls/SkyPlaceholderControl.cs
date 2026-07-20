using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace SkyUI.Controls;

/// <summary>
/// Sample control for the SkyUI library. Replace or extend with your own controls.
/// </summary>
public class SkyPlaceholderControl : Control
{
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        Border.BackgroundProperty.AddOwner<SkyPlaceholderControl>();

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public sealed override void Render(DrawingContext context)
    {
        if (Background is not null)
        {
            context.FillRectangle(Background, new Rect(Bounds.Size));
        }

        base.Render(context);
    }
}
