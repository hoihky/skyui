using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace SkyUI.Controls;

/// <summary>Slider with optional label and value readout for forms.</summary>
public class SkySlider : TemplatedControl
{
    public const string SliderPartName = "PART_Slider";
    public const string ValueLabelPartName = "PART_ValueLabel";

    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<SkySlider, string?>(nameof(Label));

    public static readonly StyledProperty<bool> ShowValueLabelProperty =
        AvaloniaProperty.Register<SkySlider, bool>(nameof(ShowValueLabel), true);

    public static readonly StyledProperty<string?> ValueFormatProperty =
        AvaloniaProperty.Register<SkySlider, string?>(nameof(ValueFormat), "{0:0}");

    public static readonly StyledProperty<double> ValueProperty =
        RangeBase.ValueProperty.AddOwner<SkySlider>();

    public static readonly StyledProperty<double> MinimumProperty =
        RangeBase.MinimumProperty.AddOwner<SkySlider>();

    public static readonly StyledProperty<double> MaximumProperty =
        RangeBase.MaximumProperty.AddOwner<SkySlider>();

    public static readonly StyledProperty<double> TickFrequencyProperty =
        Slider.TickFrequencyProperty.AddOwner<SkySlider>();

    public static readonly StyledProperty<bool> IsSnapToTickEnabledProperty =
        Slider.IsSnapToTickEnabledProperty.AddOwner<SkySlider>();

    static SkySlider()
    {
        ValueProperty.Changed.AddClassHandler<SkySlider>((s, _) => s.UpdateValueLabel());
    }

    private Slider? _slider;
    private TextBlock? _valueLabel;

    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public bool ShowValueLabel
    {
        get => GetValue(ShowValueLabelProperty);
        set => SetValue(ShowValueLabelProperty, value);
    }

    public string? ValueFormat
    {
        get => GetValue(ValueFormatProperty);
        set => SetValue(ValueFormatProperty, value);
    }

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double TickFrequency
    {
        get => GetValue(TickFrequencyProperty);
        set => SetValue(TickFrequencyProperty, value);
    }

    public bool IsSnapToTickEnabled
    {
        get => GetValue(IsSnapToTickEnabledProperty);
        set => SetValue(IsSnapToTickEnabledProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _slider = e.NameScope.Find(SliderPartName) as Slider;
        _valueLabel = e.NameScope.Find(ValueLabelPartName) as TextBlock;
        UpdateValueLabel();
    }

    private void UpdateValueLabel()
    {
        if (_valueLabel is null)
            return;

        var format = ValueFormat ?? "{0:0}";
        _valueLabel.Text = string.Format(format, Value);
    }
}
