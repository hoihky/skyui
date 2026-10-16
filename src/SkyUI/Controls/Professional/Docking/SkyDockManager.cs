using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace SkyUI.Controls.Professional;

/// <summary>Root docking host: document well plus tool/float panes with layout persistence hooks.</summary>
[PseudoClasses("dock-dragging")]
public class SkyDockManager : TemplatedControl
{
    public const string DocumentWellPartName = "PART_DocumentWell";
    public const string LeftDocumentZonePartName = "PART_LeftDocumentZone";
    public const string RightDocumentZonePartName = "PART_RightDocumentZone";
    public const string BottomDocumentZonePartName = "PART_BottomDocumentZone";
    public const string LeftPanelPartName = "PART_LeftPanel";
    public const string RightPanelPartName = "PART_RightPanel";
    public const string BottomPanelPartName = "PART_BottomPanel";
    public const string RootPartName = "PART_Root";
    public const string FloatCanvasPartName = "PART_FloatCanvas";
    public const string DropOverlayPartName = "PART_DropOverlay";
    public const string DropLeftPartName = "PART_DropLeft";
    public const string DropRightPartName = "PART_DropRight";
    public const string DropBottomPartName = "PART_DropBottom";
    public const string DropFloatPartName = "PART_DropFloat";

    public static readonly StyledProperty<IEnumerable?> DocumentsProperty =
        AvaloniaProperty.Register<SkyDockManager, IEnumerable?>(nameof(Documents));

    public static readonly StyledProperty<ISkyDocument?> SelectedDocumentProperty =
        AvaloniaProperty.Register<SkyDockManager, ISkyDocument?>(nameof(SelectedDocument));

    public static readonly StyledProperty<IEnumerable?> ToolPanesProperty =
        AvaloniaProperty.Register<SkyDockManager, IEnumerable?>(nameof(ToolPanes));

    public static readonly StyledProperty<double> LeftToolPaneWidthProperty =
        AvaloniaProperty.Register<SkyDockManager, double>(nameof(LeftToolPaneWidth), 260);

    public static readonly StyledProperty<double> RightToolPaneWidthProperty =
        AvaloniaProperty.Register<SkyDockManager, double>(nameof(RightToolPaneWidth), 280);

    public static readonly StyledProperty<double> BottomToolPaneHeightProperty =
        AvaloniaProperty.Register<SkyDockManager, double>(nameof(BottomToolPaneHeight), 200);

    public static readonly StyledProperty<bool> AllowDockDragProperty =
        AvaloniaProperty.Register<SkyDockManager, bool>(nameof(AllowDockDrag), true);

    private const double DefaultDocumentFloatWidth = 520;
    private const double DefaultDocumentFloatHeight = 360;

    private readonly SkyDockManagerController controller = new();
    private readonly SkyDocumentShellController documentShellController = new();
    private readonly HashSet<ISkyDocument> subscribedDocuments = new();
    private SkyDocumentWell? documentWell;
    private SkyDocumentWell? leftDocumentZone;
    private SkyDocumentWell? rightDocumentZone;
    private SkyDocumentWell? bottomDocumentZone;
    private SkyDockPanel? leftPanel;
    private SkyDockPanel? rightPanel;
    private SkyDockPanel? bottomPanel;
    private Grid? rootGrid;
    private Canvas? floatCanvas;
    private Panel? dropOverlay;
    private Border? dropLeft;
    private Border? dropRight;
    private Border? dropBottom;
    private Border? dropFloat;
    private INotifyCollectionChanged? toolPaneSubscription;
    private INotifyCollectionChanged? documentSubscription;
    private SkyDockPane? draggingPane;
    private ISkyDocument? draggingDocument;
    private SkyDockZone? activeDropZone;

    public SkyDockManager()
    {
        Classes.Add("sky-dock-manager");
    }

    public IEnumerable? Documents
    {
        get => GetValue(DocumentsProperty);
        set => SetValue(DocumentsProperty, value);
    }

    public ISkyDocument? SelectedDocument
    {
        get => GetValue(SelectedDocumentProperty);
        set => SetValue(SelectedDocumentProperty, value);
    }

    public IEnumerable? ToolPanes
    {
        get => GetValue(ToolPanesProperty);
        set => SetValue(ToolPanesProperty, value);
    }

    public double LeftToolPaneWidth
    {
        get => GetValue(LeftToolPaneWidthProperty);
        set => SetValue(LeftToolPaneWidthProperty, value);
    }

