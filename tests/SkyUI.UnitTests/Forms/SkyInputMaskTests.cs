using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class SkyInputMaskTests
{
    [Theory]
    [InlineData(SkyInputMaskKind.Phone, "(999) 999-9999")]
    [InlineData(SkyInputMaskKind.CreditCard, "9999 9999 9999 9999")]
    public void ResolveMask_returns_builtin_patterns(SkyInputMaskKind kind, string expected)
    {
        Assert.Equal(expected, SkyInputMask.ResolveMask(kind, null));
    }

    [Fact]
    public void ResolveMask_uses_custom_mask_for_custom_kind()
    {
        Assert.Equal("aa-99", SkyInputMask.ResolveMask(SkyInputMaskKind.Custom, "aa-99"));
    }

    [Fact]
    public void Format_applies_phone_mask()
    {
        var formatted = SkyInputMask.Format("(999) 999-9999", "5551234567");
        Assert.Equal("(555) 123-4567", formatted);
    }

    [Fact]
    public void Format_applies_credit_card_mask()
    {
        var formatted = SkyInputMask.Format("9999 9999 9999 9999", "4111111111111111");
        Assert.Equal("4111 1111 1111 1111", formatted);
    }

    [Fact]
    public void ExtractRaw_strips_literals()
    {
        var raw = SkyInputMask.ExtractRaw("(999) 999-9999", "(555) 123-4567");
        Assert.Equal("5551234567", raw);
    }

    [Theory]
    [InlineData("(999) 999-9999", "5551234567", true)]
    [InlineData("(999) 999-9999", "55512", false)]
    [InlineData("", "anything", true)]
    public void IsComplete_checks_placeholder_count(string mask, string raw, bool expected)
    {
        Assert.Equal(expected, SkyInputMask.IsComplete(mask, raw));
    }

    [Theory]
    [InlineData(SkyInputMask.DigitPlaceholder, '5', true)]
    [InlineData(SkyInputMask.DigitPlaceholder, 'a', false)]
    [InlineData(SkyInputMask.LetterPlaceholder, 'A', true)]
    [InlineData(SkyInputMask.AnyPlaceholder, 'x', true)]
    public void MatchesPlaceholder_respects_kind(char placeholder, char input, bool expected)
    {
        Assert.Equal(expected, SkyInputMask.MatchesPlaceholder(placeholder, input));
    }
}
