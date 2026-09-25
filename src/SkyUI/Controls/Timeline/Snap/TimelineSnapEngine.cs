namespace SkyUI.Controls.Timeline.Layout;

/// <summary>Resolves the nearest snap target for a proposed timeline time.</summary>
public sealed class TimelineSnapEngine
{
    private readonly List<ITimelineSnapTargetProvider> providers = new();

    public TimelineSnapSettings Settings { get; } = new();

    public void RegisterProvider(ITimelineSnapTargetProvider provider)
    {
        if (provider is null)
            throw new ArgumentNullException(nameof(provider));
        if (!providers.Contains(provider))
            providers.Add(provider);
    }

    public void UnregisterProvider(ITimelineSnapTargetProvider provider) => providers.Remove(provider);

    public double SnapTime(
        double proposedTimeSeconds,
        double playheadTimeSeconds,
        IReadOnlyList<TimelineMarkerItem> markers,
        IReadOnlyList<TimelineClipItem> clips,
        string? excludeClipId = null)
    {
        if (!Settings.IsEnabled || Settings.SnapThresholdSeconds <= 0)
            return proposedTimeSeconds;

        var threshold = Settings.SnapThresholdSeconds;
        var candidates = CollectCandidates(playheadTimeSeconds, markers, clips, excludeClipId);
        return SnapToNearest(proposedTimeSeconds, candidates, threshold);
    }

    public double SnapClipStart(
        double proposedStartSeconds,
        double clipDurationSeconds,
        double playheadTimeSeconds,
        IReadOnlyList<TimelineMarkerItem> markers,
        IReadOnlyList<TimelineClipItem> clips,
        string? movingClipId = null)
    {
        var proposedEnd = proposedStartSeconds + clipDurationSeconds;
        var snappedStart = SnapTime(proposedStartSeconds, playheadTimeSeconds, markers, clips, movingClipId);
        var snappedEnd = SnapTime(proposedEnd, playheadTimeSeconds, markers, clips, movingClipId);
        var endSnapped = Math.Abs(snappedEnd - proposedEnd) > 1e-9;
        var startSnapped = Math.Abs(snappedStart - proposedStartSeconds) > 1e-9;
        if (endSnapped
            && Math.Abs(snappedEnd - snappedStart) >= TimelineCoordinateSystem.MinClipDurationSeconds
            && (!startSnapped || Math.Abs(snappedEnd - proposedEnd) < Math.Abs(snappedStart - proposedStartSeconds)))
            return snappedEnd - clipDurationSeconds;

        return snappedStart;
    }

    private IEnumerable<double> CollectCandidates(
        double playheadTimeSeconds,
        IReadOnlyList<TimelineMarkerItem> markers,
        IReadOnlyList<TimelineClipItem> clips,
        string? excludeClipId)
    {
        if (Settings.SnapToPlayhead)
            yield return playheadTimeSeconds;

        if (Settings.SnapToMarkers)
        {
            foreach (var marker in markers)
                yield return marker.Time;
        }

        if (Settings.SnapToClipEdges)
        {
            foreach (var clip in clips)
            {
                if (clip.Id == excludeClipId)
                    continue;
                yield return clip.StartTime;
                yield return clip.StartTime + clip.Duration;
            }
        }

        if (Settings.SnapToGrid && Settings.GridIntervalSeconds > 0)
        {
            var grid = Settings.GridIntervalSeconds;
            var center = playheadTimeSeconds;
            var baseIndex = (int)Math.Floor(center / grid);
            for (var i = baseIndex - 3; i <= baseIndex + 3; i++)
                yield return i * grid;
        }

        foreach (var provider in providers)
        {
            foreach (var time in provider.GetSnapTimes())
                yield return time;
        }
    }

    private static double SnapToNearest(double proposed, IEnumerable<double> candidates, double threshold)
    {
        var best = proposed;
        var bestDistance = threshold;
        foreach (var candidate in candidates)
        {
            var distance = Math.Abs(candidate - proposed);
            if (distance <= bestDistance)
            {
                bestDistance = distance;
                best = candidate;
            }
        }

        return best;
    }
}