    public double RightToolPaneWidth
    {
        get => GetValue(RightToolPaneWidthProperty);
        set => SetValue(RightToolPaneWidthProperty, value);
    }

    public double BottomToolPaneHeight
    {
        get => GetValue(BottomToolPaneHeightProperty);
        set => SetValue(BottomToolPaneHeightProperty, value);
    }

    public bool AllowDockDrag
    {
        get => GetValue(AllowDockDragProperty);
        set => SetValue(AllowDockDragProperty, value);
    }

    public SkyDocumentWell? DocumentWell => documentWell;

    public SkyDockManagerController Controller => controller;

    static SkyDockManager()
    {
        DocumentsProperty.Changed.AddClassHandler<SkyDockManager>((m, _) => m.OnDocumentsChanged());
        SelectedDocumentProperty.Changed.AddClassHandler<SkyDockManager>((m, _) => m.SyncSelectedDocument());
        ToolPanesProperty.Changed.AddClassHandler<SkyDockManager>((m, _) => m.OnToolPanesChanged());
        LeftToolPaneWidthProperty.Changed.AddClassHandler<SkyDockManager>((m, _) => m.SyncPanelLengths());
        RightToolPaneWidthProperty.Changed.AddClassHandler<SkyDockManager>((m, _) => m.SyncPanelLengths());
        BottomToolPaneHeightProperty.Changed.AddClassHandler<SkyDockManager>((m, _) => m.SyncPanelLengths());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        rootGrid = e.NameScope.Find<Grid>(RootPartName);
        documentWell = e.NameScope.Find<SkyDocumentWell>(DocumentWellPartName);
        leftDocumentZone = e.NameScope.Find<SkyDocumentWell>(LeftDocumentZonePartName);
        rightDocumentZone = e.NameScope.Find<SkyDocumentWell>(RightDocumentZonePartName);
        bottomDocumentZone = e.NameScope.Find<SkyDocumentWell>(BottomDocumentZonePartName);
        leftPanel = e.NameScope.Find<SkyDockPanel>(LeftPanelPartName);
        rightPanel = e.NameScope.Find<SkyDockPanel>(RightPanelPartName);
        bottomPanel = e.NameScope.Find<SkyDockPanel>(BottomPanelPartName);
        floatCanvas = e.NameScope.Find<Canvas>(FloatCanvasPartName);
        dropOverlay = e.NameScope.Find<Panel>(DropOverlayPartName);
        dropLeft = e.NameScope.Find<Border>(DropLeftPartName);
        dropRight = e.NameScope.Find<Border>(DropRightPartName);
        dropBottom = e.NameScope.Find<Border>(DropBottomPartName);
        dropFloat = e.NameScope.Find<Border>(DropFloatPartName);

        ConfigurePanel(leftPanel, SkyDockZone.Left, nameof(LeftToolPaneWidth));
        ConfigurePanel(rightPanel, SkyDockZone.Right, nameof(RightToolPaneWidth));
        ConfigurePanel(bottomPanel, SkyDockZone.Bottom, nameof(BottomToolPaneHeight));

        ConfigureDocumentZone(leftDocumentZone, SkyDocumentShellPlacement.Left);
        ConfigureDocumentZone(rightDocumentZone, SkyDocumentShellPlacement.Right);
        ConfigureDocumentZone(bottomDocumentZone, SkyDocumentShellPlacement.Bottom);
        OnDocumentsChanged();
        SyncSelectedDocument();
        OnToolPanesChanged();
        SyncPanelLengths();
        HideDropOverlay();
    }

    public bool RegisterToolPane(SkyDockPane pane) =>
        controller.TryAddPane(pane, ToolPanes as IList);

    public bool RemoveToolPane(SkyDockPane pane) =>
        controller.TryRemovePane(pane, ToolPanes as IList);

    public void FloatToolPane(SkyDockPane pane, Point? position = null)
    {
        controller.FloatPane(pane, position);
        RefreshAfterLayoutChange();
    }

    public void DockToolPane(SkyDockPane pane, SkyDockZone zone)
    {
        controller.DockPane(pane, zone);
        RefreshAfterLayoutChange();
    }

