using SkyUI.Controls;
using SkyUI.Controls.Timeline.Model;
using SkyUI.Controls.Timeline.Navigation;
using SkyUI.Controls.Timeline.Thumbnails;
using SkyUI.Controls.Timeline.Time;

namespace SkyUI.UnitTests.Timeline;

public sealed class TimelineTier2Tests
{
    [Fact]
    public void SpriteMetadataFingerprint_changes_when_sprite_name_changes()
    {
        var fingerprint = new TimelineSpriteMetadataFingerprint();
        var clip = new TimelineClipItem
        {
            Sprite = new TimelineSpriteClipMetadata { SpriteName = "a", FrameIndex = 0 },
        };
        var a = fingerprint.Compute(clip);
        clip.Sprite.SpriteName = "b";
        var b = fingerprint.Compute(clip);
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void ClipThumbnailCache_evicts_by_clip_id()
    {
        var cache = new TimelineClipThumbnailCache();
        var key = new TimelineClipThumbnailCacheKey("c1", "fp1");
        cache.Store(key, new Avalonia.Media.DrawingImage());
        Assert.True(cache.TryGet(key, out _));
        cache.RemoveClip("c1");
        Assert.False(cache.TryGet(key, out _));
    }

    [Fact]
    public void ViewportZoomPlanner_clamps_pixels_per_second()
    {
        var planner = new TimelineViewportZoomPlanner();
        var pps = planner.ComputePixelsPerSecond(120, 800, 6, 400);
        Assert.InRange(pps, 6, 400);
        pps = planner.ComputePixelsPerSecond(0.001, 800, 6, 400);
        Assert.Equal(400, pps);
        pps = planner.ComputePixelsPerSecond(5000, 800, 6, 400);
        Assert.Equal(6, pps);
    }

    [Fact]
    public void PreferTimecodeLabels_switches_ruler_formatter_in_frame_mode()
    {
        var presentation = new TimelineTimePresentation
        {
            TimeUnit = TimelineTimeUnit.Frames,
            FramesPerSecond = 24,
            PreferTimecodeLabels = false,
        };
        Assert.IsType<FrameRulerLabelFormatter>(presentation.RulerLabels);
        presentation.PreferTimecodeLabels = true;
        Assert.IsType<TimecodeRulerLabelFormatter>(presentation.RulerLabels);
    }

    [Fact]
    public void ThumbnailReadyEventArgs_carries_metadata_fingerprint()
    {
        var args = new TimelineClipThumbnailReadyEventArgs("clip-1", "fp:v1", null);
        Assert.Equal("fp:v1", args.MetadataFingerprint);
        Assert.Equal("clip-1", args.ClipId);
    }

    [Fact]
    public void VideoTimeline_exposes_zoom_and_timecode_api()
    {
        var timeline = new VideoTimeline { Duration = 10, Fps = 24, TimeUnit = TimelineTimeUnit.Frames };
        timeline.PreferTimecodeLabels = true;
        Assert.True(timeline.PreferTimecodeLabels);
        timeline.ZoomToFit();
        timeline.ZoomToSelection();
    }
}
