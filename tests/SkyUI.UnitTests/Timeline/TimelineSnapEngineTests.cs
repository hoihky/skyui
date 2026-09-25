using System.Collections.ObjectModel;
using SkyUI.Controls;
using SkyUI.Controls.Timeline.Layout;

namespace SkyUI.UnitTests.Timeline;

public class TimelineSnapEngineTests
{
    [Fact]
    public void SnapTime_snaps_to_playhead_when_within_threshold()
    {
        var engine = new TimelineSnapEngine
        {
            Settings =
            {
                IsEnabled = true,
                SnapToPlayhead = true,
                SnapToMarkers = false,
                SnapToClipEdges = false,
                SnapToGrid = false,
                SnapThresholdSeconds = 0.2,
            },
        };

        var snapped = engine.SnapTime(4.95, playheadTimeSeconds: 5, markers: [], clips: []);
        Assert.Equal(5, snapped);
    }

    [Fact]
    public void SnapTime_returns_proposed_when_disabled()
    {
        var engine = new TimelineSnapEngine { Settings = { IsEnabled = false } };
        var snapped = engine.SnapTime(4.95, 5, [], []);
        Assert.Equal(4.95, snapped);
    }

    [Fact]
    public void SnapTime_uses_custom_provider_targets()
    {
        var engine = new TimelineSnapEngine
        {
            Settings =
            {
                IsEnabled = true,
                SnapToPlayhead = false,
                SnapToMarkers = false,
                SnapToClipEdges = false,
                SnapToGrid = false,
                SnapThresholdSeconds = 0.5,
            },
        };
        engine.RegisterProvider(new FixedSnapProvider(7.5));

        var snapped = engine.SnapTime(7.2, 0, [], []);
        Assert.Equal(7.5, snapped);
    }

    [Fact]
    public void SnapClipStart_snaps_to_neighboring_clip_edge()
    {
        var clips = new ObservableCollection<TimelineClipItem>
        {
            new() { StartTime = 0, Duration = 5 },
            new() { StartTime = 10, Duration = 5 },
        };
        var engine = new TimelineSnapEngine
        {
            Settings =
            {
                IsEnabled = true,
                SnapToPlayhead = false,
                SnapToMarkers = false,
                SnapToClipEdges = true,
                SnapToGrid = false,
                SnapThresholdSeconds = 0.25,
            },
        };

        var snapped = engine.SnapClipStart(9.85, 2, 0, [], clips);
        Assert.Equal(10, snapped);
    }

    [Fact]
    public void SnapTime_snaps_to_marker_when_within_threshold()
    {
        var markers = new ObservableCollection<TimelineMarkerItem>
        {
            new() { Time = 8 },
        };
        var engine = new TimelineSnapEngine
        {
            Settings =
            {
                IsEnabled = true,
                SnapToPlayhead = false,
                SnapToMarkers = true,
                SnapToClipEdges = false,
                SnapToGrid = false,
                SnapThresholdSeconds = 0.3,
            },
        };

        var snapped = engine.SnapTime(7.75, 0, markers, []);
        Assert.Equal(8, snapped);
    }

    [Fact]
    public void SnapTime_snaps_to_grid_interval()
    {
        var engine = new TimelineSnapEngine
        {
            Settings =
            {
                IsEnabled = true,
                SnapToPlayhead = false,
                SnapToMarkers = false,
                SnapToClipEdges = false,
                SnapToGrid = true,
                GridIntervalSeconds = 5,
                SnapThresholdSeconds = 0.5,
            },
        };

        var snapped = engine.SnapTime(9.7, playheadTimeSeconds: 10, markers: [], clips: []);
        Assert.Equal(10, snapped);
    }

    [Fact]
    public void SnapClipStart_uses_end_snap_when_only_end_is_near_target()
    {
        var markers = new ObservableCollection<TimelineMarkerItem>
        {
            new() { Time = 12 },
        };
        var engine = new TimelineSnapEngine
        {
            Settings =
            {
                IsEnabled = true,
                SnapToPlayhead = false,
                SnapToMarkers = true,
                SnapToClipEdges = false,
                SnapToGrid = false,
                SnapThresholdSeconds = 0.25,
            },
        };

        var snapped = engine.SnapClipStart(7.9, clipDurationSeconds: 4, playheadTimeSeconds: 0, markers, []);
        Assert.Equal(8, snapped);
    }

    [Fact]
    public void RegisterProvider_throws_when_null()
    {
        var engine = new TimelineSnapEngine();
        Assert.Throws<ArgumentNullException>(() => engine.RegisterProvider(null!));
    }

    [Fact]
    public void UnregisterProvider_removes_custom_targets()
    {
        var engine = new TimelineSnapEngine
        {
            Settings =
            {
                IsEnabled = true,
                SnapToPlayhead = false,
                SnapToMarkers = false,
                SnapToClipEdges = false,
                SnapToGrid = false,
                SnapThresholdSeconds = 0.5,
            },
        };
        var provider = new FixedSnapProvider(6);
        engine.RegisterProvider(provider);
        Assert.Equal(6, engine.SnapTime(5.8, 0, [], []));

        engine.UnregisterProvider(provider);
        Assert.Equal(5.8, engine.SnapTime(5.8, 0, [], []));
    }

    private sealed class FixedSnapProvider(double time) : ITimelineSnapTargetProvider
    {
        public IEnumerable<double> GetSnapTimes()
        {
            yield return time;
        }
    }
}
