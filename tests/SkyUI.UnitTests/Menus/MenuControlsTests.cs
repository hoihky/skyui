using Avalonia.Controls;
using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class MenuControlsTests
{
    [Fact]
    public void Menu_controls_instantiate()
    {
        Assert.NotNull(new SkyMenuBar());
        Assert.NotNull(new SkyContextMenu());
        Assert.NotNull(new SkyMenuFlyout());
    }

    [Fact]
    public void SkyMenuBar_has_sky_class() =>
        Assert.Contains("sky", new SkyMenuBar().Classes);

    [Fact]
    public void SkyContextMenu_has_sky_class() =>
        Assert.Contains("sky", new SkyContextMenu().Classes);

    [Fact]
    public void SkyMenuFlyout_adds_presenter_sky_class() =>
        Assert.Contains("sky", new SkyMenuFlyout().FlyoutPresenterClasses);

    [Fact]
    public void SkyAcceleratorFormatConverter_formats_gesture()
    {
        var text = SkyAcceleratorFormatConverter.Instance.Convert(
            SkyMenuGestures.Save,
            typeof(string),
            null,
            System.Globalization.CultureInfo.InvariantCulture);

        Assert.IsType<string>(text);
        Assert.False(string.IsNullOrWhiteSpace((string)text!));
    }
}
