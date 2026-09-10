using System.Globalization;
using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class SkyDateRangePickerTests
{
    [Fact]
    public void Has_sky_classes()
    {
        var picker = new SkyDateRangePicker();
        Assert.Contains("sky", picker.Classes);
        Assert.Contains("sky-date-range-picker", picker.Classes);
    }

    [Fact]
    public void SelectedPreset_defaults_to_custom()
    {
        var picker = new SkyDateRangePicker();
        Assert.Equal(SkyDateRangePreset.Custom, picker.SelectedPreset);
    }

    [Fact]
    public void Today_preset_sets_start_and_end_to_today()
    {
        var picker = new SkyDateRangePicker();
        picker.SelectedPreset = SkyDateRangePreset.Today;

        var today = DateTime.Today;
        Assert.Equal(today, picker.StartDate);
        Assert.Equal(today, picker.EndDate);
    }

    [Fact]
    public void ThisWeek_preset_spans_seven_days_from_first_day_of_week()
    {
        var culture = CultureInfo.GetCultureInfo("en-US");
        var picker = new SkyDateRangePicker { Culture = culture };
        picker.SelectedPreset = SkyDateRangePreset.ThisWeek;

        Assert.NotNull(picker.StartDate);
        Assert.NotNull(picker.EndDate);
        Assert.Equal(6, (picker.EndDate!.Value - picker.StartDate!.Value).Days);

        var firstDay = culture.DateTimeFormat.FirstDayOfWeek;
        Assert.Equal(firstDay, picker.StartDate!.Value.DayOfWeek);
    }

    [Fact]
    public void Manual_date_change_sets_custom_preset()
    {
        var picker = new SkyDateRangePicker
        {
            SelectedPreset = SkyDateRangePreset.Today
        };

        picker.StartDate = DateTime.Today.AddDays(-3);
        Assert.Equal(SkyDateRangePreset.Custom, picker.SelectedPreset);
    }

    [Fact]
    public void SkyDateRangeValue_record_holds_dates()
    {
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 1, 31);
        var value = new SkyDateRangeValue(start, end);

        Assert.Equal(start, value.Start);
        Assert.Equal(end, value.End);
    }
}
