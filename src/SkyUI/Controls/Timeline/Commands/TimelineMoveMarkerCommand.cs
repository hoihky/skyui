namespace SkyUI.Controls.Timeline.Commands;

internal sealed class TimelineMoveMarkerCommand : ITimelineEditCommand
{
    private readonly TimelineMarkerItem marker;
    private readonly double beforeTime;
    private readonly double afterTime;

    public TimelineMoveMarkerCommand(TimelineMarkerItem marker, double beforeTime, double afterTime)
    {
        this.marker = marker;
        this.beforeTime = beforeTime;
        this.afterTime = afterTime;
    }

    public string Description => "Move marker";

    public void Execute() => marker.Time = afterTime;

    public void Undo() => marker.Time = beforeTime;
}
