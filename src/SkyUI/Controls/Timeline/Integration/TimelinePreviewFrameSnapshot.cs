using SkyUI.Controls.Timeline.Composition;
using SkyUI.Controls.Timeline.OnionSkin;

namespace SkyUI.Controls.Timeline.Integration;

/// <summary>Single compositor frame: playhead time plus sampled sprite layers (and optional onion skin).</summary>
public sealed class TimelinePreviewFrameSnapshot
{
    public TimelinePreviewFrameSnapshot(
        int frame,
        double timeSeconds,
        IReadOnlyList<TimelineSpriteLayerFrameSample> spriteLayers,
        IReadOnlyList<TimelineOnionSkinFrameSample>? onionSkinFrames)
    {
        Frame = frame;
        TimeSeconds = timeSeconds;
        SpriteLayers = spriteLayers;
        OnionSkinFrames = onionSkinFrames;
    }

    public int Frame { get; }

    public double TimeSeconds { get; }

    public IReadOnlyList<TimelineSpriteLayerFrameSample> SpriteLayers { get; }

    public IReadOnlyList<TimelineOnionSkinFrameSample>? OnionSkinFrames { get; }
}
