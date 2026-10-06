using System.Linq;
using SkyUI.Controls.Timeline.Model;

namespace SkyUI.Controls.Timeline.Keyframes;

/// <summary>Queries keyframes for property lanes.</summary>
public sealed class TimelineKeyframeCatalog
{
    private readonly TimelineProject project;

    public TimelineKeyframeCatalog(TimelineProject project) => this.project = project;

    public IReadOnlyList<TimelineKeyframeItem> ForTrack(string trackId) =>
        project.Keyframes.Where(k => k.TrackId == trackId).ToList();

    public IReadOnlyList<TimelineKeyframeItem> ForTrackProperty(string trackId, string propertyName) =>
        project.Keyframes
            .Where(k => k.TrackId == trackId && k.PropertyName == propertyName)
            .OrderBy(k => k.Time)
            .ToList();

    public TimelineKeyframeItem? FindAtTime(string trackId, string propertyName, double timeSeconds, double epsilon = 1e-4)
    {
        foreach (var keyframe in project.Keyframes)
        {
            if (keyframe.TrackId != trackId || keyframe.PropertyName != propertyName)
                continue;
            if (Math.Abs(keyframe.Time - timeSeconds) <= epsilon)
                return keyframe;
        }

        return null;
    }
}
