namespace SkyUI.Controls.Timeline.Commands;

internal sealed class TimelineTrimClipCommand : ITimelineEditCommand
{
    private readonly TimelineClipItem clip;
    private readonly double beforeStart;
    private readonly double beforeDuration;
    private readonly double afterStart;
    private readonly double afterDuration;

    public TimelineTrimClipCommand(
        TimelineClipItem clip,
        double beforeStart,
        double beforeDuration,
        double afterStart,
        double afterDuration)
    {
        this.clip = clip;
        this.beforeStart = beforeStart;
        this.beforeDuration = beforeDuration;
        this.afterStart = afterStart;
        this.afterDuration = afterDuration;
    }

    public string Description => "Trim clip";

    public void Execute() => Apply(afterStart, afterDuration);

    public void Undo() => Apply(beforeStart, beforeDuration);

    private void Apply(double start, double duration)
    {
        clip.StartTime = start;
        clip.Duration = duration;
    }
}
