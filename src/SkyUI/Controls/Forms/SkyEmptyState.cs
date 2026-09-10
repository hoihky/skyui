using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;
using SkyUI.Icons;

namespace SkyUI.Controls;

/// <summary>Empty placeholder for lists, grids, and search results.</summary>
public class SkyEmptyState : TemplatedControl
{
    public const string IconPartName = "PART_Icon";
    public const string ActionPartName = "PART_Action";

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<SkyEmptyState, string?>(nameof(Title));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<SkyEmptyState, string?>(nameof(Description));

    public static readonly StyledProperty<SkyIconKind> IconKindProperty =
        AvaloniaProperty.Register<SkyEmptyState, SkyIconKind>(nameof(IconKind), SkyIconKind.Search);

    public static readonly StyledProperty<object?> ActionContentProperty =
        AvaloniaProperty.Register<SkyEmptyState, object?>(nameof(ActionContent));

    public static readonly StyledProperty<ICommand?> ActionCommandProperty =
        AvaloniaProperty.Register<SkyEmptyState, ICommand?>(nameof(ActionCommand));

    public static readonly StyledProperty<string?> ActionTextProperty =
        AvaloniaProperty.Register<SkyEmptyState, string?>(nameof(ActionText));

    public SkyEmptyState()
    {
        Classes.Add("sky");
        Classes.Add("sky-empty-state");
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public SkyIconKind IconKind
    {
        get => GetValue(IconKindProperty);
        set => SetValue(IconKindProperty, value);
    }

    public object? ActionContent
    {
        get => GetValue(ActionContentProperty);
        set => SetValue(ActionContentProperty, value);
    }

    public ICommand? ActionCommand
    {
        get => GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    public string? ActionText
    {
        get => GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }
}
