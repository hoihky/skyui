using Avalonia.Controls;
using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class LayoutControlsTests
{
    [Fact]
    public void Layout_controls_instantiate()
    {
        Assert.NotNull(new SkyCard());
        Assert.NotNull(new SkyDivider());
        Assert.NotNull(new SkyExpander());
        Assert.NotNull(new SkyResponsiveGrid());
    }

    [Fact]
    public void SkyCard_has_sky_classes()
    {
        var card = new SkyCard();
        Assert.Contains("sky", card.Classes);
        Assert.Contains("sky-card", card.Classes);
    }

    [Fact]
    public void SkyDivider_defaults_to_horizontal_orientation()
    {
        var divider = new SkyDivider();
        Assert.Equal(Avalonia.Layout.Orientation.Horizontal, divider.Orientation);
    }

    [Fact]
    public void SkyDivider_vertical_orientation_updates_property()
    {
        var divider = new SkyDivider { Orientation = Avalonia.Layout.Orientation.Vertical };
        Assert.Equal(Avalonia.Layout.Orientation.Vertical, divider.Orientation);
    }

    [Fact]
    public void SkyExpander_has_sky_class() =>
        Assert.Contains("sky", new SkyExpander().Classes);

    [Fact]
    public void SkyResponsiveGrid_has_spacing_defaults()
    {
        var grid = new SkyResponsiveGrid();
        Assert.Equal(16, grid.ColumnSpacing);
        Assert.Equal(16, grid.RowSpacing);
    }

    [Fact]
    public void SkyGridLayout_places_children_in_grid()
    {
        var grid = new Grid { Width = 1100, Height = 200 };
        SkyGridLayout.SetIsResponsive(grid, true);

        for (var i = 0; i < 6; i++)
            grid.Children.Add(new Border());

        SkyGridLayout.ApplyLayout(grid, 1100);

        Assert.Equal(5, grid.ColumnDefinitions.Count);
        Assert.True(grid.RowDefinitions.Count >= 1);
        Assert.Equal(0, Grid.GetRow(grid.Children[0]));
        Assert.Equal(0, Grid.GetColumn(grid.Children[0]));
        Assert.Equal(1, Grid.GetColumn(grid.Children[1]));
    }
}
