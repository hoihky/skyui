using SkyUI.Core.Theming;

namespace SkyUI.UnitTests;

public class SkyMotionTokenTests
{
    [Fact]
    public void Motion_token_keys_are_non_empty()
    {
        Assert.False(string.IsNullOrWhiteSpace(SkyTokenKeys.Motion.DurationEnter));
        Assert.False(string.IsNullOrWhiteSpace(SkyTokenKeys.Motion.DurationExit));
        Assert.False(string.IsNullOrWhiteSpace(SkyTokenKeys.Motion.DurationFast));
        Assert.False(string.IsNullOrWhiteSpace(SkyTokenKeys.Brush.Scrim));
    }

    [Fact]
    public void Motion_durations_match_token_intent()
    {
        Assert.True(SkyMotionDurations.Enter > SkyMotionDurations.Exit);
        Assert.True(SkyMotionDurations.Fast < SkyMotionDurations.Medium);
    }
}
