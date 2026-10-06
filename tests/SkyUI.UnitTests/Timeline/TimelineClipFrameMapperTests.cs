using SkyUI.Controls;
using SkyUI.Controls.Timeline.Frame;
using SkyUI.Controls.Timeline.Model;
using SkyUI.Controls.Timeline.Time;

namespace SkyUI.UnitTests.Timeline;

public class TimelineClipFrameMapperTests
{
    [Fact]
    public void DescribeClip_maps_start_and_duration_frames()
    {
        var presentation = new TimelineTimePresentation { TimeUnit = TimelineTimeUnit.Frames, FramesPerSecond = 24 };
        var mapper = new TimelineClipFrameMapper(presentation);
        var clip = new TimelineClipItem { StartTime = 2.0 / 24, Duration = 48.0 / 24 };
        var span = mapper.DescribeClip(clip);
        Assert.Equal(2, span.StartFrame);
        Assert.Equal(48, span.DurationFrames);
        Assert.True(span.ContainsFrame(10));
        Assert.False(span.ContainsFrame(50));
    }
}
