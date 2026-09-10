namespace SkyUI.Controls;

public sealed class CheckedListEditStartingEventArgs : EventArgs
{
    public CheckedListEditStartingEventArgs(object? item, string text)
    {
        Item = item;
        Text = text;
    }

    public object? Item { get; }

    public string Text { get; }

    public bool Cancel { get; set; }
}

public sealed class CheckedListEditCommittedEventArgs : EventArgs
{
    public CheckedListEditCommittedEventArgs(object? item, string text)
    {
        Item = item;
        Text = text;
    }

    public object? Item { get; }

    public string Text { get; }
}

public sealed class CheckedListEditCancelledEventArgs : EventArgs
{
    public CheckedListEditCancelledEventArgs(object? item, string originalText)
    {
        Item = item;
        OriginalText = originalText;
    }

    public object? Item { get; }

    public string OriginalText { get; }
}
