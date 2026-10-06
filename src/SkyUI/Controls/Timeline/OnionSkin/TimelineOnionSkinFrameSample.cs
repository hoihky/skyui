using SkyUI.Controls.Timeline.Composition;

namespace SkyUI.Controls.Timeline.OnionSkin;

public sealed class TimelineOnionSkinFrameSample
{
    public TimelineOnionSkinFrameSample(
        int frame,
        bool isCenterFrame,
        IReadOnlyList<TimelineSpriteLayerFrameSample> layers)
    {
        Frame = frame;
        IsCenterFrame = isCenterFrame;
        Layers = layers;
    }

    public int Frame { get; }

    public bool IsCenterFrame { get; }

    public IReadOnlyList<TimelineSpriteLayerFrameSample> Layers { get; }
}
