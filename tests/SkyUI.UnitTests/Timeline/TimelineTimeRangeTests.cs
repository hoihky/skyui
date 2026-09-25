using SkyUI.Controls;

namespace SkyUI.UnitTests.Timeline;

public class TimelineTimeRangeTests
{
    [Fact]
    public void Min_Max_normalize_inverted_range()
    {
        var range = new TimelineTimeRange(8, 2);
        Assert.Equal(2, range.Min);
        Assert.Equal(8, range.Max);
        Assert.Equal(6, range.Length);
    }

    [Fact]
    public void Length_is_zero_when_points_coincide()
    {
        var range = new TimelineTimeRange(4, 4);
        Assert.Equal(0, range.Length);
    }
}
