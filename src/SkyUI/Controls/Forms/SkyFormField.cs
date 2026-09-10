using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Metadata;

namespace SkyUI.Controls;

/// <summary>
/// Wraps a form input with label, hint, required indicator, and error message.
/// Bind <see cref="ErrorMessage"/> from a view model or call <see cref="Validate"/> with <see cref="Validator"/>.
/// </summary>
[PseudoClasses("error")]
[TemplatePart(InputHostPartName, typeof(ContentPresenter))]
public class SkyFormField : TemplatedControl
{
    public const string LabelPartName = "PART_Label";
    public const string HintPartName = "PART_Hint";
    public const string ErrorPartName = "PART_Error";
    public const string InputHostPartName = "PART_InputHost";
    public const string RequiredIndicatorPartName = "PART_RequiredIndicator";

    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<SkyFormField, string?>(nameof(Label));

    public static readonly StyledProperty<string?> HintProperty =
        AvaloniaProperty.Register<SkyFormField, string?>(nameof(Hint));

    public static readonly StyledProperty<string?> ErrorMessageProperty =
        AvaloniaProperty.Register<SkyFormField, string?>(nameof(ErrorMessage));

    public static readonly StyledProperty<bool> IsRequiredProperty =
        AvaloniaProperty.Register<SkyFormField, bool>(nameof(IsRequired));

    public static readonly StyledProperty<bool> ValidateOnLostFocusProperty =
        AvaloniaProperty.Register<SkyFormField, bool>(nameof(ValidateOnLostFocus), true);

    public static readonly StyledProperty<ISkyValidator?> ValidatorProperty =
        AvaloniaProperty.Register<SkyFormField, ISkyValidator?>(nameof(Validator));

    public static readonly StyledProperty<object?> ContentProperty =
        ContentControl.ContentProperty.AddOwner<SkyFormField>();

    static SkyFormField()
    {
        ErrorMessageProperty.Changed.AddClassHandler<SkyFormField>((f, _) => f.SyncErrorState());
        IsRequiredProperty.Changed.AddClassHandler<SkyFormField>((f, _) => f.SyncRequiredState());
    }

    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string? Hint
    {
        get => GetValue(HintProperty);
        set => SetValue(HintProperty, value);
    }

    public string? ErrorMessage
    {
        get => GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }

    public bool IsRequired
    {
        get => GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    public bool ValidateOnLostFocus
    {
        get => GetValue(ValidateOnLostFocusProperty);
        set => SetValue(ValidateOnLostFocusProperty, value);
    }

    public ISkyValidator? Validator
    {
        get => GetValue(ValidatorProperty);
        set => SetValue(ValidatorProperty, value);
    }

    [Content]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>True when <see cref="ErrorMessage"/> is non-empty.</summary>
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    /// <summary>Runs <see cref="Validator"/> and updates <see cref="ErrorMessage"/>.</summary>
    public bool Validate()
    {
        if (Validator is null)
            return !HasError;

        var result = Validator.Validate(GetInputValue());
        ErrorMessage = result.IsValid ? null : result.ErrorMessage;
        return result.IsValid;
    }

    /// <summary>Clears <see cref="ErrorMessage"/>.</summary>
    public void ClearValidation() => ErrorMessage = null;

    private Control? _requiredIndicator;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _requiredIndicator = e.NameScope.Find(RequiredIndicatorPartName) as Control;
        SyncErrorState();
        SyncRequiredState();
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        if (e.Source is InputElement input && IsDescendantInput(input))
            input.LostFocus += OnInputLostFocus;
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        if (e.Source is InputElement input && IsDescendantInput(input))
            input.LostFocus -= OnInputLostFocus;

        base.OnDetachedFromLogicalTree(e);
    }

    private bool IsDescendantInput(InputElement input) =>
        input != this && this.IsLogicalAncestorOf(input);

    private void OnInputLostFocus(object? sender, RoutedEventArgs e)
    {
        if (ValidateOnLostFocus)
            Validate();
    }

    private void SyncErrorState() => PseudoClasses.Set(":error", HasError);

    private void SyncRequiredState()
    {
        if (_requiredIndicator is not null)
            _requiredIndicator.IsVisible = IsRequired;
    }

    /// <summary>Reads the current value from the hosted input control.</summary>
    public object? GetInputValue()
    {
        if (Content is not Control input)
            return Content;

        return input switch
        {
            SkySearchBox searchBox => searchBox.Text,
            SkyPasswordBox passwordBox => passwordBox.Text,
            SkyMaskedTextBox maskedTextBox => maskedTextBox.RawText,
            SkyAutocomplete autocomplete => autocomplete.SelectedItem ?? autocomplete.Text,
            SkyNumericUpDown numericUpDown => numericUpDown.Value,
            SkyDateRangePicker dateRange => new SkyDateRangeValue(dateRange.StartDate, dateRange.EndDate),
            TextBox textBox => textBox.Text,
            SkySlider slider => slider.Value,
            Slider nativeSlider => nativeSlider.Value,
            CheckBox checkBox => checkBox.IsChecked,
            ToggleSwitch toggle => toggle.IsChecked,
            ComboBox comboBox => comboBox.SelectedItem,
            SkyComboBoxField comboField => comboField.SelectedItem,
            _ => input.GetValue(TextBox.TextProperty) is string text ? text : input
        };
    }
}
