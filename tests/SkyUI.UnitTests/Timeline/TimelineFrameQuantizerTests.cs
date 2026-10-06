using SkyUI.Controls.Timeline.Time;

namespace SkyUI.UnitTests.Timeline;

public class TimelineFrameQuantizerTests
{
    [Fact]
    public void QuantizeSeconds_aligns_to_frame_grid()
    {
        var quantizer = new TimelineFrameQuantizer(24);
        var quantized = quantizer.QuantizeSeconds(0.09);
        Assert.Equal(2, quantizer.ToFrame(quantized));
        Assert.Equal(2.0 / 24, quantized, 6);
    }

    [Fact]
    public void ToFrame_and_ToSeconds_round_trip()
    {
        var quantizer = new TimelineFrameQuantizer(30);
        Assert.Equal(45, quantizer.ToFrame(1.5));
        Assert.Equal(1.5, quantizer.ToSeconds(45), 6);
    }

    [Fact]
    public void MinClipDuration_is_one_frame_when_active()
    {
        var quantizer = new TimelineFrameQuantizer(24);
        Assert.Equal(1.0 / 24, quantizer.MinClipDurationSeconds, 6);
    }
}
