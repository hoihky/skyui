using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace SkyUI.Controls.Professional;

/// <summary>Tabbed document host with dirty indicators, close affordances, and optional tab reorder.</summary>
public class SkyDocumentWell : TemplatedControl
{
    public const string TabStripPartName = "PART_TabStrip";
    public const string ContentHostPartName = "PART_ContentHost";

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<SkyDocumentWell, IEnumerable?>(nameof(ItemsSource));

    public static readonly StyledProperty<ISkyDocument?> SelectedDocumentProperty =
        AvaloniaProperty.Register<SkyDocumentWell, ISkyDocument?>(nameof(SelectedDocument));

    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<SkyDocumentWell, int>(nameof(SelectedIndex), -1);

    public static readonly StyledProperty<bool> AllowTabReorderProperty =
        AvaloniaProperty.Register<SkyDocumentWell, bool>(nameof(AllowTabReorder), true);

    public static readonly StyledProperty<SkyDocumentShellPlacement> ShellPlacementFilterProperty =
        AvaloniaProperty.Register<SkyDocumentWell, SkyDocumentShellPlacement>(
            nameof(ShellPlacementFilter),
            SkyDocumentShellPlacement.Central);

    public static readonly StyledProperty<bool> ShowCloseButtonProperty =
        AvaloniaProperty.Register<SkyDocumentWell, bool>(nameof(ShowCloseButton), true);

    public static readonly StyledProperty<string> DirtySuffixProperty =
        AvaloniaProperty.Register<SkyDocumentWell, string>(nameof(DirtySuffix), " •");

    public static readonly StyledProperty<ISkyDocumentTabTitleFormatter> TabTitleFormatterProperty =
        AvaloniaProperty.Register<SkyDocumentWell, ISkyDocumentTabTitleFormatter>(
            nameof(TabTitleFormatter),
            new DefaultSkyDocumentTabTitleFormatter());

    public static readonly RoutedEvent<SkyDocumentClosingEventArgs> DocumentClosingEvent =
        RoutedEvent.Register<SkyDocumentWell, SkyDocumentClosingEventArgs>(
            nameof(DocumentClosing),
            RoutingStrategies.Bubble);

    public static readonly RoutedEvent<SkyDocumentClosedEventArgs> DocumentClosedEvent =
        RoutedEvent.Register<SkyDocumentWell, SkyDocumentClosedEventArgs>(
            nameof(DocumentClosed),
            RoutingStrategies.Bubble);

    public static readonly RoutedEvent<SkyDocumentActivatedEventArgs> DocumentActivatedEvent =
        RoutedEvent.Register<SkyDocumentWell, SkyDocumentActivatedEventArgs>(
            nameof(DocumentActivated),
            RoutingStrategies.Bubble);

    public static readonly RoutedEvent<SkyDocumentReorderedEventArgs> DocumentReorderedEvent =
        RoutedEvent.Register<SkyDocumentWell, SkyDocumentReorderedEventArgs>(
            nameof(DocumentReordered),
            RoutingStrategies.Bubble);

    private readonly SkyDocumentWellController controller = new();
    private readonly SkyDocumentWellTabReorderHandler reorderHandler;
    private readonly HashSet<ISkyDocument> subscribedDocuments = new();

    private Panel? tabStrip;
    private ContentPresenter? contentHost;
    private INotifyCollectionChanged? collectionSubscription;

    public SkyDocumentWell()
    {
        reorderHandler = new SkyDocumentWellTabReorderHandler(this);
        Classes.Add("sky-document-well");
    }

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public ISkyDocument? SelectedDocument
    {
        get => GetValue(SelectedDocumentProperty);
        set => SetValue(SelectedDocumentProperty, value);
    }

    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public bool AllowTabReorder
    {
        get => GetValue(AllowTabReorderProperty);
        set => SetValue(AllowTabReorderProperty, value);
    }

