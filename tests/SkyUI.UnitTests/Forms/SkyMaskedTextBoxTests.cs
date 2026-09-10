using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class SkyMaskedTextBoxTests
{
    [Fact]
    public void Has_sky_classes()
    {
        var box = new SkyMaskedTextBox();
        Assert.Contains("sky", box.Classes);
        Assert.Contains("sky-masked-text", box.Classes);
    }

    [Fact]
    public void Phone_mask_formats_raw_text()
    {
        var box = new SkyMaskedTextBox
        {
            MaskKind = SkyInputMaskKind.Phone,
            RawText = "5551234567"
        };

        Assert.Equal("(555) 123-4567", box.Text);
        Assert.True(box.IsMaskComplete);
    }

    [Fact]
    public void Credit_card_mask_formats_raw_text()
    {
        var box = new SkyMaskedTextBox
        {
            MaskKind = SkyInputMaskKind.CreditCard,
            RawText = "4111111111111111"
        };

        Assert.Equal("4111 1111 1111 1111", box.Text);
    }

    [Fact]
    public void Custom_mask_uses_provided_pattern()
    {
        var box = new SkyMaskedTextBox
        {
            MaskKind = SkyInputMaskKind.Custom,
            Mask = "99-aa",
            RawText = "12ab"
        };

        Assert.Equal("12-ab", box.Text);
    }

    [Fact]
    public void RawText_defaults_to_empty()
    {
        var box = new SkyMaskedTextBox();
        Assert.Equal(string.Empty, box.RawText);
    }
}
