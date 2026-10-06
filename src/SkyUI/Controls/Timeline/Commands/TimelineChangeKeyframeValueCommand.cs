namespace SkyUI.Controls.Timeline.Commands;

internal sealed class TimelineChangeKeyframeValueCommand : ITimelineEditCommand
{
    private readonly TimelineKeyframeItem keyframe;
    private readonly object? beforeValue;
    private readonly object? afterValue;

    public TimelineChangeKeyframeValueCommand(
        TimelineKeyframeItem keyframe,
        object? beforeValue,
        object? afterValue)
    {
        this.keyframe = keyframe;
        this.beforeValue = beforeValue;
        this.afterValue = afterValue;
    }

    public string Description => "Change keyframe value";

    public void Execute() => keyframe.Value = afterValue;

    public void Undo() => keyframe.Value = beforeValue;
}
