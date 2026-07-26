using SkyUI.Icons;

namespace SkyUI.UnitTests;

public class SkyIconGlyphTests
{
    public static IEnumerable<object[]> AllKinds()
    {
        foreach (SkyIconKind kind in Enum.GetValues<SkyIconKind>())
        {
            if (kind == SkyIconKind.None)
                continue;
            yield return new object[] { kind };
        }
    }

    [Theory]
    [MemberData(nameof(AllKinds))]
    public void Each_catalog_icon_has_path_data(SkyIconKind kind)
    {
        Assert.False(string.IsNullOrWhiteSpace(SkyIconGlyphs.TryGetPathData(kind)));
    }

    [Fact]
    public void Icon_sizes_match_design_tokens()
    {
        Assert.Equal(16, SkyIconGlyphs.ToPixels(SkyIconSize.Small));
        Assert.Equal(20, SkyIconGlyphs.ToPixels(SkyIconSize.Medium));
        Assert.Equal(24, SkyIconGlyphs.ToPixels(SkyIconSize.Large));
    }
}
