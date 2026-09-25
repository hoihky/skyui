using SkyUI.Controls;

namespace SkyUI.HeadlessTests;

public class VideoTimelineHeadlessTests
{
    [Fact]
    public void VideoTimeline_instantiates_with_defaults()
    {
        var timeline = new VideoTimeline();
        Assert.Equal(120, timeline.Duration);
        Assert.Equal(40, timeline.PixelsPerSecond);
        Assert.Equal(0, timeline.PlayheadTime);
        Assert.False(timeline.IsPlaying);
        Assert.False(timeline.CanUndo);
        Assert.False(timeline.CanRedo);
    }

    [Fact]
    public void AddMarker_appends_to_markers_collection()
    {
        var timeline = new VideoTimeline();
        var marker = timeline.AddMarker(12.5, "Beat");
        Assert.Same(marker, timeline.Markers[^1]);
        Assert.Equal(12.5, marker.Time);
    }

    [Fact]
    public void Seek_updates_playhead()
    {
        var timeline = new VideoTimeline { Duration = 60 };
        timeline.Seek(15);
        Assert.Equal(15, timeline.PlayheadTime);
    }

    [Fact]
    public void Play_and_Stop_toggle_is_playing()
    {
        var timeline = new VideoTimeline();
        timeline.Play();
        Assert.True(timeline.IsPlaying);
        timeline.Stop();
        Assert.False(timeline.IsPlaying);
    }

    [Fact]
    public void GetSelectedClips_empty_when_nothing_selected()
    {
        var timeline = new VideoTimeline();
        timeline.AddTrack();
        timeline.Clips.Add(new TimelineClipItem
        {
            TrackId = timeline.Tracks[0].Id,
            StartTime = 0,
            Duration = 4,
        });
        Assert.Empty(timeline.GetSelectedClips());
    }

    [Fact]
    public void RemoveSelectedTrack_noop_when_clips_selected()
    {
        var timeline = new VideoTimeline();
        timeline.AddTrack("A");
        var clip = new TimelineClipItem { TrackId = timeline.Tracks[0].Id, StartTime = 0, Duration = 2 };
        timeline.Clips.Add(clip);
        timeline.Host.Selection.SelectSingleClip(clip.Id);
        timeline.Host.Selection.SelectedTrackId = timeline.Tracks[0].Id;

        timeline.RemoveSelectedTrack();
        Assert.Single(timeline.Tracks);
    }

    [Fact]
    public void PasteClipboardAtPlayhead_does_nothing_when_clipboard_empty()
    {
        var timeline = new VideoTimeline { PlayheadTime = 3 };
        timeline.AddTrack();
        timeline.PasteClipboardAtPlayhead();
        Assert.Empty(timeline.Clips);
    }

    [Fact]
    public void RemoveSelectedTrack_removes_track_when_only_track_is_selected()
    {
        var timeline = new VideoTimeline();
        timeline.AddTrack("Only");
        timeline.Host.Selection.SelectedTrackId = timeline.Tracks[0].Id;

        timeline.RemoveSelectedTrack();

        Assert.Empty(timeline.Tracks);
    }

    [Fact]
    public void ClipsRemoved_fires_when_cutting_selection()
    {
        var timeline = new VideoTimeline();
        timeline.AddTrack("A");
        var clip = new TimelineClipItem { TrackId = timeline.Tracks[0].Id, StartTime = 0, Duration = 2 };
        timeline.Clips.Add(clip);
        timeline.Host.Selection.SelectSingleClip(clip.Id);
        IReadOnlyList<TimelineClipItem>? removed = null;
        timeline.ClipsRemoved += (_, e) => removed = e.Clips;

        timeline.CutSelectionToClipboard();

        Assert.NotNull(removed);
        Assert.Single(removed!);
        Assert.Equal(clip.Id, removed![0].Id);
    }

    [Fact]
    public void HasTimeRangeSelection_false_before_template_applied()
    {
        var timeline = new VideoTimeline();
        Assert.False(timeline.HasTimeRangeSelection);
    }
}
