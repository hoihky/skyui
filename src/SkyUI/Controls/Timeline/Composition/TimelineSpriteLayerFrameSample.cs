using SkyUI.Controls.Timeline.Model;

namespace SkyUI.Controls.Timeline.Composition;

/// <summary>Active cel on one sprite layer at a timeline frame.</summary>
public sealed class TimelineSpriteLayerFrameSample
{
    public TimelineSpriteLayerFrameSample(
        TimelineTrack track,
        TimelineClipItem? clip,
        TimelineSpriteClipMetadata? sprite)
    {
        Track = track;
        Clip = clip;
        Sprite = sprite;
    }

    public TimelineTrack Track { get; }

    public TimelineClipItem? Clip { get; }

    public TimelineSpriteClipMetadata? Sprite { get; }
}
