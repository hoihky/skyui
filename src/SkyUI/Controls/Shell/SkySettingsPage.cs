using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace SkyUI.Controls;

/// <summary>Settings app scaffold: navigation body with optional save/reset footer.</summary>
public class SkySettingsPage : TemplatedControl
{
    public const string NavigationPartName = "PART_Navigation";
    public const string FooterPartName = "PART_Footer";
    public const string FooterContentPartName = "PART_FooterContent";
    public const string ResetButtonPartName = "PART_ResetButton";
    public const string SaveButtonPartName = "PART_SaveButton";

    public static readonly StyledProperty<object?> NavigationContentProperty =
        AvaloniaProperty.Register<SkySettingsPage, object?>(nameof(NavigationContent));

    public static readonly StyledProperty<object?> FooterContentProperty =
        AvaloniaProperty.Register<SkySettingsPage, object?>(nameof(FooterContent));

    public static readonly StyledProperty<ICommand?> SaveCommandProperty =
        AvaloniaProperty.Register<SkySettingsPage, ICommand?>(nameof(SaveCommand));

    public static readonly StyledProperty<ICommand?> ResetCommandProperty =
        AvaloniaProperty.Register<SkySettingsPage, ICommand?>(nameof(ResetCommand));

    public static readonly StyledProperty<string?> SaveButtonTextProperty =
        AvaloniaProperty.Register<SkySettingsPage, string?>(nameof(SaveButtonText), "Save");

    public static readonly StyledProperty<string?> ResetButtonTextProperty =
        AvaloniaProperty.Register<SkySettingsPage, string?>(nameof(ResetButtonText), "Reset");

    public static readonly StyledProperty<bool> IsFooterVisibleProperty =
        AvaloniaProperty.Register<SkySettingsPage, bool>(nameof(IsFooterVisible), true);

    public SkySettingsPage()
    {
        Classes.Add("sky");
        Classes.Add("sky-settings-page");
    }

    public object? NavigationContent
    {
        get => GetValue(NavigationContentProperty);
        set => SetValue(NavigationContentProperty, value);
    }

    public object? FooterContent
    {
        get => GetValue(FooterContentProperty);
        set => SetValue(FooterContentProperty, value);
    }

    public ICommand? SaveCommand
    {
        get => GetValue(SaveCommandProperty);
        set => SetValue(SaveCommandProperty, value);
    }

    public ICommand? ResetCommand
    {
        get => GetValue(ResetCommandProperty);
        set => SetValue(ResetCommandProperty, value);
    }

    public string? SaveButtonText
    {
        get => GetValue(SaveButtonTextProperty);
        set => SetValue(SaveButtonTextProperty, value);
    }

    public string? ResetButtonText
    {
        get => GetValue(ResetButtonTextProperty);
        set => SetValue(ResetButtonTextProperty, value);
    }

    public bool IsFooterVisible
    {
        get => GetValue(IsFooterVisibleProperty);
        set => SetValue(IsFooterVisibleProperty, value);
    }
}
