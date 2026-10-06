using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using SkyUI.Controls;

namespace SkyUI.HeadlessTests;

public class VideoTimelineThemedHeadlessTests
{
    private static readonly HeadlessUnitTestSession Session =
        HeadlessUnitTestSession.GetOrStartForAssembly(typeof(VideoTimelineThemedHeadlessTests).Assembly);

    [Fact]
    public void Null_clip_thumbnail_provider_does_not_break_template()
    {
        Session.Dispatch(() =>
        {
            var window = CreateWindowWithTimeline(out var timeline);
            timeline.ClipThumbnailProvider = SkyUI.Controls.Timeline.Integration.NullTimelineClipThumbnailProvider.Instance;
            LayoutTimeline(timeline);
            Assert.NotNull(timeline.Host.Interaction);
            window.Close();
        }, CancellationToken.None);
    }

    [Fact]
    public void Template_applies_and_creates_interaction_context()
    {
        Session.Dispatch(() =>
        {
            var window = CreateWindowWithTimeline(out var timeline);

            Assert.NotNull(timeline.Host.Interaction);
            Assert.NotNull(timeline.Host.Interaction!.MainCanvas);
            Assert.NotNull(timeline.Host.Interaction.RulerScroll);
            Assert.NotNull(timeline.Host.Interaction.VerticalTrackScroll);

            window.Close();
        }, CancellationToken.None);
    }

    [Fact]
    public void Copy_and_paste_at_playhead_with_template()
    {
        Session.Dispatch(() =>
        {
            var window = CreateWindowWithTimeline(out var timeline);
            timeline.AddTrack("V1");
            var clip = new TimelineClipItem
            {
                TrackId = timeline.Tracks[0].Id,
                StartTime = 2,
                Duration = 4,
                Label = "Shot",
            };
            timeline.Clips.Add(clip);
            timeline.Host.Selection.SelectSingleClip(clip.Id);

            timeline.CopySelectionToClipboard();
            timeline.Host.Selection.ClearClipSelection();
            timeline.Seek(10);
            timeline.PasteClipboardAtPlayhead();

            Assert.Equal(2, timeline.Clips.Count);
            var pasted = timeline.Clips.First(c => c.Id != clip.Id);
            Assert.Equal(10, pasted.StartTime);
            Assert.Equal(4, pasted.Duration);
            Assert.Equal("Shot", pasted.Label);

            window.Close();
        }, CancellationToken.None);
    }

    [Fact]
    public void Clip_visual_syncs_when_moved_to_newly_added_track()
    {
        Session.Dispatch(() =>
        {
            var window = CreateWindowWithTimeline(out var timeline);
            timeline.AddTrack("V1");
            var clip = new TimelineClipItem
            {
                TrackId = timeline.Tracks[0].Id,
                StartTime = 1,
                Duration = 3,
            };
            timeline.Clips.Add(clip);
            timeline.AddTrack("V2");
            var renderer = timeline.Host.Interaction!.Renderer;

            clip.TrackId = timeline.Tracks[1].Id;

            Assert.Contains(clip.Id, renderer.ClipBodies.Keys);
            window.Close();
        }, CancellationToken.None);
    }

    [Fact]
    public void Ruler_hit_target_exists_after_template()
    {
        Session.Dispatch(() =>
        {
            var window = CreateWindowWithTimeline(out var timeline);
            var canvas = timeline.Host.Interaction!.RulerCanvas!;

            Assert.Contains(canvas.Children, c => c is Avalonia.Controls.Shapes.Rectangle);

            window.Close();
        }, CancellationToken.None);
    }

    private static Window CreateWindowWithTimeline(out VideoTimeline timeline)
    {
        timeline = new VideoTimeline
        {
            Width = 720,
            Height = 280,
            Duration = 60,
            PixelsPerSecond = 40,
        };

        var window = new Window
        {
            Width = 800,
            Height = 360,
            Content = timeline,
        };
        window.Show();
        LayoutTimeline(timeline);
        return window;
    }

    private static void LayoutTimeline(VideoTimeline timeline)
    {
        timeline.Measure(new Size(timeline.Width, timeline.Height));
        timeline.Arrange(new Rect(0, 0, timeline.Width, timeline.Height));
    }
}
