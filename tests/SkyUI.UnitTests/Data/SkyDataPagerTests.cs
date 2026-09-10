using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class SkyDataPagerTests
{
    [Fact]
    public void Has_sky_classes()
    {
        var pager = new SkyDataPager();
        Assert.Contains("sky", pager.Classes);
        Assert.Contains("sky-data-pager", pager.Classes);
    }

    [Fact]
    public void SummaryText_reflects_current_range()
    {
        var pager = new SkyDataPager
        {
            ItemsSource = Enumerable.Range(1, 47).ToArray(),
            PageSize = 10,
            CurrentPage = 3,
            SummaryFormat = "Showing {0}-{1} of {2}"
        };

        Assert.Equal("Showing 21-30 of 47", pager.SummaryText);
    }

    [Fact]
    public void PageSizes_defaults_to_common_options()
    {
        var pager = new SkyDataPager();
        Assert.Equal([10, 25, 50, 100], pager.PageSizes.Cast<int>().ToArray());
    }

    [Fact]
    public void Manual_total_count_is_respected_when_automatic_is_disabled()
    {
        var pager = new SkyDataPager
        {
            ItemsSource = Enumerable.Range(1, 5).ToArray(),
            IsTotalCountAutomatic = false,
            TotalCount = 100,
            PageSize = 10
        };

        Assert.Equal(100, pager.TotalCount);
        Assert.Equal(10, pager.PageCount);
    }
}
