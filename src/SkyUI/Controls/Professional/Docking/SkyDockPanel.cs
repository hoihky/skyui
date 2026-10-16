using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace SkyUI.Controls.Professional;

/// <summary>Hosts tool panes for a single <see cref="SkyDockZone"/> with tabs and resize chrome.</summary>
public class SkyDockPanel : TemplatedControl
{
    public const string TabStripPartName = "PART_TabStrip";
    public const string ContentHostPartName = "PART_ContentHost";
    public const string ResizeGripPartName = "PART_ResizeGrip";

    public static readonly StyledProperty<SkyDockZone> ZoneProperty =
        AvaloniaProperty.Register<SkyDockPanel, SkyDockZone>(nameof(Zone));

    public static readonly StyledProperty<IEnumerable?> PanesSourceProperty =
        AvaloniaProperty.Register<SkyDockPanel, IEnumerable?>(nameof(PanesSource));

    public static readonly StyledProperty<double> PaneLengthProperty =
        AvaloniaProperty.Register<SkyDockPanel, double>(nameof(PaneLength), 260);

    public static readonly StyledProperty<double> MinPaneLengthProperty =
        AvaloniaProperty.Register<SkyDockPanel, double>(nameof(MinPaneLength), 120);

    public static readonly StyledProperty<double> MaxPaneLengthProperty =
        AvaloniaProperty.Register<SkyDockPanel, double>(nameof(MaxPaneLength), 640);

    public event EventHandler<double>? PaneLengthChangeRequested;

    private readonly SkyDockManagerController zoneController = new();
    private Panel? tabStrip;
    private ContentPresenter? contentHost;
    private SkyDockResizeGrip? resizeGrip;
    private INotifyCollectionChanged? collectionSubscription;
    private readonly HashSet<SkyDockPane> subscribedPanes = new();
    private readonly SkyDockPaneTabDragHandler tabDragHandler;

    public SkyDockPanel()
    {
        tabDragHandler = new SkyDockPaneTabDragHandler(this);
        Classes.Add("sky-dock-panel");
    }

    internal SkyDockManager? Host { get; set; }

    public SkyDockZone Zone
    {
        get => GetValue(ZoneProperty);
        set => SetValue(ZoneProperty, value);
    }

    public IEnumerable? PanesSource
    {
        get => GetValue(PanesSourceProperty);
        set => SetValue(PanesSourceProperty, value);
    }

    public double PaneLength
    {
        get => GetValue(PaneLengthProperty);
        set => SetValue(PaneLengthProperty, value);
    }

    public double MinPaneLength
    {
        get => GetValue(MinPaneLengthProperty);
        set => SetValue(MinPaneLengthProperty, value);
    }

    public double MaxPaneLength
    {
        get => GetValue(MaxPaneLengthProperty);
        set => SetValue(MaxPaneLengthProperty, value);
    }

    static SkyDockPanel()
    {
        PanesSourceProperty.Changed.AddClassHandler<SkyDockPanel>((p, _) => p.OnPanesSourceChanged());
        ZoneProperty.Changed.AddClassHandler<SkyDockPanel>((p, _) =>
        {
            if (p.resizeGrip is not null)
                p.ConfigureResizeGrip(p.resizeGrip);
            p.Rebuild();
        });
        PaneLengthProperty.Changed.AddClassHandler<SkyDockPanel>((p, _) => p.ApplyLength());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        tabStrip = e.NameScope.Find<Panel>(TabStripPartName);
        contentHost = e.NameScope.Find<ContentPresenter>(ContentHostPartName);
        resizeGrip = e.NameScope.Find<SkyDockResizeGrip>(ResizeGripPartName);
        if (resizeGrip is not null)
        {
            resizeGrip.ResizeDelta += OnResizeDelta;
            ConfigureResizeGrip(resizeGrip);
        }

        OnPanesSourceChanged();
        ApplyLength();
    }

