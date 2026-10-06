using SkyUI.Controls;
using SkyUI.Controls.Timeline.Commands;
using SkyUI.Controls.Timeline.Model;

namespace SkyUI.UnitTests.Timeline;

public class VideoTimelineHostTests
{
    [Fact]
    public void PlayheadTime_is_clamped_to_duration()
    {
        var timeline = new VideoTimeline { Duration = 20, TimeUnit = TimelineTimeUnit.Seconds };
        timeline.PlayheadTime = 100;
        Assert.Equal(20, timeline.PlayheadTime);
    }

    [Fact]
    public void StepPlayheadFrames_advances_in_frame_mode()
    {
        var timeline = new VideoTimeline { Duration = 10, Fps = 24, TimeUnit = TimelineTimeUnit.Frames };
        timeline.PlayheadTime = 0;
        timeline.StepPlayheadFrames(3);
        Assert.Equal(3, timeline.PlayheadFrame);
    }

    [Fact]
    public void PixelsPerSecond_is_clamped()
    {
        var timeline = new VideoTimeline();
        timeline.PixelsPerSecond = 1;
        Assert.Equal(6, timeline.PixelsPerSecond);
        timeline.PixelsPerSecond = 900;
        Assert.Equal(640, timeline.PixelsPerSecond);
    }

    [Fact]
    public void AddTrack_and_RemoveTrack_update_collections()
    {
        var timeline = new VideoTimeline();
        timeline.AddTrack("A");
        timeline.AddTrack("B");
        Assert.Equal(2, timeline.Tracks.Count);

        timeline.RemoveTrack(timeline.Tracks[0]);
        Assert.Single(timeline.Tracks);
    }

    [Fact]
    public void RemoveTrack_removes_clips_on_that_lane()
    {
        var timeline = new VideoTimeline();
        timeline.AddTrack("A");
        var trackId = timeline.Tracks[0].Id;
        timeline.Clips.Add(new TimelineClipItem { TrackId = trackId, StartTime = 0, Duration = 5 });
        timeline.RemoveTrack(timeline.Tracks[0]);
        Assert.Empty(timeline.Clips);
    }

    [Fact]
    public void SplitClipAtPlayhead_requires_selected_clip()
    {
        var timeline = new VideoTimeline { Duration = 30 };
        Assert.Null(timeline.SplitClipAtPlayhead());
    }

    [Fact]
    public void SplitClipAtPlayhead_creates_segment_and_undo_restores()
    {
        var timeline = new VideoTimeline { Duration = 30, PlayheadTime = 4 };
        timeline.AddTrack("V1");
        var clip = new TimelineClipItem
        {
            TrackId = timeline.Tracks[0].Id,
            StartTime = 0,
            Duration = 10,
            Label = "Intro",
        };
        timeline.Clips.Add(clip);
        timeline.Host.Selection.SelectSingleClip(clip.Id);

        var right = timeline.SplitClipAtPlayhead();
        Assert.NotNull(right);
        Assert.Equal(4, clip.Duration);
        Assert.Equal(4, right!.StartTime);
        Assert.Equal(6, right.Duration);
        Assert.True(timeline.CanUndo);

        timeline.Undo();
        Assert.Equal(10, clip.Duration);
        Assert.DoesNotContain(timeline.Clips, c => c.Id == right.Id);
    }

    [Fact]
    public void Undo_redo_after_move_command_via_host_stack()
    {
        var timeline = new VideoTimeline();
        timeline.AddTrack("A");
        var clip = new TimelineClipItem { TrackId = timeline.Tracks[0].Id, StartTime = 1, Duration = 3 };
        timeline.Clips.Add(clip);
        timeline.Host.Selection.SelectSingleClip(clip.Id);

        var before = TimelineMoveClipsCommand.Capture([clip]);
        clip.StartTime = 6;
        var after = TimelineMoveClipsCommand.Capture([clip]);
        timeline.Host.UndoStack.Execute(new TimelineMoveClipsCommand([clip], before, after));

        Assert.Equal(6, clip.StartTime);
        timeline.Undo();
        Assert.Equal(1, clip.StartTime);
        timeline.Redo();
        Assert.Equal(6, clip.StartTime);
    }

    [Fact]
    public void Project_exposes_same_collections_as_control()
    {
        var timeline = new VideoTimeline();
        timeline.AddTrack("Shared");
        Assert.Same(timeline.Tracks, timeline.Project.Tracks);
        Assert.Same(timeline.Clips, timeline.Project.Clips);
        Assert.Same(timeline.Markers, timeline.Project.Markers);
    }

