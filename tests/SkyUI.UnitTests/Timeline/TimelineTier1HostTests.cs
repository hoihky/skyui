using SkyUI.Controls;
using SkyUI.Controls.Timeline.Model;

namespace SkyUI.UnitTests.Timeline;

public class TimelineTier1HostTests
{
    [Fact]
    public void GetClipFrameSpan_exposes_frame_wrappers()
    {
        var timeline = new VideoTimeline { Fps = 24, TimeUnit = TimelineTimeUnit.Frames, Duration = 10 };
        timeline.AddSpriteTrack("Layer");
        var clip = new TimelineClipItem
        {
            TrackId = timeline.Tracks[0].Id,
            StartTime = 10.0 / 24,
            Duration = 5.0 / 24,
        };
        timeline.Clips.Add(clip);
        var span = timeline.GetClipFrameSpan(clip);
        Assert.Equal(10, span.StartFrame);
        Assert.Equal(5, span.DurationFrames);
    }

    [Fact]
    public void CurrentFrameChanged_fires_only_when_frame_index_changes()
    {
        var timeline = new VideoTimeline { Fps = 24, TimeUnit = TimelineTimeUnit.Frames, Duration = 10 };
        var frames = new List<int>();
        timeline.CurrentFrameChanged += (_, e) => frames.Add(e.Frame);
        timeline.StepPlayheadFrames(1);
        timeline.StepPlayheadFrames(1);
        Assert.Equal(new[] { 1, 2 }, frames);
    }

    [Fact]
    public void SplitClipAtPlayhead_copies_sprite_metadata()
    {
        var timeline = new VideoTimeline { Duration = 10, Fps = 24, TimeUnit = TimelineTimeUnit.Frames };
        timeline.AddSpriteTrack("L");
        var clip = new TimelineClipItem
        {
            TrackId = timeline.Tracks[0].Id,
            StartTime = 0,
            Duration = 2,
            Sprite = new TimelineSpriteClipMetadata { SpriteName = "cel", FrameIndex = 9 },
        };
        timeline.Clips.Add(clip);
        timeline.Host.Selection.SelectSingleClip(clip.Id);
        timeline.PlayheadTime = 1;
        var right = timeline.SplitClipAtPlayhead();
        Assert.NotNull(right);
        Assert.Equal("cel", right!.Sprite?.SpriteName);
        Assert.Equal(9, right.Sprite?.FrameIndex);
    }

    [Fact]
    public void CopyPaste_works_without_template_via_offline_clipboard()
    {
        var timeline = new VideoTimeline { Duration = 20, Fps = 24 };
        timeline.AddSpriteTrack("L");
        var clip = new TimelineClipItem
        {
            TrackId = timeline.Tracks[0].Id,
            StartTime = 0,
            Duration = 1,
            Label = "x",
        };
        timeline.Clips.Add(clip);
        timeline.Host.Selection.SelectSingleClip(clip.Id);
        timeline.CopySelectionToClipboard();
        timeline.PasteClipboardAtPlayhead();
        Assert.Equal(2, timeline.Clips.Count);
    }

    [Fact]
    public void ClonePrototype_clones_sprite_metadata()
    {
        var source = new TimelineClipItem
        {
            TrackId = "t",
            StartTime = 0,
            Duration = 1,
            Sprite = new TimelineSpriteClipMetadata { SpriteName = "x", FrameIndex = 2 },
        };
        var clone = SkyUI.Controls.Timeline.Editing.TimelineClipOperations.ClonePrototype(source);
        Assert.Equal("x", clone.Sprite?.SpriteName);
        Assert.Equal(2, clone.Sprite?.FrameIndex);
        Assert.NotSame(source.Sprite, clone.Sprite);
    }
}
