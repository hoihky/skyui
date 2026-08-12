using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class SkyValidationResultTests
{
    [Fact]
    public void Valid_result_has_no_error()
    {
        var result = SkyValidationResult.Valid;
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Invalid_result_stores_message()
    {
        var result = SkyValidationResult.Invalid("Required");
        Assert.False(result.IsValid);
        Assert.Equal("Required", result.ErrorMessage);
    }
}
