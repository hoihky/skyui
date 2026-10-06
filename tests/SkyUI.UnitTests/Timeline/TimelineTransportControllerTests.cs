using SkyUI.Controls.Timeline.Model;
using SkyUI.Controls.Timeline.Time;
using SkyUI.Controls.Timeline.Transport;

namespace SkyUI.UnitTests.Timeline;

public class TimelineTransportControllerTests
{
    [Fact]
    public void StepPlayheadByFrames_moves_discrete_frames()
    {
        var presentation = new TimelineTimePresentation
        {
            TimeUnit = TimelineTimeUnit.Frames,
            FramesPerSecond = 24,
        };
        var transport = new TimelineTransportController(presentation);

        var next = transport.StepPlayheadByFrames(1.0 / 24, 2, 10);
        Assert.Equal(3.0 / 24, next, 6);
    }

    [Fact]
    public void QuantizePlayhead_snaps_in_frame_mode()
    {
        var presentation = new TimelineTimePresentation
        {
            TimeUnit = TimelineTimeUnit.Frames,
            FramesPerSecond = 24,
        };
        var transport = new TimelineTransportController(presentation);

        var q = transport.QuantizePlayhead(0.11, 10);
        Assert.Equal(3.0 / 24, q, 6);
    }
}
