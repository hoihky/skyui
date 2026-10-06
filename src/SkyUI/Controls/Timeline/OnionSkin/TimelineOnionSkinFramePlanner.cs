namespace SkyUI.Controls.Timeline.OnionSkin;

/// <summary>Builds the list of timeline frame indices used for onion skin sampling.</summary>
public sealed class TimelineOnionSkinFramePlanner
{
    public IReadOnlyList<int> PlanFrames(
        TimelineOnionSkinSettings settings,
        int centerFrame,
        int maxFrameInclusive)
    {
        settings.Clamp();
        if (!settings.IsEnabled)
            return [Math.Clamp(centerFrame, 0, maxFrameInclusive)];

        var frames = new SortedSet<int> { Math.Clamp(centerFrame, 0, maxFrameInclusive) };
        for (var i = 1; i <= settings.PreviousFrameCount; i++)
        {
            var f = centerFrame - i;
            if (f >= 0)
                frames.Add(f);
        }

        for (var i = 1; i <= settings.NextFrameCount; i++)
        {
            var f = centerFrame + i;
            if (f <= maxFrameInclusive)
                frames.Add(f);
        }

        return frames.ToList();
    }
}
