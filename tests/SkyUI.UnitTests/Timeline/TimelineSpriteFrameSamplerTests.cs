using SkyUI.Controls;
using SkyUI.Controls.Timeline.Composition;
using SkyUI.Controls.Timeline.Frame;
using SkyUI.Controls.Timeline.Model;
using SkyUI.Controls.Timeline.Time;

namespace SkyUI.UnitTests.Timeline;

public class TimelineSpriteFrameSamplerTests
{
    [Fact]
    public void SampleAtFrame_returns_active_cel_per_sprite_layer()
    {
        var project = new TimelineProject { Fps = 24, TimeUnit = TimelineTimeUnit.Frames };
        var track = new TimelineTrackItem { Name = "Layer", Kind = TimelineTrackKind.Sprite };
        project.Tracks.Add(track);
        project.Clips.Add(new TimelineClipItem
        {
            TrackId = track.Id,
            StartTime = 0,
            Duration = 1,
            Sprite = new TimelineSpriteClipMetadata { SpriteName = "a" },
        });
        project.Clips.Add(new TimelineClipItem
        {
            TrackId = track.Id,
            StartTime = 1,
            Duration = 1,
            Sprite = new TimelineSpriteClipMetadata { SpriteName = "b" },
        });

        var presentation = new TimelineTimePresentation { FramesPerSecond = 24, TimeUnit = TimelineTimeUnit.Frames };
        var mapper = new TimelineClipFrameMapper(presentation);
        var sampler = new TimelineSpriteFrameSampler(new TimelineTrackCatalog(project), mapper);

        var at0 = sampler.SampleAtFrame(12, project.Clips);
        Assert.Single(at0);
        Assert.Equal("a", at0[0].Sprite?.SpriteName);

        var at24 = sampler.SampleAtFrame(24, project.Clips);
        Assert.Equal("b", at24[0].Sprite?.SpriteName);
    }

    [Fact]
    public void SampleAtFrame_skips_hidden_sprite_layers()
    {
        var project = new TimelineProject();
        var track = new TimelineTrackItem { Kind = TimelineTrackKind.Sprite, IsVisible = false };
        project.Tracks.Add(track);
        project.Clips.Add(new TimelineClipItem
        {
            TrackId = track.Id,
            StartTime = 0,
            Duration = 2,
            Sprite = new TimelineSpriteClipMetadata { SpriteName = "hidden" },
        });

        var presentation = new TimelineTimePresentation { FramesPerSecond = 24 };
        var sampler = new TimelineSpriteFrameSampler(
            new TimelineTrackCatalog(project),
            new TimelineClipFrameMapper(presentation));
        var sample = sampler.SampleAtFrame(0, project.Clips);
        Assert.Null(sample[0].Clip);
    }
}
