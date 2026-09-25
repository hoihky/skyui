using SkyUI.Controls;
using SkyUI.Controls.Timeline.Model;

namespace SkyUI.UnitTests.Timeline;

public class TimelineModelTests
{
    [Fact]
    public void TimelineTrackItem_inherits_model_track()
    {
        var item = new TimelineTrackItem { Name = "Audio" };
        Assert.IsAssignableFrom<TimelineTrack>(item);
        Assert.False(string.IsNullOrEmpty(item.Id));
        Assert.Equal("Audio", item.Name);
    }

    [Fact]
    public void TimelineClipItem_notifies_on_start_time_change()
    {
        var clip = new TimelineClipItem();
        var changes = 0;
        clip.PropertyChanged += (_, _) => changes++;
        clip.StartTime = 3;
        clip.StartTime = 3;
        clip.StartTime = 4;
        Assert.Equal(2, changes);
    }

    [Fact]
    public void TimelineMarkerItem_clamps_nan_time_to_zero()
    {
        var marker = new TimelineMarkerItem { Time = double.NaN };
        Assert.Equal(0, marker.Time);
    }

    [Fact]
    public void TimelineProject_defaults()
    {
        var project = new TimelineProject();
        Assert.Empty(project.Tracks);
        Assert.Empty(project.Clips);
        Assert.Empty(project.Markers);
        Assert.Empty(project.Keyframes);
        Assert.Equal(120, project.Duration);
        Assert.Equal(TimelineTimeUnit.Seconds, project.TimeUnit);
        Assert.Equal(30, project.Fps);
    }

    [Fact]
    public void TimelineKeyframe_stores_lane_metadata()
    {
        var key = new TimelineKeyframe
        {
            TrackId = "t1",
            PropertyName = "Opacity",
            Time = 1.5,
            Value = 0.5,
        };
        Assert.Equal("t1", key.TrackId);
        Assert.Equal("Opacity", key.PropertyName);
        Assert.Equal(0.5, key.Value);
    }
}
