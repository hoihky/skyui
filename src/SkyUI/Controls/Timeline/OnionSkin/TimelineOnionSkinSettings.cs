namespace SkyUI.Controls.Timeline.OnionSkin;

/// <summary>Host preview onion-skin configuration (ghost frames before/after playhead).</summary>
public sealed class TimelineOnionSkinSettings
{
    public bool IsEnabled { get; set; }

    public int PreviousFrameCount { get; set; } = 2;

    public int NextFrameCount { get; set; } = 2;

    public void Clamp()
    {
        if (PreviousFrameCount < 0)
            PreviousFrameCount = 0;
        if (NextFrameCount < 0)
            NextFrameCount = 0;
        if (PreviousFrameCount > 24)
            PreviousFrameCount = 24;
        if (NextFrameCount > 24)
            NextFrameCount = 24;
    }
}
