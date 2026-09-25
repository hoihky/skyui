using System.Collections.ObjectModel;
using SkyUI.Controls;
using SkyUI.Controls.Timeline.Editing;

namespace SkyUI.UnitTests.Timeline;

public class TimelineClipOperationsTests
{
    [Fact]
    public void SplitAt_creates_right_segment_and_shortens_left()
    {
        var clips = new ObservableCollection<TimelineClipItem>();
        var left = new TimelineClipItem { StartTime = 0, Duration = 10, Label = "A" };
        clips.Add(left);

        var right = TimelineClipOperations.SplitAt(left, 4, clips);

        Assert.NotNull(right);
        Assert.Equal(4, left.Duration);
        Assert.Equal(4, right!.StartTime);
        Assert.Equal(6, right.Duration);
        Assert.Equal(2, clips.Count);
    }

    [Fact]
    public void SplitAt_returns_null_when_too_close_to_edges()
    {
        var clips = new ObservableCollection<TimelineClipItem>();
        var clip = new TimelineClipItem { StartTime = 0, Duration = 1 };
        clips.Add(clip);

        Assert.Null(TimelineClipOperations.SplitAt(clip, 0.01, clips));
        Assert.Null(TimelineClipOperations.SplitAt(clip, 0.99, clips));
        Assert.Single(clips);
    }

    [Fact]
    public void ClonePrototype_copies_editable_fields()
    {
        var source = new TimelineClipItem
        {
            TrackId = "t1",
            StartTime = 2,
            Duration = 5,
            Label = "Intro",
            Tag = 42,
        };

        var clone = TimelineClipOperations.ClonePrototype(source);

        Assert.NotEqual(source.Id, clone.Id);
        Assert.Equal(source.TrackId, clone.TrackId);
        Assert.Equal(source.StartTime, clone.StartTime);
        Assert.Equal(source.Duration, clone.Duration);
        Assert.Equal(source.Label, clone.Label);
        Assert.Equal(source.Tag, clone.Tag);
    }

    [Fact]
    public void IntersectsMarquee_respects_row_and_time_bounds()
    {
        var clip = new TimelineClipItem { StartTime = 5, Duration = 4 };
        Assert.True(TimelineClipOperations.IntersectsMarquee(clip, 1, 4, 10, 0, 2));
        Assert.False(TimelineClipOperations.IntersectsMarquee(clip, 3, 4, 10, 0, 2));
        Assert.False(TimelineClipOperations.IntersectsMarquee(clip, 1, 0, 4, 0, 2));
    }

    [Fact]
    public void IntersectsTimeRange_detects_overlap_and_touching_edges()
    {
        var clip = new TimelineClipItem { StartTime = 5, Duration = 4 };
        Assert.True(TimelineClipOperations.IntersectsTimeRange(clip, 4, 6));
        Assert.True(TimelineClipOperations.IntersectsTimeRange(clip, 8, 12));
        Assert.False(TimelineClipOperations.IntersectsTimeRange(clip, 0, 5));
        Assert.False(TimelineClipOperations.IntersectsTimeRange(clip, 9, 12));
    }

    [Fact]
    public void SplitAt_appends_numbered_label_to_right_segment()
    {
        var clips = new ObservableCollection<TimelineClipItem>();
        var left = new TimelineClipItem { StartTime = 0, Duration = 10, Label = "Scene" };
        clips.Add(left);

        var right = TimelineClipOperations.SplitAt(left, 5, clips);

        Assert.NotNull(right);
        Assert.Equal("Scene (2)", right!.Label);
    }
}
