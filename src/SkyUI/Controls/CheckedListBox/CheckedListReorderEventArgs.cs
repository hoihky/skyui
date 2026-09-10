namespace SkyUI.Controls;

public sealed class CheckedListReorderEventArgs : EventArgs
{
    public CheckedListReorderEventArgs(
        object? sourceItem,
        object? targetItem,
        CheckedListDropPosition position,
        object? sharedParent = null)
    {
        SourceItem = sourceItem;
        TargetItem = targetItem;
        Position = position;
        SharedParent = sharedParent;
    }

    public object? SourceItem { get; }

    public object? TargetItem { get; }

    public CheckedListDropPosition Position { get; }

    public object? SharedParent { get; }

    public bool Handled { get; set; }
}
