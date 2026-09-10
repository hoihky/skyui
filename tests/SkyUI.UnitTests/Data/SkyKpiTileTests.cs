using Avalonia.Controls;
using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class SkyKpiTileTests
{
    [Fact]
    public void Has_sky_classes()
    {
        var tile = new SkyKpiTile();
        Assert.Contains("sky", tile.Classes);
        Assert.Contains("sky-kpi-tile", tile.Classes);
    }

    [Fact]
    public void Label_value_and_delta_bind()
    {
        var tile = new SkyKpiTile
        {
            Label = "Revenue",
            Value = "$42",
            Delta = "+8%"
        };

        Assert.Equal("Revenue", tile.Label);
        Assert.Equal("$42", tile.Value);
        Assert.Equal("+8%", tile.Delta);
    }

    [Fact]
    public void DeltaTrend_sets_visual_classes()
    {
        var tile = new SkyKpiTile { DeltaTrend = SkyKpiDeltaTrend.Positive };
        Assert.Contains("sky-kpi-tile-delta-positive", tile.Classes);

        tile.DeltaTrend = SkyKpiDeltaTrend.Negative;
        Assert.Contains("sky-kpi-tile-delta-negative", tile.Classes);
        Assert.DoesNotContain("sky-kpi-tile-delta-positive", tile.Classes);
    }

    [Fact]
    public void SparklineContent_accepts_custom_content()
    {
        var sparkline = new Border();
        var tile = new SkyKpiTile { SparklineContent = sparkline };
        Assert.Same(sparkline, tile.SparklineContent);
    }
}
