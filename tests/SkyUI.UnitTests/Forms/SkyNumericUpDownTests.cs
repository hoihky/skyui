using System.Globalization;
using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class SkyNumericUpDownTests
{
    [Fact]
    public void Has_sky_classes()
    {
        var control = new SkyNumericUpDown();
        Assert.Contains("sky", control.Classes);
        Assert.Contains("sky-numeric-up-down", control.Classes);
    }

    [Fact]
    public void Default_range_is_zero_to_hundred()
    {
        var control = new SkyNumericUpDown();
        Assert.Equal(0, control.Minimum);
        Assert.Equal(100, control.Maximum);
        Assert.Equal(1, control.Step);
    }

    [Fact]
    public void Value_clamps_to_minimum()
    {
        var control = new SkyNumericUpDown { Minimum = 10, Maximum = 100, Value = 5 };
        Assert.Equal(10, control.Value);
    }

    [Fact]
    public void Value_clamps_to_maximum()
    {
        var control = new SkyNumericUpDown { Minimum = 0, Maximum = 50, Value = 75 };
        Assert.Equal(50, control.Value);
    }

    [Fact]
    public void Integer_mode_truncates_fractional_values()
    {
        var control = new SkyNumericUpDown
        {
            IsInteger = true,
            Minimum = 0,
            Maximum = 100,
            Value = 12.9m
        };

        Assert.Equal(12, control.Value);
    }

    [Fact]
    public void Minimum_change_reclamps_current_value()
    {
        var control = new SkyNumericUpDown { Value = 5, Maximum = 100 };
        control.Minimum = 10;
        Assert.Equal(10, control.Value);
    }

    [Fact]
    public void Culture_and_format_string_properties_roundtrip()
    {
        var culture = CultureInfo.GetCultureInfo("de-DE");
        var control = new SkyNumericUpDown
        {
            Culture = culture,
            FormatString = "N2",
            Value = 1234.5m
        };

        Assert.Equal(culture, control.Culture);
        Assert.Equal("N2", control.FormatString);
    }
}
