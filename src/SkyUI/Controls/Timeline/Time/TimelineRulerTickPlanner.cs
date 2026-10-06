using SkyUI.Controls.Timeline.Model;

namespace SkyUI.Controls.Timeline.Time;

/// <summary>Chooses ruler tick spacing for seconds-based or frame-based timelines.</summary>
public sealed class TimelineRulerTickPlanner
{
    public double ComputeTickStepSeconds(
        TimelineTimeUnit timeUnit,
        ITimelineFrameQuantizer quantizer,
        double durationSeconds,
        double widthPixels)
    {
        if (timeUnit == TimelineTimeUnit.Frames && quantizer.IsActive)
            return ComputeFrameTickStep(quantizer, durationSeconds, widthPixels);

        return ComputeSecondsTickStep(durationSeconds, widthPixels);
    }

    private static double ComputeFrameTickStep(
        ITimelineFrameQuantizer quantizer,
        double durationSeconds,
        double widthPixels)
    {
        var totalFrames = Math.Max(1, quantizer.ToFrame(durationSeconds));
        if (widthPixels < 120)
            return quantizer.ToSeconds(Math.Max(1, totalFrames / 4));

        var targetLabels = Math.Max(4, widthPixels / 90);
        var rawFrames = totalFrames / targetLabels;
        var stepFrames = PickNiceFrameStep(rawFrames);
        return quantizer.ToSeconds(stepFrames);
    }

    private static int PickNiceFrameStep(double rawFrames)
    {
        if (rawFrames <= 1)
            return 1;
        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(Math.Max(rawFrames, 1))));
        var normalized = rawFrames / magnitude;
        var multiplier = normalized <= 1 ? 1 : normalized <= 2 ? 2 : normalized <= 5 ? 5 : 10;
        return (int)Math.Max(1, Math.Round(multiplier * magnitude));
    }

    private double ComputeSecondsTickStep(double durationSeconds, double widthPixels)
    {
        if (widthPixels < 120)
            return Math.Max(1, durationSeconds / 4);

        var targetLabels = Math.Max(4, widthPixels / 90);
        var raw = durationSeconds / targetLabels;
        var pow = Math.Pow(10, Math.Floor(Math.Log10(Math.Max(raw, 0.001))));
        var normalized = raw / pow;
        var multiplier = normalized <= 1 ? 1 : normalized <= 2 ? 2 : normalized <= 5 ? 5 : 10;
        return multiplier * pow;
    }
}
