using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class SkyPaginationMathTests
{
    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(1, 10, 1)]
    [InlineData(10, 10, 1)]
    [InlineData(11, 10, 2)]
    [InlineData(25, 10, 3)]
    public void ComputePageCount_returns_expected_values(int totalCount, int pageSize, int expected)
    {
        Assert.Equal(expected, SkyPaginationMath.ComputePageCount(totalCount, pageSize));
    }

    [Theory]
    [InlineData(0, 0, 1)]
    [InlineData(3, 5, 3)]
    [InlineData(7, 5, 5)]
    [InlineData(-2, 5, 1)]
    public void ClampPage_clamps_to_valid_range(int page, int pageCount, int expected)
    {
        Assert.Equal(expected, SkyPaginationMath.ClampPage(page, pageCount));
    }

    [Theory]
    [InlineData(1, 10, 25, 1, 10)]
    [InlineData(3, 10, 25, 21, 25)]
    [InlineData(2, 10, 0, 0, 0)]
    public void GetInclusiveRange_returns_expected_bounds(int page, int pageSize, int total, int start, int end)
    {
        var range = SkyPaginationMath.GetInclusiveRange(page, pageSize, total);
        Assert.Equal(start, range.Start);
        Assert.Equal(end, range.End);
    }

    [Fact]
    public void Slice_returns_requested_page()
    {
        var source = Enumerable.Range(1, 25).ToArray();
        var slice = SkyPaginationMath.Slice(source, 3, 10).Cast<int>().ToArray();

        Assert.Equal([21, 22, 23, 24, 25], slice);
    }

    [Fact]
    public void CountItems_supports_collections_and_enumerables()
    {
        Assert.Equal(3, SkyPaginationMath.CountItems(new[] { 1, 2, 3 }));
        Assert.Equal(2, SkyPaginationMath.CountItems(EnumerateTwoItems()));
        Assert.Equal(0, SkyPaginationMath.CountItems(null));
    }

    private static IEnumerable<int> EnumerateTwoItems()
    {
        yield return 1;
        yield return 2;
    }
}
