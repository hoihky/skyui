using System.Windows.Input;
using Avalonia;
using SkyUI.Icons;

namespace SkyUI.Controls;

/// <summary>Menu entry used by <see cref="SkyDropDownButton"/> and <see cref="SkySplitButton"/>.</summary>
public sealed class SkyDropDownMenuItem : AvaloniaObject
{
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<SkyDropDownMenuItem, string?>(nameof(Label));

    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<SkyDropDownMenuItem, ICommand?>(nameof(Command));

    public static readonly StyledProperty<SkyIconKind?> IconKindProperty =
        AvaloniaProperty.Register<SkyDropDownMenuItem, SkyIconKind?>(nameof(IconKind));

    public static readonly StyledProperty<bool> IsEnabledProperty =
        AvaloniaProperty.Register<SkyDropDownMenuItem, bool>(nameof(IsEnabled), true);

    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public SkyIconKind? IconKind
    {
        get => GetValue(IconKindProperty);
        set => SetValue(IconKindProperty, value);
    }

    public bool IsEnabled
    {
        get => GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }
}
