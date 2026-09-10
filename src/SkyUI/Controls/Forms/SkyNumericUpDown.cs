using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace SkyUI.Controls;

/// <summary>Numeric stepper with min/max, step, and culture-aware formatting.</summary>
public class SkyNumericUpDown : TemplatedControl
{
    public const string TextBoxPartName = "PART_TextBox";
    public const string IncrementButtonPartName = "PART_IncrementButton";
    public const string DecrementButtonPartName = "PART_DecrementButton";

    public static readonly StyledProperty<decimal> ValueProperty =
        AvaloniaProperty.Register<SkyNumericUpDown, decimal>(
            nameof(Value),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<decimal> MinimumProperty =
        AvaloniaProperty.Register<SkyNumericUpDown, decimal>(nameof(Minimum));

    public static readonly StyledProperty<decimal> MaximumProperty =
        AvaloniaProperty.Register<SkyNumericUpDown, decimal>(nameof(Maximum), 100);

    public static readonly StyledProperty<decimal> StepProperty =
        AvaloniaProperty.Register<SkyNumericUpDown, decimal>(nameof(Step), 1);

    public static readonly StyledProperty<bool> IsIntegerProperty =
        AvaloniaProperty.Register<SkyNumericUpDown, bool>(nameof(IsInteger));

    public static readonly StyledProperty<CultureInfo?> CultureProperty =
        AvaloniaProperty.Register<SkyNumericUpDown, CultureInfo?>(nameof(Culture));

    public static readonly StyledProperty<string?> FormatStringProperty =
        AvaloniaProperty.Register<SkyNumericUpDown, string?>(nameof(FormatString));

    private TextBox? textBox;

    static SkyNumericUpDown()
    {
        ValueProperty.Changed.AddClassHandler<SkyNumericUpDown>((control, e) =>
        {
            control.ClampAndFormat(e.GetNewValue<decimal>());
        });
        MinimumProperty.Changed.AddClassHandler<SkyNumericUpDown>((control, _) => control.ClampAndFormat(control.Value));
        MaximumProperty.Changed.AddClassHandler<SkyNumericUpDown>((control, _) => control.ClampAndFormat(control.Value));
        IsIntegerProperty.Changed.AddClassHandler<SkyNumericUpDown>((control, _) => control.ClampAndFormat(control.Value));
        CultureProperty.Changed.AddClassHandler<SkyNumericUpDown>((control, _) => control.UpdateDisplay());
        FormatStringProperty.Changed.AddClassHandler<SkyNumericUpDown>((control, _) => control.UpdateDisplay());
    }

    public SkyNumericUpDown()
    {
        Classes.Add("sky");
        Classes.Add("sky-numeric-up-down");
    }

    public decimal Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public decimal Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public decimal Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public decimal Step
    {
        get => GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    public bool IsInteger
    {
        get => GetValue(IsIntegerProperty);
        set => SetValue(IsIntegerProperty, value);
    }

    public CultureInfo? Culture
    {
        get => GetValue(CultureProperty);
        set => SetValue(CultureProperty, value);
    }

    public string? FormatString
    {
        get => GetValue(FormatStringProperty);
        set => SetValue(FormatStringProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        DetachHandlers();
        textBox = e.NameScope.Find(TextBoxPartName) as TextBox;
        var increment = e.NameScope.Find(IncrementButtonPartName) as Button;
        var decrement = e.NameScope.Find(DecrementButtonPartName) as Button;

        if (textBox is not null)
        {
            textBox.LostFocus += OnTextBoxLostFocus;
            textBox.KeyDown += OnTextBoxKeyDown;
        }

        if (increment is not null)
            increment.Click += OnIncrementClick;

        if (decrement is not null)
            decrement.Click += OnDecrementClick;

        UpdateDisplay();
    }

    private void DetachHandlers()
    {
        if (textBox is null)
            return;

        textBox.LostFocus -= OnTextBoxLostFocus;
        textBox.KeyDown -= OnTextBoxKeyDown;
    }

    private void OnIncrementClick(object? sender, RoutedEventArgs e) =>
        SetClampedValue(Value + Step);

    private void OnDecrementClick(object? sender, RoutedEventArgs e) =>
        SetClampedValue(Value - Step);

    private void OnTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Up)
        {
            SetClampedValue(Value + Step);
            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            SetClampedValue(Value - Step);
            e.Handled = true;
        }
    }

    private void OnTextBoxLostFocus(object? sender, RoutedEventArgs e)
    {
        if (textBox is null)
            return;

        if (TryParse(textBox.Text, out var parsed))
            SetClampedValue(parsed);
        else
            UpdateDisplay();
    }

    private void ClampAndFormat(decimal newValue)
    {
        var clamped = Clamp(newValue);
        if (clamped != newValue)
        {
            SetCurrentValue(ValueProperty, clamped);
            return;
        }

        UpdateDisplay();
    }

    private void SetClampedValue(decimal value) =>
        Value = Clamp(value);

    private decimal Clamp(decimal value)
    {
        var clamped = Math.Clamp(value, Minimum, Maximum);
        return IsInteger ? decimal.Truncate(clamped) : clamped;
    }

    private void UpdateDisplay()
    {
        if (textBox is null)
            return;

        textBox.Text = FormatValue(Value);
    }

    private string FormatValue(decimal value)
    {
        var culture = Culture ?? CultureInfo.CurrentCulture;
        if (!string.IsNullOrWhiteSpace(FormatString))
            return value.ToString(FormatString, culture);

        return IsInteger
            ? decimal.Truncate(value).ToString("N0", culture)
            : value.ToString("N", culture);
    }

    private bool TryParse(string? text, out decimal value)
    {
        var culture = Culture ?? CultureInfo.CurrentCulture;
        if (decimal.TryParse(text, NumberStyles.Number, culture, out value))
            return true;

        value = 0;
        return false;
    }
}
