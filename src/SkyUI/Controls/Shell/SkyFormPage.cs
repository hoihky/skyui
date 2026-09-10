using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;

namespace SkyUI.Controls;

/// <summary>Form page scaffold: header, scrollable fields, and optional action footer.</summary>
public class SkyFormPage : TemplatedControl
{
    public const string HeaderPartName = "PART_Header";
    public const string FormContentPartName = "PART_FormContent";
    public const string FooterPartName = "PART_Footer";
    public const string SubmitButtonPartName = "PART_SubmitButton";
    public const string CancelButtonPartName = "PART_CancelButton";

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<SkyFormPage, string?>(nameof(Title));

    public static readonly StyledProperty<string?> SubtitleProperty =
        AvaloniaProperty.Register<SkyFormPage, string?>(nameof(Subtitle));

    public static readonly StyledProperty<bool> IsBackButtonVisibleProperty =
        AvaloniaProperty.Register<SkyFormPage, bool>(nameof(IsBackButtonVisible));

    public static readonly StyledProperty<ICommand?> BackCommandProperty =
        AvaloniaProperty.Register<SkyFormPage, ICommand?>(nameof(BackCommand));

    public static readonly StyledProperty<object?> ActionContentProperty =
        AvaloniaProperty.Register<SkyFormPage, object?>(nameof(ActionContent));

    public static readonly StyledProperty<object?> FormContentProperty =
        AvaloniaProperty.Register<SkyFormPage, object?>(nameof(FormContent));

    public static readonly StyledProperty<object?> FooterContentProperty =
        AvaloniaProperty.Register<SkyFormPage, object?>(nameof(FooterContent));

    public static readonly StyledProperty<ICommand?> SubmitCommandProperty =
        AvaloniaProperty.Register<SkyFormPage, ICommand?>(nameof(SubmitCommand));

    public static readonly StyledProperty<ICommand?> CancelCommandProperty =
        AvaloniaProperty.Register<SkyFormPage, ICommand?>(nameof(CancelCommand));

    public static readonly StyledProperty<string?> SubmitButtonTextProperty =
        AvaloniaProperty.Register<SkyFormPage, string?>(nameof(SubmitButtonText), "Save");

    public static readonly StyledProperty<string?> CancelButtonTextProperty =
        AvaloniaProperty.Register<SkyFormPage, string?>(nameof(CancelButtonText), "Cancel");

    public static readonly StyledProperty<bool> IsFooterVisibleProperty =
        AvaloniaProperty.Register<SkyFormPage, bool>(nameof(IsFooterVisible), true);

    public SkyFormPage()
    {
        Classes.Add("sky");
        Classes.Add("sky-form-page");
    }

    [Content]
    public object? FormContent
    {
        get => GetValue(FormContentProperty);
        set => SetValue(FormContentProperty, value);
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public bool IsBackButtonVisible
    {
        get => GetValue(IsBackButtonVisibleProperty);
        set => SetValue(IsBackButtonVisibleProperty, value);
    }

    public ICommand? BackCommand
    {
        get => GetValue(BackCommandProperty);
        set => SetValue(BackCommandProperty, value);
    }

    public object? ActionContent
    {
        get => GetValue(ActionContentProperty);
        set => SetValue(ActionContentProperty, value);
    }

    public object? FooterContent
    {
        get => GetValue(FooterContentProperty);
        set => SetValue(FooterContentProperty, value);
    }

    public ICommand? SubmitCommand
    {
        get => GetValue(SubmitCommandProperty);
        set => SetValue(SubmitCommandProperty, value);
    }

    public ICommand? CancelCommand
    {
        get => GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }

    public string? SubmitButtonText
    {
        get => GetValue(SubmitButtonTextProperty);
        set => SetValue(SubmitButtonTextProperty, value);
    }

    public string? CancelButtonText
    {
        get => GetValue(CancelButtonTextProperty);
        set => SetValue(CancelButtonTextProperty, value);
    }

    public bool IsFooterVisible
    {
        get => GetValue(IsFooterVisibleProperty);
        set => SetValue(IsFooterVisibleProperty, value);
    }
}
