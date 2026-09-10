using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace SkyUI.Controls;

/// <summary>Start/end date picker with quick presets.</summary>
public class SkyDateRangePicker : TemplatedControl
{
    public const string PresetTodayPartName = "PART_PresetToday";
    public const string PresetWeekPartName = "PART_PresetWeek";
    public const string StartDatePartName = "PART_StartDate";
    public const string EndDatePartName = "PART_EndDate";

    public static readonly StyledProperty<DateTime?> StartDateProperty =
        AvaloniaProperty.Register<SkyDateRangePicker, DateTime?>(
            nameof(StartDate),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<DateTime?> EndDateProperty =
        AvaloniaProperty.Register<SkyDateRangePicker, DateTime?>(
            nameof(EndDate),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<SkyDateRangePreset> SelectedPresetProperty =
        AvaloniaProperty.Register<SkyDateRangePicker, SkyDateRangePreset>(
            nameof(SelectedPreset),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<CultureInfo?> CultureProperty =
        AvaloniaProperty.Register<SkyDateRangePicker, CultureInfo?>(nameof(Culture));

    private SkyDatePicker? startDatePicker;
    private SkyDatePicker? endDatePicker;
    private bool isApplyingPreset;

    static SkyDateRangePicker()
    {
        StartDateProperty.Changed.AddClassHandler<SkyDateRangePicker>((picker, _) => picker.OnManualDateChanged());
        EndDateProperty.Changed.AddClassHandler<SkyDateRangePicker>((picker, _) => picker.OnManualDateChanged());
        SelectedPresetProperty.Changed.AddClassHandler<SkyDateRangePicker>((picker, e) =>
            picker.ApplyPreset(e.GetNewValue<SkyDateRangePreset>()));
    }

    public SkyDateRangePicker()
    {
        Classes.Add("sky");
        Classes.Add("sky-date-range-picker");
    }

    public DateTime? StartDate
    {
        get => GetValue(StartDateProperty);
        set => SetValue(StartDateProperty, value);
    }

    public DateTime? EndDate
    {
        get => GetValue(EndDateProperty);
        set => SetValue(EndDateProperty, value);
    }

    public SkyDateRangePreset SelectedPreset
    {
        get => GetValue(SelectedPresetProperty);
        set => SetValue(SelectedPresetProperty, value);
    }

    public CultureInfo? Culture
    {
        get => GetValue(CultureProperty);
        set => SetValue(CultureProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var today = e.NameScope.Find(PresetTodayPartName) as Button;
        var week = e.NameScope.Find(PresetWeekPartName) as Button;
        startDatePicker = e.NameScope.Find(StartDatePartName) as SkyDatePicker;
        endDatePicker = e.NameScope.Find(EndDatePartName) as SkyDatePicker;

        if (today is not null)
            today.Click += (_, _) => SelectedPreset = SkyDateRangePreset.Today;

        if (week is not null)
            week.Click += (_, _) => SelectedPreset = SkyDateRangePreset.ThisWeek;

        if (startDatePicker is not null)
            startDatePicker.Culture = Culture;

        if (endDatePicker is not null)
            endDatePicker.Culture = Culture;

        ApplyPreset(SelectedPreset);
    }

    private void OnManualDateChanged()
    {
        if (isApplyingPreset)
            return;

        SelectedPreset = SkyDateRangePreset.Custom;
    }

    private void ApplyPreset(SkyDateRangePreset preset)
    {
        if (preset == SkyDateRangePreset.Custom)
            return;

        isApplyingPreset = true;
        try
        {
            var today = DateTime.Today;
            switch (preset)
            {
                case SkyDateRangePreset.Today:
                    StartDate = today;
                    EndDate = today;
                    break;
                case SkyDateRangePreset.ThisWeek:
                    var culture = Culture ?? CultureInfo.CurrentCulture;
                    var dayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;
                    var offset = ((int)today.DayOfWeek - (int)dayOfWeek + 7) % 7;
                    StartDate = today.AddDays(-offset);
                    EndDate = StartDate.Value.AddDays(6);
                    break;
            }
        }
        finally
        {
            isApplyingPreset = false;
        }
    }
}
