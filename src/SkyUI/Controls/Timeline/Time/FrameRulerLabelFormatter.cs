namespace SkyUI.Controls.Timeline.Time;

/// <summary>Formats ruler labels as frame indices (e.g. f120).</summary>
public sealed class FrameRulerLabelFormatter : ITimelineRulerLabelFormatter
{
    private readonly ITimelineFrameQuantizer quantizer;

    public FrameRulerLabelFormatter(ITimelineFrameQuantizer quantizer) =>
        this.quantizer = quantizer;

    public string Format(double timeSeconds)
    {
        if (!quantizer.IsActive)
            return "f0";
        return $"f{quantizer.ToFrame(timeSeconds)}";
    }
}
