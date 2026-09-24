using SkyUI.Controls;
using SkyUI.Icons;

namespace SkyUI.UnitTests;

public class SkyFabTests
{
    [Fact]
    public void Default_icon_is_add_and_classes_applied()
    {
        var fab = new SkyFab();
        Assert.Equal(SkyIconKind.Add, fab.Icon);
        Assert.True(fab.Classes.Contains("sky"));
        Assert.True(fab.Classes.Contains("sky-fab"));
        Assert.True(fab.HonorSafeArea);
    }

    [Fact]
    public void IsExtended_and_label_bind_for_mvvm()
    {
        var fab = new SkyFab
        {
            IsExtended = true,
            Label = "Compose",
        };

        Assert.True(fab.IsExtended);
        Assert.Equal("Compose", fab.Label);
    }
}
