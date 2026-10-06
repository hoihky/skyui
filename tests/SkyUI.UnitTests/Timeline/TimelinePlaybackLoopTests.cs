using SkyUI.Controls;
using SkyUI.Controls.Timeline.Transport;

namespace SkyUI.UnitTests.Timeline;

public class TimelinePlaybackLoopTests
{
    [Fact]
    public void Advance_wraps_inside_loop_range()
    {
        var loop = new TimelinePlaybackLoop
        {
            IsEnabled = true,
            LoopStartSeconds = 2,
            LoopEndSeconds = 4,
        };

        var next = loop.Advance(3.9, 0.2, 10);
        Assert.Equal(2.1, next, 3);
    }

    [Fact]
    public void SetFromRange_uses_range_min_max()
    {
        var loop = new TimelinePlaybackLoop();
        loop.SetFromRange(new TimelineTimeRange(8, 2));
        Assert.Equal(2, loop.LoopStartSeconds);
        Assert.Equal(8, loop.LoopEndSeconds);
        Assert.True(loop.HasValidLoop);
    }
}
