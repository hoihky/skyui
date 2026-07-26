using Avalonia;
using Avalonia.Controls;
using SkyUI.Core.Theming;

namespace SkyUI.UnitTests;

public class SkyDensityApplicatorTests
{
    [Fact]
    public void Compact_sets_button_min_height_override()
    {
        var app = new Application();
        SkyDensityApplicator.Apply(app, SkyDensity.Compact);

        Assert.Equal(32.0, app.Resources[SkyDensityKeys.ButtonMinHeight]);
    }

    [Fact]
    public void Comfortable_clears_compact_overrides()
    {
        var app = new Application();
        SkyDensityApplicator.Apply(app, SkyDensity.Compact);
        SkyDensityApplicator.Apply(app, SkyDensity.Comfortable);

        Assert.False(app.Resources.ContainsKey(SkyDensityKeys.ButtonMinHeight));
    }
}
