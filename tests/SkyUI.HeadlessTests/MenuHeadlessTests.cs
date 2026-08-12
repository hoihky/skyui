using SkyUI.Controls;

namespace SkyUI.HeadlessTests;

public class MenuHeadlessTests
{
    [Fact]
    public void SkyAccelerator_parse_and_format_roundtrip()
    {
        var gesture = SkyAccelerator.Parse("Ctrl+O");
        Assert.NotNull(gesture);
        Assert.False(string.IsNullOrWhiteSpace(SkyAccelerator.Format(gesture)));
    }

    [Fact]
    public void SkyMenuGestures_save_is_control_s()
    {
        Assert.Equal(Avalonia.Input.Key.S, SkyMenuGestures.Save.Key);
        Assert.Equal(Avalonia.Input.KeyModifiers.Control, SkyMenuGestures.Save.KeyModifiers);
    }

    [Fact]
    public void SkyContextMenu_has_sky_class() =>
        Assert.Contains("sky", new SkyContextMenu().Classes);
}
