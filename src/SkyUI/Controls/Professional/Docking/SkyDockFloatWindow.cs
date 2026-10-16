using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace SkyUI.Controls.Professional;

/// <summary>Floating tool window chrome (title bar drag, dock back, close).</summary>
public class SkyDockFloatWindow : TemplatedControl
{
    public const string TitleBarPartName = "PART_TitleBar";
    public const string TitleTextPartName = "PART_TitleText";
    public const string DockButtonPartName = "PART_Dock";
    public const string CloseButtonPartName = "PART_Close";
    public const string ContentHostPartName = "PART_ContentHost";

    public static readonly StyledProperty<SkyDockPane?> PaneProperty =
        AvaloniaProperty.Register<SkyDockFloatWindow, SkyDockPane?>(nameof(Pane));

    public static readonly StyledProperty<ISkyDocument?> DocumentProperty =
        AvaloniaProperty.Register<SkyDockFloatWindow, ISkyDocument?>(nameof(Document));

    public static readonly RoutedEvent<RoutedEventArgs> DockBackRequestedEvent =
        RoutedEvent.Register<SkyDockFloatWindow, RoutedEventArgs>(nameof(DockBackRequested), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<RoutedEventArgs> CloseRequestedEvent =
        RoutedEvent.Register<SkyDockFloatWindow, RoutedEventArgs>(nameof(CloseRequested), RoutingStrategies.Bubble);

    public event EventHandler<Point>? PositionDragDelta;
    public event EventHandler? DragCompleted;

    private const double DockDragThreshold = 8;

    private Control? titleBar;
    private TextBlock? titleText;
    private ContentPresenter? contentHost;
    private bool isDragging;
    private bool isDockDragging;
    private Point dragStart;
    private Point pressPoint;

    public SkyDockFloatWindow()
    {
        Classes.Add("sky-dock-float-window");
    }

    public SkyDockPane? Pane
    {
        get => GetValue(PaneProperty);
        set => SetValue(PaneProperty, value);
    }

    public ISkyDocument? Document
    {
        get => GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    public event EventHandler<RoutedEventArgs>? DockBackRequested
    {
        add => AddHandler(DockBackRequestedEvent, value);
        remove => RemoveHandler(DockBackRequestedEvent, value);
    }

    public event EventHandler<RoutedEventArgs>? CloseRequested
    {
        add => AddHandler(CloseRequestedEvent, value);
        remove => RemoveHandler(CloseRequestedEvent, value);
    }

    static SkyDockFloatWindow()
    {
        PaneProperty.Changed.AddClassHandler<SkyDockFloatWindow>((w, _) => w.SyncChrome());
        DocumentProperty.Changed.AddClassHandler<SkyDockFloatWindow>((w, _) => w.SyncChrome());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        titleBar = e.NameScope.Find<Control>(TitleBarPartName);
        var dock = e.NameScope.Find<Button>(DockButtonPartName);
        var close = e.NameScope.Find<Button>(CloseButtonPartName);
        contentHost = e.NameScope.Find<ContentPresenter>(ContentHostPartName);
        titleText = e.NameScope.Find<TextBlock>(TitleTextPartName);

        if (titleBar is not null)
        {
            titleBar.PointerPressed += OnTitleBarPointerPressed;
            titleBar.PointerMoved += OnTitleBarPointerMoved;
            titleBar.PointerReleased += OnTitleBarPointerReleased;
            titleBar.PointerCaptureLost += OnTitleBarCaptureLost;
        }

        if (dock is not null)
            dock.Click += (_, _) => RaiseEvent(new RoutedEventArgs(DockBackRequestedEvent));
        if (close is not null)
            close.Click += (_, _) => RaiseEvent(new RoutedEventArgs(CloseRequestedEvent));

        SyncChrome();
    }

    private void SyncChrome()
    {
        if (Document is not null)
        {
            if (titleText is not null)
                titleText.Text = Document.Title;
            SkyDockContentHostHelper.Assign(contentHost, Document.Content);
            return;
        }

        if (Pane is null)
            return;
        Width = Pane.FloatWidth;
        Height = Pane.FloatHeight;
        if (titleText is not null)
            titleText.Text = Pane.Title;
        SkyDockContentHostHelper.Assign(contentHost, Pane.Content);
    }

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(titleBar).Properties.IsLeftButtonPressed)
            return;
        isDragging = true;
        isDockDragging = false;
        dragStart = e.GetPosition(this);
        var manager = FindDockManager();
        pressPoint = manager is not null ? e.GetPosition(manager) : dragStart;
        e.Pointer.Capture(titleBar);
        e.Handled = true;
    }

    private void OnTitleBarPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!isDragging)
            return;

        var manager = FindDockManager();
        if (Pane is not null && manager is not null && manager.AllowDockDrag)
        {
            var pointInManager = e.GetPosition(manager);
            if (!isDockDragging)
            {
                if (Math.Abs(pointInManager.X - pressPoint.X) < DockDragThreshold
                    && Math.Abs(pointInManager.Y - pressPoint.Y) < DockDragThreshold)
                    return;
                isDockDragging = true;
                isDragging = false;
                manager.BeginDockDrag(Pane);
                manager.UpdateDockDrag(pointInManager);
                e.Pointer.Capture(manager);
                e.Handled = true;
                return;
            }

            manager.UpdateDockDrag(pointInManager);
            e.Handled = true;
            return;
        }

        var point = e.GetPosition(this);
        var delta = point - dragStart;
        dragStart = point;
        PositionDragDelta?.Invoke(this, delta);
        e.Handled = true;
    }

    private void OnTitleBarPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (isDockDragging)
        {
            isDockDragging = false;
            isDragging = false;
            return;
        }

        EndDrag(e.Pointer);
    }

    private void OnTitleBarCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (isDockDragging && FindDockManager() is SkyDockManager manager
            && ReferenceEquals(e.Pointer.Captured, manager))
            return;
        EndDrag(e.Pointer);
    }

    private SkyDockManager? FindDockManager() => this.FindAncestorOfType<SkyDockManager>();

    private void EndDrag(IPointer pointer)
    {
        if (!isDragging)
            return;
        isDragging = false;
        pointer.Capture(null);
        DragCompleted?.Invoke(this, EventArgs.Empty);
    }
}
