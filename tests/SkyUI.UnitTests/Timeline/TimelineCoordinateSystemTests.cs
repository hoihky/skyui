using SkyUI.Controls;

namespace SkyUI.UnitTests.Timeline;

public class TimelineCoordinateSystemTests
{
    [Theory]
    [InlineData(-1, 0)]
    [InlineData(5, 5)]
    [InlineData(120, 90)]
    public void ClampPlayhead_respects_duration(double input, double expected)
    {
        Assert.Equal(expected, TimelineCoordinateSystem.ClampPlayhead(input, 90));
    }

    [Fact]
    public void ClampPixelsPerSecond_clamps_to_bounds()
    {
        Assert.Equal(6, TimelineCoordinateSystem.ClampPixelsPerSecond(1));
        Assert.Equal(640, TimelineCoordinateSystem.ClampPixelsPerSecond(900));
        Assert.Equal(48, TimelineCoordinateSystem.ClampPixelsPerSecond(48));
    }

    [Fact]
    public void ContentWidth_uses_minimum_width()
    {
        Assert.Equal(400, TimelineCoordinateSystem.ContentWidth(1, 10));
        Assert.Equal(480, TimelineCoordinateSystem.ContentWidth(10, 48));
    }

    [Fact]
    public void FormatTimeLabel_formats_minutes_and_hours()
    {
        Assert.Equal("1:05", TimelineCoordinateSystem.FormatTimeLabel(65));
        Assert.Equal("1:01:05", TimelineCoordinateSystem.FormatTimeLabel(3665));
    }

    [Fact]
    public void NiceTickStep_returns_positive_step()
    {
        var step = TimelineCoordinateSystem.NiceTickStep(120, 800);
        Assert.True(step > 0);
    }

    [Fact]
    public void ClampClipStart_keeps_clip_inside_timeline()
    {
        Assert.Equal(10, TimelineCoordinateSystem.ClampClipStart(10, 5, 20));
        Assert.Equal(15, TimelineCoordinateSystem.ClampClipStart(99, 5, 20));
    }
}
