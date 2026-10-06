namespace SkyUI.Controls.Timeline.Time;

/// <summary>Formats ruler tick labels for a timeline time base.</summary>
public interface ITimelineRulerLabelFormatter
{
    string Format(double timeSeconds);
}
