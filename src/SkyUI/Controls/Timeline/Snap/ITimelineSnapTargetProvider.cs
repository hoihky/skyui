namespace SkyUI.Controls;

/// <summary>Supplies additional snap times (beats, keyframes, custom guides).</summary>
public interface ITimelineSnapTargetProvider
{
    IEnumerable<double> GetSnapTimes();
}
