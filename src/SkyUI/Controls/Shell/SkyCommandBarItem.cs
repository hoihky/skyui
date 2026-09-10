using System.Windows.Input;
using Avalonia;
using SkyUI.Icons;

namespace SkyUI.Controls;

/// <summary>Declarative command-bar action (bind from MVVM collections).</summary>
public sealed class SkyCommandBarItem : AvaloniaObject
{
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<SkyCommandBarItem, string?>(nameof(Label));

    public static readonly StyledProperty<SkyIconKind?> IconKindProperty =
        AvaloniaProperty.Register<SkyCommandBarItem, SkyIconKind?>(nameof(IconKind));

    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<SkyCommandBarItem, ICommand?>(nameof(Command));

    public static readonly StyledProperty<object?> ToolTipProperty =
        AvaloniaProperty.Register<SkyCommandBarItem, object?>(nameof(ToolTip));

    public static readonly StyledProperty<bool> IsVisibleProperty =
        AvaloniaProperty.Register<SkyCommandBarItem, bool>(nameof(IsVisible), true);

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

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? ToolTip
    {
        get => GetValue(ToolTipProperty);
        set => SetValue(ToolTipProperty, value);
    }

    public bool IsVisible
    {
        get => GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }
}
