using SkyUI.Controls.Timeline.Model;

namespace SkyUI.Controls.Timeline.Snap;

/// <summary>Exposes marker times as magnetic snap targets.</summary>
public sealed class TimelineMarkerSnapTargetProvider : ITimelineSnapTargetProvider
{
    private readonly TimelineProject project;

    public TimelineMarkerSnapTargetProvider(TimelineProject project) => this.project = project;

    public IEnumerable<double> GetSnapTimes()
    {
        foreach (var marker in project.Markers)
            yield return marker.Time;
    }
}
