namespace SkyUI.Controls;

/// <summary>Undo/redo stack for <see cref="Timeline.Commands.ITimelineEditCommand"/> edits.</summary>
public sealed class TimelineUndoStack
{
    private readonly Stack<Timeline.Commands.ITimelineEditCommand> undoStack = new();
    private readonly Stack<Timeline.Commands.ITimelineEditCommand> redoStack = new();

    public event EventHandler? StateChanged;

    public bool CanUndo => undoStack.Count > 0;

    public bool CanRedo => redoStack.Count > 0;

    public void Execute(Timeline.Commands.ITimelineEditCommand command)
    {
        command.Execute();
        undoStack.Push(command);
        redoStack.Clear();
        RaiseStateChanged();
    }

    public void Undo()
    {
        if (!CanUndo)
            return;
        var command = undoStack.Pop();
        command.Undo();
        redoStack.Push(command);
        RaiseStateChanged();
    }

    public void Redo()
    {
        if (!CanRedo)
            return;
        var command = redoStack.Pop();
        command.Execute();
        undoStack.Push(command);
        RaiseStateChanged();
    }

    public void Clear()
    {
        undoStack.Clear();
        redoStack.Clear();
        RaiseStateChanged();
    }

    private void RaiseStateChanged() => StateChanged?.Invoke(this, EventArgs.Empty);
}