    public SkyDocumentShellPlacement ShellPlacementFilter
    {
        get => GetValue(ShellPlacementFilterProperty);
        set => SetValue(ShellPlacementFilterProperty, value);
    }

    public bool ShowCloseButton
    {
        get => GetValue(ShowCloseButtonProperty);
        set => SetValue(ShowCloseButtonProperty, value);
    }

    public string DirtySuffix
    {
        get => GetValue(DirtySuffixProperty);
        set => SetValue(DirtySuffixProperty, value);
    }

    public ISkyDocumentTabTitleFormatter TabTitleFormatter
    {
        get => GetValue(TabTitleFormatterProperty);
        set => SetValue(TabTitleFormatterProperty, value);
    }

    public SkyDocumentWellController Controller => controller;

    public event EventHandler<SkyDocumentClosingEventArgs>? DocumentClosing
    {
        add => AddHandler(DocumentClosingEvent, value);
        remove => RemoveHandler(DocumentClosingEvent, value);
    }

    public event EventHandler<SkyDocumentClosedEventArgs>? DocumentClosed
    {
        add => AddHandler(DocumentClosedEvent, value);
        remove => RemoveHandler(DocumentClosedEvent, value);
    }

    public event EventHandler<SkyDocumentActivatedEventArgs>? DocumentActivated
    {
        add => AddHandler(DocumentActivatedEvent, value);
        remove => RemoveHandler(DocumentActivatedEvent, value);
    }

    public event EventHandler<SkyDocumentReorderedEventArgs>? DocumentReordered
    {
        add => AddHandler(DocumentReorderedEvent, value);
        remove => RemoveHandler(DocumentReorderedEvent, value);
    }

    static SkyDocumentWell()
    {
        ItemsSourceProperty.Changed.AddClassHandler<SkyDocumentWell>((w, _) => w.OnItemsSourceChanged());
        SelectedDocumentProperty.Changed.AddClassHandler<SkyDocumentWell>((w, _) => w.SyncSelectionFromDocument());
        SelectedIndexProperty.Changed.AddClassHandler<SkyDocumentWell>((w, _) => w.SyncSelectionFromIndex());
        DirtySuffixProperty.Changed.AddClassHandler<SkyDocumentWell>((w, _) => w.RebuildTabs());
        TabTitleFormatterProperty.Changed.AddClassHandler<SkyDocumentWell>((w, _) => w.RebuildTabs());
        ShowCloseButtonProperty.Changed.AddClassHandler<SkyDocumentWell>((w, _) => w.RebuildTabs());
        AllowTabReorderProperty.Changed.AddClassHandler<SkyDocumentWell>((w, _) => w.RebuildTabs());
        ShellPlacementFilterProperty.Changed.AddClassHandler<SkyDocumentWell>((w, _) => w.OnShellPlacementFilterChanged());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        tabStrip = e.NameScope.Find<Panel>(TabStripPartName);
        contentHost = e.NameScope.Find<ContentPresenter>(ContentHostPartName);
        OnItemsSourceChanged();
    }

    public void OpenDocument(ISkyDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (!controller.Contains(document))
            controller.TryAdd(document);
        if (ShellPlacementFilter == SkyDocumentShellPlacement.Central
            && document.ShellPlacement != SkyDocumentShellPlacement.Central)
            document.ShellPlacement = SkyDocumentShellPlacement.Central;
        ActivateDocument(document);
        RebuildTabs();
    }

    public void ActivateDocument(ISkyDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var index = controller.IndexOf(document);
        if (index < 0)
            return;

        SetCurrentValue(SelectedIndexProperty, index);
        SetCurrentValue(SelectedDocumentProperty, document);
        SyncContent();
        RebuildTabs();
        RaiseEvent(new SkyDocumentActivatedEventArgs(document, index, DocumentActivatedEvent));
    }

    public bool CloseDocument(ISkyDocument document) => TryCloseDocument(document);

