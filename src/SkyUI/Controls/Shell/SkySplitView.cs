using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace SkyUI.Controls;

/// <summary>Master-detail layout with inline or overlay pane.</summary>
[PseudoClasses("open", "closed", "overlay", "inline", "left", "right", "resizing")]
public class SkySplitView : TemplatedControl
{
    public const string RootPartName = "PART_Root";
    public const string PanePartName = "PART_Pane";
    public const string ResizeGripPartName = "PART_ResizeGrip";
    public const string DetailHostPartName = "PART_DetailHost";
    public const string DetailPartName = "PART_Detail";
    public const string ScrimPartName = "PART_Scrim";

    public static readonly StyledProperty<object?> PaneContentProperty =
        AvaloniaProperty.Register<SkySplitView, object?>(nameof(PaneContent));

    public static readonly StyledProperty<object?> DetailContentProperty =
        AvaloniaProperty.Register<SkySplitView, object?>(nameof(DetailContent));

    public static readonly StyledProperty<bool> IsPaneOpenProperty =
        AvaloniaProperty.Register<SkySplitView, bool>(nameof(IsPaneOpen), true);

    public static readonly StyledProperty<double> OpenPaneLengthProperty =
        AvaloniaProperty.Register<SkySplitView, double>(nameof(OpenPaneLength), 280);

    public static readonly StyledProperty<double> CompactPaneLengthProperty =
        AvaloniaProperty.Register<SkySplitView, double>(nameof(CompactPaneLength), 48);

    public static readonly StyledProperty<double> MinPaneLengthProperty =
        AvaloniaProperty.Register<SkySplitView, double>(nameof(MinPaneLength), 120);

    public static readonly StyledProperty<double> MaxPaneLengthProperty =
        AvaloniaProperty.Register<SkySplitView, double>(nameof(MaxPaneLength), 640);

    public static readonly StyledProperty<bool> IsPaneResizableProperty =
        AvaloniaProperty.Register<SkySplitView, bool>(nameof(IsPaneResizable), true);

    public static readonly StyledProperty<SkySplitViewDisplayMode> DisplayModeProperty =
        AvaloniaProperty.Register<SkySplitView, SkySplitViewDisplayMode>(
            nameof(DisplayMode),
            SkySplitViewDisplayMode.Inline);

    public static readonly StyledProperty<SkySplitViewPanePlacement> PanePlacementProperty =
        AvaloniaProperty.Register<SkySplitView, SkySplitViewPanePlacement>(
            nameof(PanePlacement),
            SkySplitViewPanePlacement.Left);

    private Grid? root;
    private Control? paneHost;
    private Control? resizeGrip;
    private Control? detailHost;
    private Control? scrim;
    private bool isResizing;
    private double resizeStartX;
    private double resizeStartLength;

    static SkySplitView()
    {
        IsPaneOpenProperty.Changed.AddClassHandler<SkySplitView>((view, _) => view.ApplyPaneState());
        OpenPaneLengthProperty.Changed.AddClassHandler<SkySplitView>((view, _) => view.ApplyPaneState());
        CompactPaneLengthProperty.Changed.AddClassHandler<SkySplitView>((view, _) => view.ApplyPaneState());
        MinPaneLengthProperty.Changed.AddClassHandler<SkySplitView>((view, _) => view.ApplyPaneState());
        MaxPaneLengthProperty.Changed.AddClassHandler<SkySplitView>((view, _) => view.ApplyPaneState());
        IsPaneResizableProperty.Changed.AddClassHandler<SkySplitView>((view, _) => view.ApplyPaneState());
        DisplayModeProperty.Changed.AddClassHandler<SkySplitView>((view, _) => view.ApplyPaneState());
        PanePlacementProperty.Changed.AddClassHandler<SkySplitView>((view, _) => view.ApplyPaneState());
    }

    public SkySplitView()
    {
        Classes.Add("sky");
        Classes.Add("sky-split-view");
    }

    public object? PaneContent
    {
        get => GetValue(PaneContentProperty);
        set => SetValue(PaneContentProperty, value);
    }

    public object? DetailContent
    {
        get => GetValue(DetailContentProperty);
        set => SetValue(DetailContentProperty, value);
    }

    public bool IsPaneOpen
    {
        get => GetValue(IsPaneOpenProperty);
        set => SetValue(IsPaneOpenProperty, value);
    }

    public double OpenPaneLength
    {
        get => GetValue(OpenPaneLengthProperty);
        set => SetValue(OpenPaneLengthProperty, value);
    }

