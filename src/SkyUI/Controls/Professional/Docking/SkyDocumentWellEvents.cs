using Avalonia.Interactivity;

namespace SkyUI.Controls.Professional;

public sealed class SkyDocumentClosingEventArgs : RoutedEventArgs
{
    public SkyDocumentClosingEventArgs(ISkyDocument document, RoutedEvent routedEvent)
        : base(routedEvent)
    {
        Document = document;
    }

    public ISkyDocument Document { get; }

    public bool Cancel { get; set; }
}

public sealed class SkyDocumentClosedEventArgs : RoutedEventArgs
{
    public SkyDocumentClosedEventArgs(ISkyDocument document, RoutedEvent routedEvent)
        : base(routedEvent)
    {
        Document = document;
    }

    public ISkyDocument Document { get; }
}

public sealed class SkyDocumentActivatedEventArgs : RoutedEventArgs
{
    public SkyDocumentActivatedEventArgs(ISkyDocument document, int index, RoutedEvent routedEvent)
        : base(routedEvent)
    {
        Document = document;
        Index = index;
    }

    public ISkyDocument Document { get; }

    public int Index { get; }
}

public sealed class SkyDocumentReorderedEventArgs : RoutedEventArgs
{
    public SkyDocumentReorderedEventArgs(ISkyDocument document, int oldIndex, int newIndex, RoutedEvent routedEvent)
        : base(routedEvent)
    {
        Document = document;
        OldIndex = oldIndex;
        NewIndex = newIndex;
    }

    public ISkyDocument Document { get; }

    public int OldIndex { get; }

    public int NewIndex { get; }
}
