namespace SkyUI.Controls.Timeline.Commands;

internal sealed class TimelineMoveClipsCommand : ITimelineEditCommand
{
    private readonly IReadOnlyList<TimelineClipItem> clips;
    private readonly IReadOnlyList<ClipMoveSnapshot> before;
    private readonly IReadOnlyList<ClipMoveSnapshot> after;

    public TimelineMoveClipsCommand(
        IReadOnlyList<TimelineClipItem> clips,
        IReadOnlyList<ClipMoveSnapshot> before,
        IReadOnlyList<ClipMoveSnapshot> after)
    {
        this.clips = clips;
        this.before = before;
        this.after = after;
    }

    public string Description => "Move clips";

    public void Execute() => Apply(after);

    public void Undo() => Apply(before);

    private void Apply(IReadOnlyList<ClipMoveSnapshot> snapshots)
    {
        foreach (var snapshot in snapshots)
        {
            var clip = clips.FirstOrDefault(c => c.Id == snapshot.ClipId);
            if (clip is null)
                continue;
            clip.StartTime = snapshot.StartTime;
            clip.TrackId = snapshot.TrackId;
        }
    }

    internal readonly record struct ClipMoveSnapshot(string ClipId, double StartTime, string TrackId);

    public static IReadOnlyList<ClipMoveSnapshot> Capture(IEnumerable<TimelineClipItem> clips) =>
        clips.Select(c => new ClipMoveSnapshot(c.Id, c.StartTime, c.TrackId)).ToList();
}