    public SkyDockLayoutState CaptureLayout()
    {
        var toolLayout = controller.CaptureLayout(LeftToolPaneWidth, RightToolPaneWidth, BottomToolPaneHeight);
        return new SkyDockLayoutState
        {
            LeftToolPaneWidth = toolLayout.LeftToolPaneWidth,
            RightToolPaneWidth = toolLayout.RightToolPaneWidth,
            BottomToolPaneHeight = toolLayout.BottomToolPaneHeight,
            ToolPanes = toolLayout.ToolPanes,
            DocumentShell = SkyDockLayoutDocumentShell.Capture(Documents),
        };
    }

    public string CaptureLayoutJson() => SkyDockLayoutSerializer.ToJson(CaptureLayout());

    public void RestoreLayout(SkyDockLayoutState? state)
    {
        if (state is null)
            return;
        controller.ApplyPaneMetadata(state, ToolPanes as IList);
        SkyDockLayoutDocumentShell.Apply(state.DocumentShell, Documents);
        SetCurrentValue(LeftToolPaneWidthProperty, state.LeftToolPaneWidth);
        SetCurrentValue(RightToolPaneWidthProperty, state.RightToolPaneWidth);
        SetCurrentValue(BottomToolPaneHeightProperty, state.BottomToolPaneHeight);
        RefreshAfterLayoutChange();
    }

    public bool TryRestoreLayoutJson(string? json)
    {
        var state = SkyDockLayoutSerializer.TryFromJson(json);
        if (state is null)
            return false;
        RestoreLayout(state);
        return true;
    }

