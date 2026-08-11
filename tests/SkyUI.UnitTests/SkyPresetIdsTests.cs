using SkyUI.Themes.Sky;

namespace SkyUI.UnitTests;

public class SkyPresetIdsTests
{
    [Fact]
    public void ContentFirstDark_id_is_stable() =>
        Assert.Equal("ContentFirstDark", SkyPresetIds.ContentFirstDark);

    [Fact]
    public void ContentFirstDark_uri_matches_preset_file() =>
        Assert.Contains("Presets/ContentFirstDark/ContentFirstDark.axaml", SkyPresetUris.ContentFirstDark);
}
