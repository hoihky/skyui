using SkyUI.Controls.Timeline.Composition;

namespace SkyUI.Controls.Timeline.OnionSkin;

public sealed class TimelineOnionSkinSampler
{
    private readonly TimelineOnionSkinFramePlanner framePlanner = new();
    private readonly TimelineSpriteFrameSampler spriteSampler;

    public TimelineOnionSkinSampler(TimelineSpriteFrameSampler spriteSampler) =>
        this.spriteSampler = spriteSampler;

    public IReadOnlyList<TimelineOnionSkinFrameSample> Sample(
        TimelineOnionSkinSettings settings,
        int centerFrame,
        int maxFrameInclusive,
        IReadOnlyList<TimelineClipItem> clips)
    {
        var frames = framePlanner.PlanFrames(settings, centerFrame, maxFrameInclusive);
        var results = new List<TimelineOnionSkinFrameSample>(frames.Count);
        foreach (var frame in frames)
        {
            var layers = spriteSampler.SampleAtFrame(frame, clips);
            results.Add(new TimelineOnionSkinFrameSample(frame, frame == centerFrame, layers));
        }

        return results;
    }
}