    internal void BeginDocumentShellDrag(ISkyDocument document)
    {
        if (draggingPane is not null)
            return;
        draggingDocument = document;
        PseudoClasses.Set(":dock-dragging", true);
        if (dropOverlay is not null)
            dropOverlay.IsVisible = true;
        AddHandler(PointerMovedEvent, OnManagerDockPointerMoved, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(PointerReleasedEvent, OnManagerDockPointerReleased, RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    internal void UpdateDocumentShellDrag(Point pointInManager) => UpdateDockDrag(pointInManager);

    internal void CompleteDocumentShellDrag(Point pointInManager)
    {
        if (draggingDocument is null)
        {
            EndDockDrag();
            return;
        }

        var zone = activeDropZone ?? ResolveDropZone(pointInManager) ?? SkyDockZone.Float;
        Point dropPoint = pointInManager;
        if (zone == SkyDockZone.Float)
        {
            var origin = GetFloatCanvasOriginInManager();
            dropPoint = new Point(
                pointInManager.X - origin.X - DefaultDocumentFloatWidth / 2,
                pointInManager.Y - origin.Y - 24);
        }

        documentShellController.ApplyDockZone(draggingDocument, zone, dropPoint);
        RefreshAfterLayoutChange();
        EndDockDrag();
    }

    internal void BeginDockDrag(SkyDockPane pane)
    {
        if (draggingDocument is not null)
            return;
        draggingPane = pane;
        PseudoClasses.Set(":dock-dragging", true);
        if (dropOverlay is not null)
            dropOverlay.IsVisible = true;
        AddHandler(PointerMovedEvent, OnManagerDockPointerMoved, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(PointerReleasedEvent, OnManagerDockPointerReleased, RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    internal void UpdateDockDrag(Point pointInManager)
    {
        activeDropZone = ResolveDropZone(pointInManager);
        UpdateDropHighlights(activeDropZone);
    }

    private SkyDockZone? ResolveDropZone(Point pointInManager)
    {
        var managerBounds = GetManagerBounds();
        var documentBounds = GetDocumentBoundsInManager();
        return SkyDockDropZoneAdvisor.ResolveForDockHost(managerBounds, documentBounds, pointInManager);
    }

    private Rect GetManagerBounds()
    {
        var width = Bounds.Width > 0 ? Bounds.Width : Width;
        var height = Bounds.Height > 0 ? Bounds.Height : Height;
        if (width <= 0)
            width = 800;
        if (height <= 0)
            height = 600;
        return new Rect(0, 0, width, height);
    }

    private Rect GetDocumentBoundsInManager()
    {
        if (documentWell is null)
            return default;
        var topLeft = documentWell.TranslatePoint(new Point(0, 0), this);
        if (topLeft is null)
            return default;
        return new Rect(topLeft.Value, documentWell.Bounds.Size);
    }

    private Point GetFloatCanvasOriginInManager()
    {
        if (floatCanvas is null)
            return default;
        return floatCanvas.TranslatePoint(new Point(0, 0), this) ?? default;
    }

    internal void CompleteDockDrag(Point pointInManager)
    {
        if (draggingPane is null)
        {
            EndDockDrag();
            return;
        }

        var zone = activeDropZone ?? ResolveDropZone(pointInManager) ?? SkyDockZone.Float;
        Point? dropPoint = null;
        if (zone == SkyDockZone.Float)
        {
            var origin = GetFloatCanvasOriginInManager();
            dropPoint = new Point(
                pointInManager.X - origin.X - draggingPane.FloatWidth / 2,
                pointInManager.Y - origin.Y - 24);
        }

        controller.ApplyDrop(draggingPane, zone, dropPoint ?? pointInManager);
        RefreshAfterLayoutChange();
        EndDockDrag();
    }

    internal void CancelDockDrag() => EndDockDrag();

    private void OnManagerDockPointerMoved(object? sender, PointerEventArgs e)
    {
        if (draggingPane is null && draggingDocument is null)
            return;
        UpdateDockDrag(e.GetPosition(this));
        e.Handled = true;
    }

    private void OnManagerDockPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (draggingPane is not null)
            CompleteDockDrag(e.GetPosition(this));
        else if (draggingDocument is not null)
            CompleteDocumentShellDrag(e.GetPosition(this));
        else
            return;
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private void EndDockDrag()
    {
        RemoveHandler(PointerMovedEvent, OnManagerDockPointerMoved);
        RemoveHandler(PointerReleasedEvent, OnManagerDockPointerReleased);
        draggingPane = null;
        draggingDocument = null;
        activeDropZone = null;
        PseudoClasses.Set(":dock-dragging", false);
        HideDropOverlay();
    }

    private void HideDropOverlay()
    {
        if (dropOverlay is not null)
            dropOverlay.IsVisible = false;
        UpdateDropHighlights(null);
    }

    private void UpdateDropHighlights(SkyDockZone? zone)
    {
        SetDropActive(dropLeft, zone == SkyDockZone.Left);
        SetDropActive(dropRight, zone == SkyDockZone.Right);
        SetDropActive(dropBottom, zone == SkyDockZone.Bottom);
        SetDropActive(dropFloat, zone == SkyDockZone.Float);
    }

    private static void SetDropActive(Border? border, bool active)
    {
        if (border is null)
            return;
        if (active)
            border.Classes.Add("active");
        else
            border.Classes.Remove("active");
    }

    private void ConfigurePanel(SkyDockPanel? panel, SkyDockZone zone, string lengthPropertyName)
    {
        if (panel is null)
            return;
        panel.Host = this;
        panel.Zone = zone;
        panel.PanesSource = ToolPanes;
        panel.PaneLengthChangeRequested += (_, length) => SetLengthProperty(lengthPropertyName, length);
    }

    private void SetLengthProperty(string propertyName, double length)
    {
        switch (propertyName)
        {
            case nameof(LeftToolPaneWidth):
                SetCurrentValue(LeftToolPaneWidthProperty, length);
                break;
            case nameof(RightToolPaneWidth):
                SetCurrentValue(RightToolPaneWidthProperty, length);
                break;
            case nameof(BottomToolPaneHeight):
                SetCurrentValue(BottomToolPaneHeightProperty, length);
                break;
        }
    }

    private void ConfigureDocumentZone(SkyDocumentWell? zone, SkyDocumentShellPlacement placement)
    {
        if (zone is null)
            return;
        zone.ShellPlacementFilter = placement;
        zone.ItemsSource = Documents;
        zone.SelectedDocument = SelectedDocument;
    }

    private void OnDocumentsChanged()
    {
        UnsubscribeDocuments();
        if (documentWell is not null)
            documentWell.ItemsSource = Documents;
        if (leftDocumentZone is not null)
            leftDocumentZone.ItemsSource = Documents;
        if (rightDocumentZone is not null)
            rightDocumentZone.ItemsSource = Documents;
        if (bottomDocumentZone is not null)
            bottomDocumentZone.ItemsSource = Documents;
        SubscribeDocuments();
        RefreshAfterLayoutChange();
    }

    private void SubscribeDocuments()
    {
        if (Documents is INotifyCollectionChanged notify)
        {
            documentSubscription = notify;
            documentSubscription.CollectionChanged += OnDocumentCollectionChanged;
        }

        if (Documents is null)
            return;
        foreach (var item in Documents)
        {
            if (SkyDocumentWellDocumentAccessor.TryAsDocument(item, out var document))
                SubscribeDocument(document);
        }
    }

    private void UnsubscribeDocuments()
    {
        if (documentSubscription is not null)
        {
            documentSubscription.CollectionChanged -= OnDocumentCollectionChanged;
            documentSubscription = null;
        }

        foreach (var document in subscribedDocuments.ToList())
            UnsubscribeDocument(document);
    }

    private void OnDocumentCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
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

        RefreshAfterLayoutChange();
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
        if (e.PropertyName is nameof(ISkyDocument.ShellPlacement) or nameof(ISkyDocument.ShellFloatLeft)
            or nameof(ISkyDocument.ShellFloatTop))
            RefreshAfterLayoutChange();
    }

    private void SyncSelectedDocument()
    {
        SyncWellSelection(documentWell);
        SyncWellSelection(leftDocumentZone);
        SyncWellSelection(rightDocumentZone);
        SyncWellSelection(bottomDocumentZone);
    }

    private void SyncWellSelection(SkyDocumentWell? well)
    {
        if (well is null)
            return;
        if (!ReferenceEquals(well.SelectedDocument, SelectedDocument))
            well.SelectedDocument = SelectedDocument;
    }

    private void OnToolPanesChanged()
    {
        UnsubscribeToolPanes();
        controller.ToolPanes = ToolPanes;
        if (ToolPanes is INotifyCollectionChanged notify)
        {
            toolPaneSubscription = notify;
            toolPaneSubscription.CollectionChanged += OnToolPaneCollectionChanged;
        }

        RefreshAfterLayoutChange();
    }

    private void OnToolPaneCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        RefreshAfterLayoutChange();

    private void UnsubscribeToolPanes()
    {
        if (toolPaneSubscription is null)
            return;
        toolPaneSubscription.CollectionChanged -= OnToolPaneCollectionChanged;
        toolPaneSubscription = null;
    }

    private void RefreshAfterLayoutChange()
    {
        RefreshToolPanels();
        RebuildFloatLayer();
        InvalidateDockPanels();
        SyncDocumentZoneVisibility();
        SyncSideColumnVisibility();
    }

    private void SyncSideColumnVisibility()
    {
        if (rootGrid is null || rootGrid.ColumnDefinitions.Count < 3)
            return;
        var showLeft = controller.HasVisibleToolsInZone(SkyDockZone.Left)
            || HasDocumentsInPlacement(SkyDocumentShellPlacement.Left);
        var showRight = controller.HasVisibleToolsInZone(SkyDockZone.Right)
            || HasDocumentsInPlacement(SkyDocumentShellPlacement.Right);
        rootGrid.ColumnDefinitions[0].Width = showLeft ? GridLength.Auto : new GridLength(0);
        rootGrid.ColumnDefinitions[2].Width = showRight ? GridLength.Auto : new GridLength(0);
    }

    private void SyncDocumentZoneVisibility()
    {
        SetDocumentZoneRowVisible(leftDocumentZone, HasDocumentsInPlacement(SkyDocumentShellPlacement.Left));
        SetDocumentZoneRowVisible(rightDocumentZone, HasDocumentsInPlacement(SkyDocumentShellPlacement.Right));
        SetDocumentZoneRowVisible(bottomDocumentZone, HasDocumentsInPlacement(SkyDocumentShellPlacement.Bottom));
    }

    private bool HasDocumentsInPlacement(SkyDocumentShellPlacement placement) =>
        SkyDocumentShellAccessor.GetByPlacement(Documents, placement).Count > 0;

    private static void SetDocumentZoneRowVisible(SkyDocumentWell? zone, bool visible)
    {
        if (zone is null)
            return;
        zone.IsVisible = visible;
    }

    private void InvalidateDockPanels()
    {
        leftPanel?.InvalidateChrome();
        rightPanel?.InvalidateChrome();
        bottomPanel?.InvalidateChrome();
        SyncBottomRowVisibility();
    }

    private void SyncBottomRowVisibility()
    {
        if (rootGrid is null || rootGrid.RowDefinitions.Count < 2)
            return;
        var hasBottom = controller.HasVisibleToolsInZone(SkyDockZone.Bottom)
            || HasDocumentsInPlacement(SkyDocumentShellPlacement.Bottom);
        rootGrid.RowDefinitions[1].Height = hasBottom ? GridLength.Auto : new GridLength(0);
    }

    private void RefreshToolPanels()
    {
        if (leftPanel is not null)
            leftPanel.PanesSource = ToolPanes;
        if (rightPanel is not null)
            rightPanel.PanesSource = ToolPanes;
        if (bottomPanel is not null)
            bottomPanel.PanesSource = ToolPanes;
    }

    private void SyncPanelLengths()
    {
        if (leftPanel is not null)
            leftPanel.PaneLength = LeftToolPaneWidth;
        if (rightPanel is not null)
            rightPanel.PaneLength = RightToolPaneWidth;
        if (bottomPanel is not null)
            bottomPanel.PaneLength = BottomToolPaneHeight;
    }

    private void RebuildFloatLayer()
    {
        if (floatCanvas is null)
            return;
        floatCanvas.Children.Clear();
        var index = 0;
        var canvasWidth = floatCanvas.Bounds.Width > 0 ? floatCanvas.Bounds.Width : Bounds.Width;
        var canvasHeight = floatCanvas.Bounds.Height > 0 ? floatCanvas.Bounds.Height : Bounds.Height;
        foreach (var pane in controller.GetFloatingPanes())
        {
            var left = double.IsNaN(pane.FloatLeft) ? 24 + index * 28 : pane.FloatLeft;
            var top = double.IsNaN(pane.FloatTop) ? 24 + index * 28 : pane.FloatTop;
            left = ClampFloatCoordinate(left, pane.FloatWidth, canvasWidth);
            top = ClampFloatCoordinate(top, pane.FloatHeight, canvasHeight);
            pane.FloatLeft = left;
            pane.FloatTop = top;

            var window = new SkyDockFloatWindow { Pane = pane };
            window.DockBackRequested += (_, _) =>
            {
                controller.DockBack(pane);
                RefreshAfterLayoutChange();
            };
            window.CloseRequested += (_, _) =>
            {
                pane.IsVisible = false;
                RefreshAfterLayoutChange();
            };
            window.PositionDragDelta += (_, delta) =>
            {
                pane.FloatLeft = Math.Max(0, pane.FloatLeft + delta.X);
                pane.FloatTop = Math.Max(0, pane.FloatTop + delta.Y);
                Canvas.SetLeft(window, pane.FloatLeft);
                Canvas.SetTop(window, pane.FloatTop);
            };

            Canvas.SetLeft(window, left);
            Canvas.SetTop(window, top);
            floatCanvas.Children.Add(window);
            index++;
        }

        foreach (var document in SkyDocumentShellAccessor.GetByPlacement(Documents, SkyDocumentShellPlacement.Float))
        {
            var left = double.IsNaN(document.ShellFloatLeft) ? 48 + index * 28 : document.ShellFloatLeft;
            var top = double.IsNaN(document.ShellFloatTop) ? 48 + index * 28 : document.ShellFloatTop;
            left = ClampFloatCoordinate(left, DefaultDocumentFloatWidth, canvasWidth);
            top = ClampFloatCoordinate(top, DefaultDocumentFloatHeight, canvasHeight);
            document.ShellFloatLeft = left;
            document.ShellFloatTop = top;

            var window = new SkyDockFloatWindow
            {
                Document = document,
                Width = DefaultDocumentFloatWidth,
                Height = DefaultDocumentFloatHeight,
            };
            window.DockBackRequested += (_, _) =>
            {
                documentShellController.DockBack(document);
                RefreshAfterLayoutChange();
            };
            window.CloseRequested += (_, _) =>
            {
                documentWell?.TryCloseDocument(document);
                RefreshAfterLayoutChange();
            };
            window.PositionDragDelta += (_, delta) =>
            {
                document.ShellFloatLeft = Math.Max(0, document.ShellFloatLeft + delta.X);
                document.ShellFloatTop = Math.Max(0, document.ShellFloatTop + delta.Y);
                Canvas.SetLeft(window, document.ShellFloatLeft);
                Canvas.SetTop(window, document.ShellFloatTop);
            };

            Canvas.SetLeft(window, left);
            Canvas.SetTop(window, top);
            floatCanvas.Children.Add(window);
            index++;
        }
    }

    private static double ClampFloatCoordinate(double value, double size, double canvasSize)
    {
        if (canvasSize <= 0)
            return Math.Max(0, value);
        var max = Math.Max(0, canvasSize - size);
        return Math.Clamp(value, 0, max);
    }
}
