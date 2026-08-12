using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class CompositeSkyValidatorTests
{
    [Fact]
    public void Returns_first_failure()
    {
        var composite = new CompositeSkyValidator(
            SkyValidators.Required("Required"),
            SkyValidators.MinLength(5, "Too short"));

        var result = composite.Validate("");
        Assert.False(result.IsValid);
        Assert.Equal("Required", result.ErrorMessage);
    }

    [Fact]
    public void Runs_subsequent_validators_after_first_passes()
    {
        var composite = new CompositeSkyValidator(
            SkyValidators.Required(),
            SkyValidators.MinLength(5, "Too short"));

        var result = composite.Validate("abc");
        Assert.False(result.IsValid);
        Assert.Equal("Too short", result.ErrorMessage);
    }

    [Fact]
    public void Returns_valid_when_all_pass()
    {
        var composite = new CompositeSkyValidator(
            SkyValidators.Required(),
            SkyValidators.MinLength(3),
            SkyValidators.Email());

        Assert.True(composite.Validate("user@example.com").IsValid);
    }

    [Fact]
    public void Empty_composite_always_valid()
    {
        var composite = new CompositeSkyValidator();
        Assert.True(composite.Validate(null).IsValid);
    }
}
