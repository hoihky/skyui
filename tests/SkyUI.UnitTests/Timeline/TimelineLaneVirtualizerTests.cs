using SkyUI.Controls.Timeline.Rendering;

namespace SkyUI.UnitTests.Timeline;

public class TimelineLaneVirtualizerTests
{
    [Fact]
    public void GetVisibleRowRange_returns_empty_when_no_tracks()
    {
        var (first, last) = TimelineLaneVirtualizer.GetVisibleRowRange(0, 200, 0, 44);
        Assert.Equal(0, first);
        Assert.Equal(-1, last);
    }

    [Fact]
    public void GetVisibleRowRange_windows_viewport_with_overscan()
    {
        var (first, last) = TimelineLaneVirtualizer.GetVisibleRowRange(88, 132, 10, 44);
        Assert.Equal(1, first);
        Assert.Equal(6, last);
    }

    [Fact]
    public void GetVisibleRowRange_returns_all_rows_when_viewport_unmeasured()
    {
        var (first, last) = TimelineLaneVirtualizer.GetVisibleRowRange(0, 0, 5, 44);
        Assert.Equal(0, first);
        Assert.Equal(4, last);
    }

    [Fact]
    public void MeasureContentHeight_fills_viewport_when_few_tracks()
    {
        var height = TimelineLaneVirtualizer.MeasureContentHeight(2, 44, 300);
        Assert.Equal(300, height);
    }

    [Fact]
    public void MeasureContentHeight_grows_with_track_count()
    {
        var height = TimelineLaneVirtualizer.MeasureContentHeight(8, 44, 200);
        Assert.Equal(8 * 44, height);
    }
}
