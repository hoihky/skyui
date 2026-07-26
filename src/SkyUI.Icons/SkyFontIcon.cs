using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace SkyUI.Icons;

/// <summary>Icon from <see cref="SkyIconFont"/> at a fixed pixel size.</summary>
public class SkyFontIcon : TextBlock
{
    public static readonly StyledProperty<SkyIconKind> KindProperty =
        AvaloniaProperty.Register<SkyFontIcon, SkyIconKind>(nameof(Kind), SkyIconKind.None);

    public static readonly StyledProperty<SkyIconSize> SizeProperty =
        AvaloniaProperty.Register<SkyFontIcon, SkyIconSize>(nameof(Size), SkyIconSize.Medium);

    static SkyFontIcon()
    {
        FontFamilyProperty.OverrideDefaultValue<SkyFontIcon>(SkyIconFont.FamilyWithFallback);
        TextAlignmentProperty.OverrideDefaultValue<SkyFontIcon>(TextAlignment.Center);
        VerticalAlignmentProperty.OverrideDefaultValue<SkyFontIcon>(VerticalAlignment.Center);
    }

    public SkyFontIcon()
    {
        ApplyGlyph();
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

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == KindProperty
            || change.Property == SizeProperty
            || change.Property == ForegroundProperty)
            ApplyGlyph();
    }

    private void ApplyGlyph()
    {
        Text = SkyIconFont.TryGetGlyph(Kind) ?? string.Empty;
        FontSize = SkyIconGlyphs.ToPixels(Size);
        Width = Height = FontSize;
    }
}