    public double CompactPaneLength
    {
        get => GetValue(CompactPaneLengthProperty);
        set => SetValue(CompactPaneLengthProperty, value);
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

    public bool IsPaneResizable
    {
        get => GetValue(IsPaneResizableProperty);
        set => SetValue(IsPaneResizableProperty, value);
    }

    public SkySplitViewDisplayMode DisplayMode
    {
        get => GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

    public SkySplitViewPanePlacement PanePlacement
    {
        get => GetValue(PanePlacementProperty);
        set => SetValue(PanePlacementProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        DetachResizeGripHandlers();
        if (scrim is not null)
            scrim.PointerPressed -= OnScrimPointerPressed;

        root = e.NameScope.Find(RootPartName) as Grid;
        paneHost = e.NameScope.Find(PanePartName) as Control;
        resizeGrip = e.NameScope.Find(ResizeGripPartName) as Control;
        detailHost = e.NameScope.Find(DetailHostPartName) as Control;
        scrim = e.NameScope.Find(ScrimPartName) as Control;

        AttachResizeGripHandlers();

        if (scrim is not null)
            scrim.PointerPressed += OnScrimPointerPressed;

        ApplyPaneState();
    }

    private void AttachResizeGripHandlers()
    {
        if (resizeGrip is null)
            return;

        resizeGrip.PointerPressed += OnResizeGripPointerPressed;
        resizeGrip.PointerMoved += OnResizeGripPointerMoved;
        resizeGrip.PointerReleased += OnResizeGripPointerReleased;
        resizeGrip.PointerCaptureLost += OnResizeGripPointerCaptureLost;
    }

    private void DetachResizeGripHandlers()
    {
        if (resizeGrip is null)
            return;

        resizeGrip.PointerPressed -= OnResizeGripPointerPressed;
        resizeGrip.PointerMoved -= OnResizeGripPointerMoved;
        resizeGrip.PointerReleased -= OnResizeGripPointerReleased;
        resizeGrip.PointerCaptureLost -= OnResizeGripPointerCaptureLost;
    }

    private void OnScrimPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DisplayMode == SkySplitViewDisplayMode.Overlay)
            IsPaneOpen = false;
    }

    private void OnResizeGripPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsPaneResizable || !IsPaneOpen || DisplayMode == SkySplitViewDisplayMode.Overlay)
            return;

        if (!e.GetCurrentPoint(resizeGrip).Properties.IsLeftButtonPressed)
            return;

        isResizing = true;
        resizeStartX = e.GetPosition(this).X;
        resizeStartLength = OpenPaneLength;
        PseudoClasses.Set(":resizing", true);
        e.Pointer.Capture(resizeGrip);
        e.Handled = true;
    }

    private void OnResizeGripPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!isResizing)
            return;

        var delta = e.GetPosition(this).X - resizeStartX;
        var isLeft = PanePlacement == SkySplitViewPanePlacement.Left;
        var nextLength = resizeStartLength + (isLeft ? delta : -delta);
        OpenPaneLength = ClampPaneLength(nextLength);
        e.Handled = true;
    }

    private void OnResizeGripPointerReleased(object? sender, PointerReleasedEventArgs e) =>
        EndResize(e.Pointer);

    private void OnResizeGripPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e) =>
        EndResize(e.Pointer);

    private void EndResize(IPointer pointer)
    {
        if (!isResizing)
            return;

        isResizing = false;
        PseudoClasses.Set(":resizing", false);
        pointer.Capture(null);
    }

    private double ClampPaneLength(double length) =>
        Math.Clamp(length, MinPaneLength, MaxPaneLength);

    private void ApplyPaneState()
    {
        var isOpen = IsPaneOpen;
        var isOverlay = DisplayMode == SkySplitViewDisplayMode.Overlay;
        var isLeft = PanePlacement == SkySplitViewPanePlacement.Left;
        var showResizeGrip = isOpen && !isOverlay && IsPaneResizable;

        PseudoClasses.Set(":open", isOpen);
        PseudoClasses.Set(":closed", !isOpen);
        PseudoClasses.Set(":overlay", isOverlay);
        PseudoClasses.Set(":inline", !isOverlay);
        PseudoClasses.Set(":left", isLeft);
        PseudoClasses.Set(":right", !isLeft);

        if (paneHost is not null)
        {
            paneHost.Width = isOpen ? OpenPaneLength : CompactPaneLength;
            paneHost.IsVisible = isOpen || !isOverlay;
        }

        if (resizeGrip is not null)
        {
            resizeGrip.IsVisible = showResizeGrip;
            resizeGrip.IsHitTestVisible = showResizeGrip;
        }

        if (scrim is not null)
        {
            scrim.IsVisible = isOverlay && isOpen;
            scrim.IsHitTestVisible = isOverlay && isOpen;
        }

        if (root is null || root.ColumnDefinitions.Count < 3)
            return;

        var paneColumn = isLeft ? 0 : 2;
        const int gripColumn = 1;
        var detailColumn = isLeft ? 2 : 0;

        if (paneHost is not null)
            Grid.SetColumn(paneHost, paneColumn);

        if (resizeGrip is not null)
            Grid.SetColumn(resizeGrip, gripColumn);

        if (detailHost is not null)
            Grid.SetColumn(detailHost, detailColumn);

        if (isOverlay || !isOpen)
        {
            root.ColumnDefinitions[0].Width = new GridLength(0);
            root.ColumnDefinitions[1].Width = new GridLength(0);
            root.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
            return;
        }

        root.ColumnDefinitions[paneColumn].Width = GridLength.Auto;
        root.ColumnDefinitions[gripColumn].Width = GridLength.Auto;
        root.ColumnDefinitions[detailColumn].Width = new GridLength(1, GridUnitType.Star);
    }
}
