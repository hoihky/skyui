using SkyUI.Controls.Timeline.Frame;
using SkyUI.Controls.Timeline.Model;

namespace SkyUI.Controls.Timeline.Composition;

/// <summary>Resolves which sprite clips are active per layer at a timeline frame.</summary>
public sealed class TimelineSpriteFrameSampler
{
    private readonly TimelineTrackCatalog trackCatalog;
    private readonly ITimelineClipFrameMapper frameMapper;

    public TimelineSpriteFrameSampler(TimelineTrackCatalog trackCatalog, ITimelineClipFrameMapper frameMapper)
    {
        this.trackCatalog = trackCatalog;
        this.frameMapper = frameMapper;
    }

    public IReadOnlyList<TimelineSpriteLayerFrameSample> SampleAtFrame(
        int frame,
        IReadOnlyList<TimelineClipItem> clips)
    {
        var layers = trackCatalog.SpriteTracksInOrder();
        var results = new List<TimelineSpriteLayerFrameSample>(layers.Count);
        foreach (var track in layers)
        {
            if (!track.IsVisible)
            {
                results.Add(new TimelineSpriteLayerFrameSample(track, null, null));
                continue;
            }

            var hit = FindClipOnTrackAtFrame(track.Id, frame, clips);
            results.Add(new TimelineSpriteLayerFrameSample(track, hit, hit?.Sprite));
        }

        return results;
    }

    public TimelineClipItem? FindClipOnTrackAtFrame(string trackId, int frame, IReadOnlyList<TimelineClipItem> clips)
    {
        TimelineClipItem? best = null;
        foreach (var clip in clips)
        {
            if (clip.TrackId != trackId)
                continue;
            var span = frameMapper.DescribeClip(clip);
            if (!span.ContainsFrame(frame))
                continue;
            if (best is null || clip.StartTime > best.StartTime)
                best = clip;
        }

        return best;
    }
}
