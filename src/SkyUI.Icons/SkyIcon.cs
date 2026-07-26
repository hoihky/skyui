using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Layout;
using Avalonia.Media;
using PathShape = Avalonia.Controls.Shapes.Path;

namespace SkyUI.Icons;

/// <summary>Renders a <see cref="SkyIconKind"/> at 16, 20, or 24 px.</summary>
public class SkyIcon : Viewbox
{
    public static readonly StyledProperty<SkyIconKind> KindProperty =
        AvaloniaProperty.Register<SkyIcon, SkyIconKind>(nameof(Kind), SkyIconKind.None);

    public static readonly StyledProperty<SkyIconSize> SizeProperty =
        AvaloniaProperty.Register<SkyIcon, SkyIconSize>(nameof(Size), SkyIconSize.Medium);

    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        TextElement.ForegroundProperty.AddOwner<SkyIcon>();

    private readonly PathShape _path = new()
    {
        Stretch = Stretch.Fill,
    };

    static SkyIcon()
    {
        StretchProperty.OverrideDefaultValue<SkyIcon>(Stretch.Uniform);
    }

    public SkyIcon()
    {
        Child = _path;
        ApplyIcon();
    }

    public SkyIconKind Kind
    {
        get => GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    public SkyIconSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == KindProperty
            || change.Property == SizeProperty
            || change.Property == ForegroundProperty)
            ApplyIcon();
    }

    private void ApplyIcon()
    {
        _path.Data = SkyIconGlyphs.TryGetGeometry(Kind);
        _path.Fill = Foreground;
        _path.Width = _path.Height = 24;
        _path.IsVisible = _path.Data is not null;

        var px = SkyIconGlyphs.ToPixels(Size);
        Width = Height = px;
    }
}