    public bool TryCloseDocument(ISkyDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (!document.CanClose)
            return false;

        var closing = new SkyDocumentClosingEventArgs(document, DocumentClosingEvent);
        RaiseEvent(closing);
        if (closing.Cancel)
            return false;

        var index = controller.IndexOf(document);
        if (!controller.TryRemove(document))
            return false;

        UnsubscribeDocument(document);
        RaiseEvent(new SkyDocumentClosedEventArgs(document, DocumentClosedEvent));

        var remaining = controller.Documents;
        if (remaining.Count == 0)
        {
            SetCurrentValue(SelectedDocumentProperty, null);
            SetCurrentValue(SelectedIndexProperty, -1);
        }
        else if (index >= remaining.Count)
            SetCurrentValue(SelectedIndexProperty, remaining.Count - 1);
        else
            SetCurrentValue(SelectedIndexProperty, index);

        var selected = controller.ResolveSelection(SelectedIndex, SelectedDocument);
        SetCurrentValue(SelectedDocumentProperty, selected);
        RebuildTabs();
        SyncContent();
        return true;
    }

    public void CloseAllDocuments()
    {
        foreach (var document in controller.Documents.ToList())
        {
            if (!TryCloseDocument(document))
                break;
        }
    }

    internal void MoveDocument(int oldIndex, int newIndex)
    {
        var document = controller.GetDocumentAt(oldIndex);
        if (document is null)
            return;
        if (!controller.TryMove(oldIndex, newIndex))
            return;

        SetCurrentValue(SelectedIndexProperty, newIndex);
        SetCurrentValue(SelectedDocumentProperty, document);
        RaiseEvent(new SkyDocumentReorderedEventArgs(document, oldIndex, newIndex, DocumentReorderedEvent));
        RebuildTabs();
    }

    internal int HitTestTabIndex(Point point)
    {
        if (tabStrip is null)
            return -1;

        for (var i = 0; i < tabStrip.Children.Count; i++)
        {
            if (tabStrip.Children[i] is not Control tab)
                continue;
            var bounds = tab.Bounds;
            var topLeft = tab.TranslatePoint(new Point(0, 0), this);
            if (topLeft is null)
                continue;
            var rect = new Rect(topLeft.Value, bounds.Size);
            if (rect.Contains(point))
                return i;
        }

        return -1;
    }

    private void OnShellPlacementFilterChanged()
    {
        controller.PlacementFilter = ShellPlacementFilter;
        CoerceSelection();
        RebuildTabs();
        SyncContent();
    }

    private void OnItemsSourceChanged()
    {
        UnsubscribeCollection();
        UnsubscribeAllDocuments();
        controller.ItemsSource = ItemsSource;
        controller.PlacementFilter = ShellPlacementFilter;
        SubscribeCollection();
        SubscribeAllDocuments();
        CoerceSelection();
        RebuildTabs();
        SyncContent();
    }

    private void SubscribeCollection()
    {
        if (ItemsSource is INotifyCollectionChanged notify)
        {
            collectionSubscription = notify;
            collectionSubscription.CollectionChanged += OnCollectionChanged;
        }
    }