    [Fact]
    public void SnapSettings_toggle_disables_snapping()
    {
        var timeline = new VideoTimeline();
        timeline.SnapSettings.IsEnabled = false;
        var layout = timeline.Host.Layout;
        var snapped = layout.SnapTime(4.95, 5, [], []);
        Assert.Equal(4.95, snapped);
    }

    [Fact]
    public void Duration_coerces_to_minimum()
    {
        var timeline = new VideoTimeline { Duration = 0 };
        Assert.True(timeline.Duration >= 0.01);
    }

    [Fact]
    public void RemoveSelectedTrack_removes_track_when_no_clips_selected()
    {
        var timeline = new VideoTimeline();
        timeline.AddTrack("Solo");
        timeline.Host.Selection.SelectedTrackId = timeline.Tracks[0].Id;

        timeline.RemoveSelectedTrack();

        Assert.Empty(timeline.Tracks);
        Assert.Null(timeline.SelectedTrackId);
    }

    [Fact]
    public void RemoveClip_removes_item_and_clears_selection()
    {
        var timeline = new VideoTimeline();
        timeline.AddTrack("A");
        var clip = new TimelineClipItem { TrackId = timeline.Tracks[0].Id, StartTime = 0, Duration = 3 };
        timeline.Clips.Add(clip);
        timeline.Host.Selection.SelectSingleClip(clip.Id);

        timeline.RemoveClip(clip);

        Assert.Empty(timeline.Clips);
        Assert.Empty(timeline.GetSelectedClips());
    }

    [Fact]
    public void CutSelection_deletes_selected_clips_without_template()
    {
        var timeline = new VideoTimeline();
        timeline.AddTrack("A");
        var clip = new TimelineClipItem { TrackId = timeline.Tracks[0].Id, StartTime = 0, Duration = 2 };
        timeline.Clips.Add(clip);
        timeline.Host.Selection.SelectSingleClip(clip.Id);

        timeline.CutSelectionToClipboard();

        Assert.Empty(timeline.Clips);
        Assert.Empty(timeline.GetSelectedClips());
    }

    [Fact]
    public void PlayheadChanged_fires_when_seeked()
    {
        var timeline = new VideoTimeline { Duration = 30 };
        double? reported = null;
        timeline.PlayheadChanged += (_, e) => reported = e.TimeSeconds;

        timeline.Seek(12);

        Assert.Equal(12, reported);
    }

    [Fact]
    public void MarkerAdded_fires_when_marker_created()
    {
        var timeline = new VideoTimeline();
        TimelineMarkerItem? reported = null;
        timeline.MarkerAdded += (_, e) => reported = e.Marker;

        var marker = timeline.AddMarker(3, "Cue");

        Assert.Same(marker, reported);
    }

    [Fact]
    public void UndoRedoStateChanged_fires_after_split()
    {
        var timeline = new VideoTimeline { Duration = 30, PlayheadTime = 5 };
        var changes = 0;
        timeline.UndoRedoStateChanged += (_, _) => changes++;
        timeline.AddTrack("V1");
        var clip = new TimelineClipItem { TrackId = timeline.Tracks[0].Id, StartTime = 0, Duration = 10 };
        timeline.Clips.Add(clip);
        timeline.Host.Selection.SelectSingleClip(clip.Id);

        timeline.SplitClipAtPlayhead();

        Assert.True(changes > 0);
        Assert.True(timeline.CanUndo);
    }

    [Fact]
    public void RegisterSnapTargetProvider_snaps_through_control_api()
    {
        var timeline = new VideoTimeline();
        timeline.SnapSettings.IsEnabled = true;
        timeline.SnapSettings.SnapToPlayhead = false;
        timeline.SnapSettings.SnapToMarkers = false;
        timeline.SnapSettings.SnapToClipEdges = false;
        timeline.SnapSettings.SnapToGrid = false;
        timeline.SnapSettings.SnapThresholdSeconds = 0.5;
        var provider = new FixedSnapProvider(20);
        timeline.RegisterSnapTargetProvider(provider);

        var snapped = timeline.Host.Layout.SnapTime(19.7, 0, [], []);

        Assert.Equal(20, snapped);
        timeline.UnregisterSnapTargetProvider(provider);
    }

    private sealed class FixedSnapProvider(double time) : ITimelineSnapTargetProvider
    {
        public IEnumerable<double> GetSnapTimes()
        {
            yield return time;
        }
    }
}