    private void ConfigureResizeGrip(SkyDockResizeGrip grip)
    {
        switch (Zone)
        {
            case SkyDockZone.Left:
                grip.Orientation = SkyDockResizeOrientation.Horizontal;
                grip.HorizontalAlignment = HorizontalAlignment.Right;
                grip.VerticalAlignment = VerticalAlignment.Stretch;
                grip.Width = 6;
                break;
            case SkyDockZone.Right:
                grip.Orientation = SkyDockResizeOrientation.Horizontal;
                grip.HorizontalAlignment = HorizontalAlignment.Left;
                grip.VerticalAlignment = VerticalAlignment.Stretch;
                grip.Width = 6;
                break;
            case SkyDockZone.Bottom:
                grip.Orientation = SkyDockResizeOrientation.Vertical;
                grip.HorizontalAlignment = HorizontalAlignment.Stretch;
                grip.VerticalAlignment = VerticalAlignment.Top;
                grip.Height = 6;
                break;
        }
    }

    internal void SelectPane(SkyDockPane pane) => zoneController.SelectPane(pane);

    internal void InvalidateChrome() => Rebuild();

    private void OnPanesSourceChanged()
    {
        UnsubscribeCollection();
        UnsubscribePanes();
        zoneController.ToolPanes = PanesSource;
        SubscribeCollection();
        SubscribePanes();
        zoneController.EnsureSelection(Zone);
        Rebuild();
    }

    private void SubscribeCollection()
    {
        if (PanesSource is INotifyCollectionChanged notify)
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
                if (item is SkyDockPane pane)
                    SubscribePane(pane);
            }
        }

        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is SkyDockPane pane)
                    UnsubscribePane(pane);
            }
        }

        zoneController.EnsureSelection(Zone);
        Rebuild();
    }

    private void SubscribePanes()
    {
        if (PanesSource is null)
            return;
        foreach (var item in PanesSource)
        {
            if (item is SkyDockPane pane)
                SubscribePane(pane);
        }
    }

    private void UnsubscribePanes()
    {
        foreach (var pane in subscribedPanes.ToList())
            UnsubscribePane(pane);
    }

    private void SubscribePane(SkyDockPane pane)
    {
        if (!subscribedPanes.Add(pane))
            return;
        pane.PropertyChanged += OnPanePropertyChanged;
    }

    private void UnsubscribePane(SkyDockPane pane)
    {
        if (!subscribedPanes.Remove(pane))
            return;
        pane.PropertyChanged -= OnPanePropertyChanged;
    }

    private void OnPanePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(SkyDockPane.Title) or nameof(SkyDockPane.IsSelected)
            or nameof(SkyDockPane.IsVisible) or nameof(SkyDockPane.Zone))
            Rebuild();
    }

    private void Rebuild()
    {
        if (tabStrip is null)
            return;

        tabStrip.Children.Clear();
        var panes = zoneController.GetPanesInZone(Zone);
        IsVisible = panes.Count > 0;
        if (panes.Count == 0)
        {
            SkyDockContentHostHelper.Assign(contentHost, null);
            return;
        }

        foreach (var pane in panes)
            tabStrip.Children.Add(CreateTabButton(pane));

        var selected = zoneController.ResolveSelectedPane(Zone);
        SkyDockContentHostHelper.Assign(contentHost, selected?.Content);
    }

    private Control CreateTabButton(SkyDockPane pane)
    {
        var title = new TextBlock
        {
            Text = pane.Title,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = 140,
        };
        var tab = new Border
        {
            Classes = { "sky-dock-tab" },
            Padding = new Thickness(10, 5),
            Child = title,
        };
        if (pane.IsSelected)
            tab.Classes.Add("selected");
        tabDragHandler.Attach(tab, pane);
        return tab;
    }

    private void OnResizeDelta(object? sender, double delta)
    {
        var sign = Zone switch
        {
            SkyDockZone.Left => 1,
            SkyDockZone.Right => -1,
            SkyDockZone.Bottom => 1,
            _ => 0,
        };
        if (sign == 0)
            return;
        var next = Math.Clamp(PaneLength + delta * sign, MinPaneLength, MaxPaneLength);
        PaneLengthChangeRequested?.Invoke(this, next);
    }

    private void ApplyLength()
    {
        if (Zone is SkyDockZone.Left or SkyDockZone.Right)
        {
            Width = PaneLength;
            MinWidth = MinPaneLength;
            MaxWidth = MaxPaneLength;
            Height = double.NaN;
        }
        else if (Zone == SkyDockZone.Bottom)
        {
            Height = PaneLength;
            MinHeight = MinPaneLength;
            MaxHeight = MaxPaneLength;
            Width = double.NaN;
        }
    }
}
