namespace SkyUI.Controls.Timeline.Commands;

/// <summary>Undoable timeline edit (command pattern).</summary>
public interface ITimelineEditCommand
{
    string Description { get; }

    void Execute();

    void Undo();
}
