namespace SkyUI.Controls.Timeline.Time;

/// <summary>Formats ruler labels as mm:ss:ff using the project frame rate.</summary>
public sealed class TimecodeRulerLabelFormatter : ITimelineRulerLabelFormatter
{
    private readonly ITimelineFrameQuantizer quantizer;

    public TimecodeRulerLabelFormatter(ITimelineFrameQuantizer quantizer) =>
        this.quantizer = quantizer;

    public string Format(double timeSeconds)
    {
        if (!quantizer.IsActive)
            return "0:00:00";

        var totalFrames = quantizer.ToFrame(timeSeconds);
        var fps = (int)Math.Round(quantizer.FramesPerSecond);
        if (fps < 1)
            fps = 1;
        var ff = totalFrames % fps;
        var totalSeconds = totalFrames / fps;
        var ss = totalSeconds % 60;
        var mm = (totalSeconds / 60) % 60;
        var hh = totalSeconds / 3600;
        return hh > 0 ? $"{hh}:{mm:00}:{ss:00}:{ff:00}" : $"{mm}:{ss:00}:{ff:00}";
    }
}
