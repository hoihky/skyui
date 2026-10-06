namespace SkyUI.Controls.Timeline.Time;

/// <summary>Formats ruler labels as mm:ss or h:mm:ss.</summary>
public sealed class SecondsRulerLabelFormatter : ITimelineRulerLabelFormatter
{
    public string Format(double timeSeconds)
    {
        var s = (int)Math.Floor(timeSeconds % 60);
        var m = (int)Math.Floor(timeSeconds / 60) % 60;
        var h = (int)Math.Floor(timeSeconds / 3600);
        return h > 0 ? $"{h}:{m:00}:{s:00}" : $"{m}:{s:00}";
    }
}
