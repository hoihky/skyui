using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class SkySliderTests
{
    [Fact]
    public void Default_range_is_zero_to_hundred()
    {
        var slider = new SkySlider();
        Assert.Equal(0, slider.Minimum);
        Assert.Equal(100, slider.Maximum);
    }

    [Fact]
    public void Value_can_be_set()
    {
        var slider = new SkySlider { Value = 42 };
        Assert.Equal(42, slider.Value);
    }

    [Fact]
    public void ShowValueLabel_defaults_to_true()
    {
        var slider = new SkySlider();
        Assert.True(slider.ShowValueLabel);
    }

    [Fact]
    public void ValueFormat_defaults_to_integer_format()
    {
        var slider = new SkySlider();
        Assert.Equal("{0:0}", slider.ValueFormat);
    }
}
