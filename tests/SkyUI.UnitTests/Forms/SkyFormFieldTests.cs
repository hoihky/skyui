using Avalonia.Controls;
using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class SkyFormFieldTests
{
    [Fact]
    public void HasError_when_error_message_set()
    {
        var field = new SkyFormField { ErrorMessage = "Invalid" };
        Assert.True(field.HasError);
    }

    [Fact]
    public void HasError_false_when_message_cleared()
    {
        var field = new SkyFormField { ErrorMessage = "Invalid" };
        field.ErrorMessage = null;
        Assert.False(field.HasError);
    }

    [Fact]
    public void Validate_updates_error_message_from_validator()
    {
        var field = new SkyFormField
        {
            Content = new TextBox { Text = "" },
            Validator = SkyValidators.Required("Required")
        };

        Assert.False(field.Validate());
        Assert.Equal("Required", field.ErrorMessage);
    }

    [Fact]
    public void Validate_clears_error_message_on_success()
    {
        var field = new SkyFormField
        {
            Content = new TextBox { Text = "ok@example.com" },
            ErrorMessage = "Old error",
            Validator = new CompositeSkyValidator(
                SkyValidators.Required(),
                SkyValidators.Email())
        };

        Assert.True(field.Validate());
        Assert.Null(field.ErrorMessage);
    }

    [Fact]
    public void ClearValidation_removes_error_message()
    {
        var field = new SkyFormField { ErrorMessage = "Invalid" };
        field.ClearValidation();
        Assert.Null(field.ErrorMessage);
    }

    [Fact]
    public void ReadInputValue_reads_textbox_text()
    {
        var field = new SkyFormField
        {
            Content = new TextBox { Text = "hello" }
        };

        Assert.Equal("hello", field.GetInputValue());
    }

    [Fact]
    public void GetInputValue_reads_search_and_password_boxes()
    {
        var searchField = new SkyFormField { Content = new SkySearchBox { Text = "query" } };
        var passwordField = new SkyFormField { Content = new SkyPasswordBox { Text = "secret" } };

        Assert.Equal("query", searchField.GetInputValue());
        Assert.Equal("secret", passwordField.GetInputValue());
    }

    [Fact]
    public void GetInputValue_reads_slider_value()
    {
        var field = new SkyFormField
        {
            Content = new SkySlider { Value = 42 }
        };

        Assert.Equal(42d, field.GetInputValue());
    }

    [Fact]
    public void Validate_without_validator_returns_has_error_state()
    {
        var field = new SkyFormField { ErrorMessage = "Existing" };
        Assert.False(field.Validate());
    }

    [Fact]
    public void Validate_without_validator_returns_true_when_no_error()
    {
        var field = new SkyFormField();
        Assert.True(field.Validate());
    }

    [Fact]
    public void GetInputValue_reads_masked_text_box_raw_text()
    {
        var field = new SkyFormField
        {
            Content = new SkyMaskedTextBox
            {
                MaskKind = SkyInputMaskKind.Phone,
                RawText = "5551234567"
            }
        };

        Assert.Equal("5551234567", field.GetInputValue());
    }

    [Fact]
    public void GetInputValue_reads_numeric_up_down_value()
    {
        var field = new SkyFormField
        {
            Content = new SkyNumericUpDown { Value = 42 }
        };

        Assert.Equal(42m, field.GetInputValue());
    }

    [Fact]
    public void GetInputValue_reads_autocomplete_selection()
    {
        var field = new SkyFormField
        {
            Content = new SkyAutocomplete
            {
                Text = "typed",
                SelectedItem = "selected"
            }
        };

        Assert.Equal("selected", field.GetInputValue());
    }

    [Fact]
    public void GetInputValue_reads_date_range_picker()
    {
        var start = DateTime.Today;
        var end = DateTime.Today.AddDays(7);
        var field = new SkyFormField
        {
            Content = new SkyDateRangePicker
            {
                StartDate = start,
                EndDate = end
            }
        };

        var value = Assert.IsType<SkyDateRangeValue>(field.GetInputValue());
        Assert.Equal(start, value.Start);
        Assert.Equal(end, value.End);
    }
}
