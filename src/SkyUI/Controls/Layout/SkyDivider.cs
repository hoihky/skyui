using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace SkyUI.Controls;

/// <summary>Themed divider with optional label.</summary>
[PseudoClasses("horizontal", "vertical")]
public class SkyDivider : TemplatedControl
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<SkyDivider, string?>(nameof(Text));

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<SkyDivider, Orientation>(nameof(Orientation), Orientation.Horizontal);

    static SkyDivider()
    {
        OrientationProperty.Changed.AddClassHandler<SkyDivider>((divider, e) =>
            divider.SyncOrientation((Orientation)e.NewValue!));
    }

    public SkyDivider()
    {
        Classes.Add("sky");
        Classes.Add("sky-divider");
        SyncOrientation(Orientation.Horizontal);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    private void SyncOrientation(Orientation orientation)
    {
        PseudoClasses.Set(":horizontal", orientation == Orientation.Horizontal);
        PseudoClasses.Set(":vertical", orientation == Orientation.Vertical);
    }
}
