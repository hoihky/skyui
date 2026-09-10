using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class FormsControlsTests
{
    [Fact]
    public void Form_controls_instantiate()
    {
        Assert.NotNull(new SkyFormField());
        Assert.NotNull(new SkyRadioGroup());
        Assert.NotNull(new SkyRadioGroupItem());
        Assert.NotNull(new SkySlider());
        Assert.NotNull(new SkySearchBox());
        Assert.NotNull(new SkyPasswordBox());
        Assert.NotNull(new SkyMaskedTextBox());
        Assert.NotNull(new SkyNumericUpDown());
        Assert.NotNull(new SkyAutocomplete());
        Assert.NotNull(new SkyComboBoxField());
        Assert.NotNull(new SkyDateRangePicker());
        Assert.NotNull(new SkyValidationSummary());
        Assert.NotNull(new SkyEmptyState());
    }

    [Fact]
    public void SkySearchBox_has_sky_classes()
    {
        var search = new SkySearchBox();
        Assert.Contains("sky", search.Classes);
        Assert.Contains("sky-search", search.Classes);
    }

    [Fact]
    public void SkyPasswordBox_has_sky_classes_and_password_char()
    {
        var password = new SkyPasswordBox();
        Assert.Contains("sky", password.Classes);
        Assert.Contains("sky-password", password.Classes);
        Assert.Equal('•', password.PasswordChar);
    }
}
