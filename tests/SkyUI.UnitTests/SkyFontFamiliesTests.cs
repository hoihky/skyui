using SkyUI.Fonts;

namespace SkyUI.UnitTests;

public class SkyFontFamiliesTests
{
    [Fact]
    public void Ui_stack_includes_inter_and_noto()
    {
        Assert.Contains("Inter", SkyFontFamilies.Ui, StringComparison.Ordinal);
        Assert.Contains("Noto Sans SC", SkyFontFamilies.Ui, StringComparison.Ordinal);
    }
}
