namespace SkyUI.Core.Theming;

/// <summary>Default motion durations aligned with <see cref="SkyTokenKeys.Motion"/> resources.</summary>
public static class SkyMotionDurations
{
    public static readonly TimeSpan Instant = TimeSpan.Zero;
    public static readonly TimeSpan Fast = TimeSpan.FromMilliseconds(150);
    public static readonly TimeSpan Medium = TimeSpan.FromMilliseconds(250);
    public static readonly TimeSpan Slow = TimeSpan.FromMilliseconds(350);
    public static readonly TimeSpan Enter = TimeSpan.FromMilliseconds(250);
    public static readonly TimeSpan Exit = TimeSpan.FromMilliseconds(200);
}
