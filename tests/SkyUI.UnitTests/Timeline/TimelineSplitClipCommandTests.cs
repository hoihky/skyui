using System.Collections.ObjectModel;
using SkyUI.Controls;
using SkyUI.Controls.Timeline.Commands;

namespace SkyUI.UnitTests.Timeline;

public class TimelineSplitClipCommandTests
{
    [Fact]
    public void Split_undo_removes_right_segment_and_restores_duration()
    {
        var clips = new ObservableCollection<TimelineClipItem>();
        var left = new TimelineClipItem { StartTime = 0, Duration = 10 };
        clips.Add(left);

        var command = new TimelineSplitClipCommand(left, clips, 4, 10);
        command.Execute();

        var right = clips.First(c => c.Id != left.Id);
        Assert.Equal(4, left.Duration);
        Assert.Equal(4, right.StartTime);

        command.Undo();
        Assert.Single(clips);
        Assert.Equal(10, left.Duration);
    }

    [Fact]
    public void Execute_twice_is_idempotent_when_right_segment_exists()
    {
        var clips = new ObservableCollection<TimelineClipItem>();
        var left = new TimelineClipItem { StartTime = 0, Duration = 10 };
        clips.Add(left);

        var command = new TimelineSplitClipCommand(left, clips, 4, 10);
        command.Execute();
        command.Execute();

        Assert.Equal(2, clips.Count);
        Assert.Equal(4, left.Duration);
    }
}
