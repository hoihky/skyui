using SkyUI.Controls;
using SkyUI.Controls.Timeline.Integration;
using SkyUI.Controls.Timeline.Model;

namespace SkyUI.UnitTests.Timeline;

public class TimelineTier4Tests
{
    [Fact]
    public void MediaClock_reflects_timeline_transport_state()
    {
        var timeline = new VideoTimeline { Fps = 24, TimeUnit = TimelineTimeUnit.Frames };
        timeline.PlayheadTime = 2.0 / 24;
        var clock = timeline.MediaClock;
        Assert.Equal(24, clock.FramesPerSecond);
        Assert.Equal(2, clock.CurrentFrame);
        Assert.False(clock.IsPlaying);
        Assert.Equal(TimeSpan.FromSeconds(1.0 / 24), clock.FrameDuration);
    }

    [Fact]
    public void PreviewSynchronizer_builds_sprite_layer_snapshot()
    {
        var timeline = new VideoTimeline { Fps = 24, Duration = 10 };
        timeline.AddSpriteTrack("L");
        timeline.Clips.Add(new TimelineClipItem
        {
            TrackId = timeline.Tracks[0].Id,
            StartTime = 0,
            Duration = 1,
            Sprite = new TimelineSpriteClipMetadata { SpriteName = "cel" },
        });
        var snapshot = timeline.PreviewSync.CreateSnapshot(0, 0);
        Assert.Single(snapshot.SpriteLayers);
        Assert.Equal("cel", snapshot.SpriteLayers[0].Sprite?.SpriteName);
    }

    [Fact]
    public async Task Null_thumbnail_provider_returns_null_image()
    {
        var provider = NullTimelineClipThumbnailProvider.Instance;
        var image = await provider.GetThumbnailAsync(new TimelineClipItem());
        Assert.Null(image);
    }
}
