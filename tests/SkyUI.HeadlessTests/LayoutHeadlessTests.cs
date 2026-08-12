using Avalonia.Controls;
using SkyUI.Controls;

namespace SkyUI.HeadlessTests;

public class LayoutHeadlessTests
{
    [Fact]
    public void SkyResponsiveGrid_resolves_columns_for_width()
    {
        var grid = new SkyResponsiveGrid { Width = 800 };
        grid.Measure(new Avalonia.Size(800, 400));
        grid.Arrange(new Avalonia.Rect(0, 0, 800, 400));

        Assert.Equal(3, grid.Columns);
    }

    [Fact]
    public void SkyCard_accepts_header_and_content()
    {
        var card = new SkyCard
        {
            Header = "Title",
            Content = new TextBlock { Text = "Body" },
        };

        Assert.Equal("Title", card.Header);
        Assert.IsType<TextBlock>(card.Content);
    }
}
