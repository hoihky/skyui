using System.Globalization;
using SkyUI.Controls;

namespace SkyUI.HeadlessTests;

public class PickersHeadlessTests
{
    [Fact]
    public void SkyDatePicker_applies_german_culture()
    {
        var picker = new SkyDatePicker { Culture = new CultureInfo("de-DE") };
        Assert.Equal(DayOfWeek.Monday, picker.FirstDayOfWeek);
    }

    [Fact]
    public void SkyCalendar_has_sky_class() =>
        Assert.Contains("sky", new SkyCalendar().Classes);

    [Fact]
    public void SkyTimePicker_formats_with_culture()
    {
        var text = SkyPickerFormat.FormatTime(new TimeSpan(14, 30, 0), new CultureInfo("de-DE"));
        Assert.Contains("14", text);
    }
}
