using SkyUI.Controls;
using SkyUI.Controls.Timeline.Layout;

namespace SkyUI.UnitTests.Timeline;

public class TimelineLayoutEngineTests
{
    [Fact]
    public void TimeToPixel_and_PixelToTime_are_inverse()
    {
        var layout = new TimelineLayoutEngine { PixelsPerSecond = 50 };
        Assert.Equal(100, layout.TimeToPixel(2));
        Assert.Equal(2, layout.PixelToTime(100));
    }

    [Fact]
    public void PointerTimeToSeconds_includes_scroll_offset()
    {
        var layout = new TimelineLayoutEngine { PixelsPerSecond = 40 };
        Assert.Equal(5, layout.PointerTimeToSeconds(0, 200));
    }

    [Fact]
    public void ClampPlayhead_respects_duration()
    {
        var layout = new TimelineLayoutEngine { Duration = 30 };
        Assert.Equal(30, layout.ClampPlayhead(99));
        Assert.Equal(0, layout.ClampPlayhead(-1));
    }

    [Fact]
    public void SnapTime_uses_registered_provider()
    {
        var layout = new TimelineLayoutEngine
        {
            Duration = 60,
            PixelsPerSecond = 40,
        };
        layout.SnapSettings.IsEnabled = true;
        layout.SnapSettings.SnapToPlayhead = false;
        layout.SnapSettings.SnapToMarkers = false;
        layout.SnapSettings.SnapToClipEdges = false;
        layout.SnapSettings.SnapToGrid = false;
        layout.SnapSettings.SnapThresholdSeconds = 0.5;
        layout.RegisterSnapTargetProvider(new FixedSnapProvider(12));

        var snapped = layout.SnapTime(11.8, 0, [], []);
        Assert.Equal(12, snapped);
    }

    [Fact]
    public void ContentWidth_honors_duration_and_zoom()
    {
        var layout = new TimelineLayoutEngine { Duration = 10, PixelsPerSecond = 48 };
        Assert.Equal(480, layout.ContentWidth(minimumWidth: 100));
    }

    [Fact]
    public void ClampClipStart_delegates_to_coordinate_system()
    {
        var layout = new TimelineLayoutEngine { Duration = 20 };
        Assert.Equal(15, layout.ClampClipStart(99, 5));
    }

    [Fact]
    public void UnregisterSnapTargetProvider_stops_snapping()
    {
        var layout = new TimelineLayoutEngine
        {
            Duration = 60,
            PixelsPerSecond = 40,
        };
        layout.SnapSettings.IsEnabled = true;
        layout.SnapSettings.SnapToPlayhead = false;
        layout.SnapSettings.SnapToMarkers = false;
        layout.SnapSettings.SnapToClipEdges = false;
        layout.SnapSettings.SnapToGrid = false;
        layout.SnapSettings.SnapThresholdSeconds = 0.5;
        var provider = new FixedSnapProvider(9);
        layout.RegisterSnapTargetProvider(provider);
        layout.UnregisterSnapTargetProvider(provider);

        Assert.Equal(8.6, layout.SnapTime(8.6, 0, [], []));
    }

    [Fact]
    public void SnapClipStart_delegates_to_snap_engine()
    {
        var layout = new TimelineLayoutEngine();
        layout.SnapSettings.IsEnabled = true;
        layout.SnapSettings.SnapToPlayhead = true;
        layout.SnapSettings.SnapToMarkers = false;
        layout.SnapSettings.SnapToClipEdges = false;
        layout.SnapSettings.SnapToGrid = false;
        layout.SnapSettings.SnapThresholdSeconds = 0.2;

        var snapped = layout.SnapClipStart(2.9, 2, playheadTimeSeconds: 5, markers: [], clips: []);
        Assert.Equal(3, snapped);
    }

    private sealed class FixedSnapProvider(double time) : ITimelineSnapTargetProvider
    {
        public IEnumerable<double> GetSnapTimes()
        {
            yield return time;
        }
    }
}
