using Avalonia;
using Avalonia.Controls;
using SkyUI.Icons;

namespace SkyUI.Controls;

/// <summary>Entry in <see cref="SkyNavigationView"/>; <see cref="Content"/> is shown when selected.</summary>
public class SkyNavigationViewItem : ContentControl
{
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<SkyNavigationViewItem, string?>(nameof(Label));

    public static readonly StyledProperty<SkyIconKind?> IconKindProperty =
        AvaloniaProperty.Register<SkyNavigationViewItem, SkyIconKind?>(nameof(IconKind));

    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public SkyIconKind? IconKind
    {
        get => GetValue(IconKindProperty);
        set => SetValue(IconKindProperty, value);
    }
}
