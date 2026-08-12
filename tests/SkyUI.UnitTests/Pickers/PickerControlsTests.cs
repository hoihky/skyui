using System.Globalization;
using Avalonia.Controls;
using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class PickerControlsTests
{
    [Fact]
    public void Picker_controls_instantiate()
    {
        Assert.NotNull(new SkyDatePicker());
        Assert.NotNull(new SkyCalendar());
        Assert.NotNull(new SkyTimePicker());
    }

    [Fact]
    public void SkyDatePicker_has_sky_classes()
    {
        var picker = new SkyDatePicker();
        Assert.Contains("sky", picker.Classes);
        Assert.Contains("sky-date-picker", picker.Classes);
    }

    [Fact]
    public void SkyCalendar_defaults_to_month_view()
    {
        var calendar = new SkyCalendar();
        Assert.Equal(CalendarMode.Month, calendar.DisplayMode);
        Assert.True(calendar.IsTodayHighlighted);
    }

    [Fact]
    public void SkyTimePicker_has_five_minute_increment() =>
        Assert.Equal(5, new SkyTimePicker().MinuteIncrement);

    [Fact]
    public void Culture_change_updates_date_picker_format()
    {
        var picker = new SkyDatePicker { Culture = new CultureInfo("de-DE") };
        Assert.Equal(CalendarDatePickerFormat.Custom, picker.SelectedDateFormat);
        Assert.Equal(new CultureInfo("de-DE").DateTimeFormat.ShortDatePattern, picker.CustomDateFormatString);
    }

    [Fact]
    public void Culture_change_updates_time_picker_clock()
    {
        var picker = new SkyTimePicker { Culture = new CultureInfo("en-US") };
        Assert.Equal("12HourClock", picker.ClockIdentifier);

        picker.Culture = new CultureInfo("de-DE");
        Assert.Equal("24HourClock", picker.ClockIdentifier);
    }

    [Fact]
    public void SkyCalendar_pointer_wheel_handler_is_overridden()
    {
        var method = typeof(SkyCalendar).GetMethod(
            "OnPointerWheelChanged",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly);

        Assert.NotNull(method);
        Assert.Equal(typeof(SkyCalendar), method!.DeclaringType);
    }

    [Fact]
    public void SkyPickerFormatConverter_formats_date()
    {
        var text = SkyPickerFormatConverter.DateInstance.Convert(
            new DateTime(2026, 8, 12),
            typeof(string),
            new CultureInfo("en-US"),
            CultureInfo.InvariantCulture);

        Assert.IsType<string>(text);
        Assert.False(string.IsNullOrWhiteSpace((string)text!));
    }
}
