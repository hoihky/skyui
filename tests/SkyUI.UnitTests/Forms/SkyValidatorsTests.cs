using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class SkyValidatorsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Required_rejects_empty_values(object? value)
    {
        var validator = SkyValidators.Required("Required");
        var result = validator.Validate(value);
        Assert.False(result.IsValid);
        Assert.Equal("Required", result.ErrorMessage);
    }

    [Fact]
    public void Required_accepts_non_empty_string()
    {
        var validator = SkyValidators.Required();
        Assert.True(validator.Validate("hello").IsValid);
    }

    [Theory]
    [InlineData("ab", 3, false)]
    [InlineData("abc", 3, true)]
    [InlineData("abcd", 3, true)]
    public void MinLength_validates_length(string value, int min, bool expectedValid)
    {
        var validator = SkyValidators.MinLength(min);
        Assert.Equal(expectedValid, validator.Validate(value).IsValid);
    }

    [Theory]
    [InlineData("abcdef", 5, false)]
    [InlineData("abc", 5, true)]
    public void MaxLength_validates_length(string value, int max, bool expectedValid)
    {
        var validator = SkyValidators.MaxLength(max);
        Assert.Equal(expectedValid, validator.Validate(value).IsValid);
    }

    [Theory]
    [InlineData("user@example.com", true)]
    [InlineData("not-an-email", false)]
    [InlineData("@example.com", false)]
    public void Email_validates_pattern(string value, bool expectedValid)
    {
        var validator = SkyValidators.Email();
        Assert.Equal(expectedValid, validator.Validate(value).IsValid);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(50, true)]
    [InlineData(100, true)]
    [InlineData(-1, false)]
    [InlineData(101, false)]
    [InlineData("50", true)]
    [InlineData("abc", false)]
    public void Range_validates_numeric_values(object value, bool expectedValid)
    {
        var validator = SkyValidators.Range(0, 100);
        Assert.Equal(expectedValid, validator.Validate(value).IsValid);
    }

    [Fact]
    public void Create_runs_custom_delegate()
    {
        var validator = SkyValidators.Create(v =>
            v is int i && i % 2 == 0
                ? SkyValidationResult.Valid
                : SkyValidationResult.Invalid("Even numbers only"));

        Assert.True(validator.Validate(2).IsValid);
        Assert.False(validator.Validate(3).IsValid);
    }
}
