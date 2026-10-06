namespace SkyUI.Controls.Timeline.Commands;

internal sealed class TimelineMoveKeyframeCommand : ITimelineEditCommand
{
    private readonly TimelineKeyframeItem keyframe;
    private readonly double beforeTime;
    private readonly double afterTime;

    public TimelineMoveKeyframeCommand(TimelineKeyframeItem keyframe, double beforeTime, double afterTime)
    {
        this.keyframe = keyframe;
        this.beforeTime = beforeTime;
        this.afterTime = afterTime;
    }

    public string Description => "Move keyframe";

    public void Execute() => keyframe.Time = afterTime;

    public void Undo() => keyframe.Time = beforeTime;
}
