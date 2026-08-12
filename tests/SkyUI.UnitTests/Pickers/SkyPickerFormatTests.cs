using System.Globalization;
using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class SkyPickerFormatTests
{
    [Theory]
    [InlineData("en-US", "8/12/2026")]
    [InlineData("de-DE", "12.08.2026")]
    public void FormatDate_uses_culture_short_pattern(string cultureName, string expected)
    {
        var culture = new CultureInfo(cultureName);
        var text = SkyPickerFormat.FormatDate(new DateTime(2026, 8, 12), culture);
        Assert.Equal(expected, text);
    }

    [Fact]
    public void FormatTime_returns_empty_for_null() =>
        Assert.Equal(string.Empty, SkyPickerFormat.FormatTime(null));

    [Fact]
    public void FormatDate_returns_empty_for_null() =>
        Assert.Equal(string.Empty, SkyPickerFormat.FormatDate(null));

    [Theory]
    [InlineData("en-US", true)]
    [InlineData("de-DE", false)]
    [InlineData("ja-JP", false)]
    public void Uses12HourClock_matches_culture(string cultureName, bool expected) =>
        Assert.Equal(expected, SkyPickerFormat.Uses12HourClock(new CultureInfo(cultureName)));

    [Fact]
    public void TryParseDate_parses_culture_specific_text()
    {
        var culture = new CultureInfo("en-US");
        Assert.True(SkyPickerFormat.TryParseDate("8/12/2026", out var date, culture));
        Assert.Equal(new DateTime(2026, 8, 12), date);
    }

    [Fact]
    public void Resolve_falls_back_to_current_culture_when_null() =>
        Assert.Equal(CultureInfo.CurrentCulture, SkyPickerFormat.Resolve(null));
}