    private void UnsubscribeCollection()
    {
        if (collectionSubscription is null)
            return;
        collectionSubscription.CollectionChanged -= OnCollectionChanged;
        collectionSubscription = null;
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (var item in e.NewItems)
            {
                if (SkyDocumentWellDocumentAccessor.TryAsDocument(item, out var document))
                    SubscribeDocument(document);
            }
        }

        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems)
            {
                if (SkyDocumentWellDocumentAccessor.TryAsDocument(item, out var document))
                    UnsubscribeDocument(document);
            }
        }

        CoerceSelection();
        RebuildTabs();
        SyncContent();
    }

    private void SubscribeAllDocuments()
    {
        if (ItemsSource is null)
            return;
        foreach (var item in ItemsSource)
        {
            if (SkyDocumentWellDocumentAccessor.TryAsDocument(item, out var document))
                SubscribeDocument(document);
        }
    }

    private void UnsubscribeAllDocuments()
    {
        foreach (var document in subscribedDocuments.ToList())
            UnsubscribeDocument(document);
    }

    private void SubscribeDocument(ISkyDocument document)
    {
        if (!subscribedDocuments.Add(document))
            return;
        if (document is INotifyPropertyChanged notify)
            notify.PropertyChanged += OnDocumentPropertyChanged;
    }

    private void UnsubscribeDocument(ISkyDocument document)
    {
        if (!subscribedDocuments.Remove(document))
            return;
        if (document is INotifyPropertyChanged notify)
            notify.PropertyChanged -= OnDocumentPropertyChanged;
    }

    private void OnDocumentPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ISkyDocument.Title) or nameof(ISkyDocument.IsDirty) or nameof(ISkyDocument.CanClose)
            or nameof(ISkyDocument.ShellPlacement))
        {
            RebuildTabs();
            if (e.PropertyName is nameof(ISkyDocument.ShellPlacement))
                CoerceSelection();
        }
        if (e.PropertyName is nameof(ISkyDocument.Content) && sender is ISkyDocument document
            && ReferenceEquals(SelectedDocument, document))
            SyncContent();
    }

    private void SyncSelectionFromDocument()
    {
        if (SelectedDocument is null)
            return;
        var index = controller.IndexOf(SelectedDocument);
        if (index >= 0 && index != SelectedIndex)
            SetCurrentValue(SelectedIndexProperty, index);
        SyncContent();
        RebuildTabs();
    }

    private void SyncSelectionFromIndex()
    {
        var document = controller.ResolveSelection(SelectedIndex, SelectedDocument);
        if (!ReferenceEquals(document, SelectedDocument))
            SetCurrentValue(SelectedDocumentProperty, document);
        SyncContent();
        RebuildTabs();
    }

    private void CoerceSelection()
    {
        var index = controller.ClampSelectionIndex(SelectedIndex, SelectedDocument);
        if (index != SelectedIndex)
            SetCurrentValue(SelectedIndexProperty, index);
        var document = controller.ResolveSelection(index, SelectedDocument);
        if (!ReferenceEquals(document, SelectedDocument))
            SetCurrentValue(SelectedDocumentProperty, document);
    }

    private void SyncContent()
    {
        if (contentHost is null)
            return;
        SkyDockContentHostHelper.Assign(contentHost, SelectedDocument?.Content);
    }

    private void RebuildTabs()
    {
        if (tabStrip is null)
            return;

        tabStrip.Children.Clear();
        var formatter = TabTitleFormatter ?? new DefaultSkyDocumentTabTitleFormatter();
        var suffix = DirtySuffix ?? string.Empty;
        var documents = controller.Documents;
        for (var i = 0; i < documents.Count; i++)
        {
            var document = documents[i];
            var isSelected = ReferenceEquals(document, SelectedDocument);
            tabStrip.Children.Add(CreateTab(document, i, isSelected, formatter.Format(document, suffix)));
        }
    }

    private Control CreateTab(ISkyDocument document, int index, bool isSelected, string headerText)
    {
        var title = new TextBlock
        {
            Text = headerText,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = 160,
        };

        var header = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children = { title },
        };

        if (ShowCloseButton && document.CanClose)
        {
            var close = new Button
            {
                Classes = { "sky", "sky-subtle", "sky-document-tab-close" },
                Content = "×",
                Padding = new Thickness(4, 0),
                MinWidth = 24,
                MinHeight = 24,
                VerticalAlignment = VerticalAlignment.Center,
            };
            close.Click += (_, _) => TryCloseDocument(document);
            header.Children.Add(close);
        }

        var tab = new Border
        {
            Classes = { "sky-document-tab" },
            Padding = new Thickness(10, 6),
            Child = header,
        };

        if (isSelected)
            tab.Classes.Add("selected");

        reorderHandler.AttachTab(tab, document, index);
        return tab;
    }
}
