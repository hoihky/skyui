using SkyUI.Controls;
using SkyUI.Controls.Timeline.Model;
using SkyUI.Controls.Timeline.Time;

namespace SkyUI.UnitTests.Timeline;

public class TimelineTimePresentationTests
{
    [Fact]
    public void Frame_mode_uses_frame_ruler_labels()
    {
        var presentation = new TimelineTimePresentation
        {
            TimeUnit = TimelineTimeUnit.Frames,
            FramesPerSecond = 24,
        };

        Assert.Equal("f48", presentation.RulerLabels.Format(2));
    }

    [Fact]
    public void ApplyFrameSnapTo_sets_grid_to_one_frame()
    {
        var presentation = new TimelineTimePresentation
        {
            TimeUnit = TimelineTimeUnit.Frames,
            FramesPerSecond = 30,
        };
        var snap = new TimelineSnapSettings();
        presentation.ApplyFrameSnapTo(snap);

        Assert.True(snap.SnapToGrid);
        Assert.Equal(1.0 / 30, snap.GridIntervalSeconds, 6);
    }
}
