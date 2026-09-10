using SkyUI.Controls;

namespace SkyUI.HeadlessTests;

public class FormsHeadlessTests
{
    [Fact]
    public void SkyFormField_validate_with_composite_validator()
    {
        var field = new SkyFormField
        {
            Content = new SkySearchBox { Text = "bad" },
            Validator = new CompositeSkyValidator(
                SkyValidators.Required(),
                SkyValidators.MinLength(5, "Too short"))
        };

        Assert.False(field.Validate());
        Assert.Equal("Too short", field.ErrorMessage);

        ((SkySearchBox)field.Content!).Text = "valid-query";
        Assert.True(field.Validate());
        Assert.Null(field.ErrorMessage);
    }

    [Fact]
    public void SkyRadioGroup_selected_value_roundtrip()
    {
        var group = new SkyRadioGroup();
        group.Items.Add(new SkyRadioGroupItem { Label = "Dark", Value = "dark" });
        group.Items.Add(new SkyRadioGroupItem { Label = "Light", Value = "light" });

        group.SelectedValue = "light";
        Assert.Equal("light", group.SelectedValue);
    }

    [Fact]
    public void SkySlider_value_binding_roundtrip()
    {
        var slider = new SkySlider { Minimum = 0, Maximum = 100, Value = 25 };
        slider.Value = 75;
        Assert.Equal(75, slider.Value);
    }

    [Fact]
    public void SkyPasswordBox_masks_text()
    {
        var password = new SkyPasswordBox { Text = "secret" };
        Assert.Equal('•', password.PasswordChar);
        Assert.Equal("secret", password.Text);
    }

    [Fact]
    public void SkyMaskedTextBox_phone_mask_roundtrip()
    {
        var masked = new SkyMaskedTextBox
        {
            MaskKind = SkyInputMaskKind.Phone,
            RawText = "5551234567"
        };

        Assert.Equal("(555) 123-4567", masked.Text);
        Assert.True(masked.IsMaskComplete);
    }

    [Fact]
    public void SkyNumericUpDown_clamps_value()
    {
        var numeric = new SkyNumericUpDown { Minimum = 0, Maximum = 10, Value = 15 };
        Assert.Equal(10, numeric.Value);
    }

    [Fact]
    public void SkyDateRangePicker_today_preset()
    {
        var picker = new SkyDateRangePicker();
        picker.SelectedPreset = SkyDateRangePreset.Today;
        Assert.Equal(DateTime.Today, picker.StartDate);
        Assert.Equal(DateTime.Today, picker.EndDate);
    }

    [Fact]
    public void SkyValidationSummary_collects_errors()
    {
        var summary = new SkyValidationSummary();
        summary.SetErrors([new SkyValidationSummaryItem("field", "Error message")]);
        Assert.True(summary.HasErrors);
        Assert.Single(summary.Items);
    }

    [Fact]
    public void SkyComboBoxField_selected_item_roundtrip()
    {
        var field = new SkyComboBoxField
        {
            ItemsSource = new[] { "A", "B" },
            SelectedItem = "A"
        };

        field.SelectedItem = "B";
        Assert.Equal("B", field.GetInputValue());
    }
}
