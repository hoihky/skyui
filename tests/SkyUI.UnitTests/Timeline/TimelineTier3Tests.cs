using SkyUI.Controls;
using SkyUI.Controls.Timeline.Editing;
using SkyUI.Controls.Timeline.Frame;
using SkyUI.Controls.Timeline.Keyframes;
using SkyUI.Controls.Timeline.Model;
using SkyUI.Controls.Timeline.Commands;
using SkyUI.Controls.Timeline.OnionSkin;
using SkyUI.Controls.Timeline.Time;

namespace SkyUI.UnitTests.Timeline;

public class TimelineTier3Tests
{
    [Fact]
    public void ClipHoldEditor_extends_duration_in_whole_frames()
    {
        var presentation = new TimelineTimePresentation { FramesPerSecond = 24, TimeUnit = TimelineTimeUnit.Frames };
        var mapper = new TimelineClipFrameMapper(presentation);
        var editor = new TimelineClipHoldEditor(mapper);
        var clip = new TimelineClipItem { StartTime = 0, Duration = 2.0 / 24 };
        var newDuration = editor.ComputeDurationAfterHoldDelta(clip, 2, 10);
        Assert.Equal(4.0 / 24, newDuration, 6);
    }

    [Fact]
    public void OnionSkin_planner_includes_neighbors()
    {
        var planner = new TimelineOnionSkinFramePlanner();
        var settings = new TimelineOnionSkinSettings
        {
            IsEnabled = true,
            PreviousFrameCount = 2,
            NextFrameCount = 1,
        };
        var frames = planner.PlanFrames(settings, 10, 120);
        Assert.Contains(8, frames);
        Assert.Contains(9, frames);
        Assert.Contains(10, frames);
        Assert.Contains(11, frames);
    }

    [Fact]
    public void KeyframeEditor_adds_and_updates_with_undo()
    {
        var project = new TimelineProject();
        var track = new TimelineTrackItem { Kind = TimelineTrackKind.Property };
        project.Tracks.Add(track);
        var catalog = new TimelineKeyframeCatalog(project);
        var stack = new TimelineUndoStack();
        var editor = new TimelineKeyframeEditor(project, catalog, stack);
        var kf = editor.SetKeyframe(track.Id, "Opacity", 1, 0.5);
        Assert.Single(project.Keyframes);
        editor.SetKeyframe(track.Id, "Opacity", 1, 1.0);
        Assert.Equal(1.0, kf.Value);
        stack.Undo();
        Assert.Equal(0.5, kf.Value);
    }

    [Fact]
    public void VideoTimeline_extend_hold_updates_selected_clip()
    {
        var timeline = new VideoTimeline { Duration = 10, Fps = 24, TimeUnit = TimelineTimeUnit.Frames };
        timeline.AddSpriteTrack("L");
        var clip = new TimelineClipItem
        {
            TrackId = timeline.Tracks[0].Id,
            StartTime = 0,
            Duration = 24.0 / 24,
        };
        timeline.Clips.Add(clip);
        timeline.Host.Selection.SelectSingleClip(clip.Id);
        Assert.True(timeline.ExtendSelectedClipHoldFrames(12));
        Assert.Equal(36.0 / 24, clip.Duration, 6);
    }

    [Fact]
    public void Marker_snap_provider_yields_marker_times()
    {
        var project = new TimelineProject();
        project.Markers.Add(new TimelineMarkerItem { Time = 1.5 });
        var provider = new SkyUI.Controls.Timeline.Snap.TimelineMarkerSnapTargetProvider(project);
        Assert.Contains(1.5, provider.GetSnapTimes());
    }
}
