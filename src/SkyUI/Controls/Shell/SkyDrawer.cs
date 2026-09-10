using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace SkyUI.Controls;

/// <summary>Slide-in side panel for filters, settings, or navigation on narrow widths.</summary>
[TemplatePart(OverlayPartName, typeof(Panel))]
[TemplatePart(DrawerPanelPartName, typeof(Border))]
[TemplatePart(CloseButtonPartName, typeof(Button))]
public class SkyDrawer : TemplatedControl
{
    public const string CloseButtonPartName = "PART_CloseButton";
    public const string OverlayPartName = "PART_Overlay";
    public const string DrawerPanelPartName = "PART_DrawerPanel";

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<SkyDrawer, bool>(
            nameof(IsOpen),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<SkyDrawer, string?>(nameof(Title));

    public static readonly StyledProperty<object?> DrawerContentProperty =
        AvaloniaProperty.Register<SkyDrawer, object?>(nameof(DrawerContent));

    public static readonly StyledProperty<SkyDrawerPlacement> PlacementProperty =
        AvaloniaProperty.Register<SkyDrawer, SkyDrawerPlacement>(
            nameof(Placement),
            SkyDrawerPlacement.Left);

    public static readonly StyledProperty<double> DrawerWidthProperty =
        AvaloniaProperty.Register<SkyDrawer, double>(nameof(DrawerWidth), 320);

    public static readonly RoutedEvent<RoutedEventArgs> ClosedEvent =
        RoutedEvent.Register<SkyDrawer, RoutedEventArgs>(nameof(Closed), RoutingStrategies.Bubble);

    private Button? closeButton;
    private Panel? overlayPanel;
    private Border? drawerPanel;
    private bool templateApplied;

    static SkyDrawer()
    {
        IsOpenProperty.Changed.AddClassHandler<SkyDrawer>((drawer, e) => drawer.OnIsOpenChanged(e));
        PlacementProperty.Changed.AddClassHandler<SkyDrawer>((drawer, _) => drawer.SyncPlacementClass());
    }

    public SkyDrawer()
    {
        IsHitTestVisible = false;
        Classes.Add("sky");
        Classes.Add("sky-drawer");
    }

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public object? DrawerContent
    {
        get => GetValue(DrawerContentProperty);
        set => SetValue(DrawerContentProperty, value);
    }

    public SkyDrawerPlacement Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public double DrawerWidth
    {
        get => GetValue(DrawerWidthProperty);
        set => SetValue(DrawerWidthProperty, value);
    }

    public event EventHandler<RoutedEventArgs>? Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    public void Show() => IsOpen = true;

    public void Close()
    {
        if (!IsOpen)
            return;

        IsOpen = false;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (closeButton is not null)
            closeButton.Click -= OnCloseClick;

        if (overlayPanel is not null)
            overlayPanel.PointerPressed -= OnOverlayPointerPressed;

        closeButton = e.NameScope.Find(CloseButtonPartName) as Button;
        overlayPanel = e.NameScope.Find(OverlayPartName) as Panel;
        drawerPanel = e.NameScope.Find(DrawerPanelPartName) as Border;

        if (closeButton is not null)
            closeButton.Click += OnCloseClick;

        if (overlayPanel is not null)
            overlayPanel.PointerPressed += OnOverlayPointerPressed;

        templateApplied = overlayPanel is not null && drawerPanel is not null;
        SyncPlacementClass();
        ApplyOpenState(IsOpen, raiseClosed: false);
    }

    private void SyncPlacementClass()
    {
        Classes.Set("sky-drawer-left", Placement == SkyDrawerPlacement.Left);
        Classes.Set("sky-drawer-right", Placement == SkyDrawerPlacement.Right);
    }

    private void OnIsOpenChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var isOpen = change.GetNewValue<bool>();
        var wasOpen = change.GetOldValue<bool>();

        if (!templateApplied)
        {
            if (!isOpen && wasOpen)
                RaiseClosed();

            return;
        }

        ApplyOpenState(isOpen, raiseClosed: wasOpen && !isOpen);
    }

    private void ApplyOpenState(bool isOpen, bool raiseClosed)
    {
        if (overlayPanel is null || drawerPanel is null)
            return;

        if (isOpen)
        {
            IsHitTestVisible = true;
            overlayPanel.IsVisible = true;
            overlayPanel.IsHitTestVisible = true;
            overlayPanel.Opacity = 1d;
            drawerPanel.IsVisible = true;
            drawerPanel.IsHitTestVisible = true;
            drawerPanel.Opacity = 1d;
            drawerPanel.RenderTransform = null;
            return;
        }

        IsHitTestVisible = false;
        overlayPanel.IsVisible = false;
        overlayPanel.IsHitTestVisible = false;
        overlayPanel.Opacity = 0d;
        drawerPanel.IsVisible = false;
        drawerPanel.IsHitTestVisible = false;

        if (raiseClosed)
            RaiseClosed();
    }

    private void RaiseClosed() =>
        RaiseEvent(new RoutedEventArgs(ClosedEvent));

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();

    private void OnOverlayPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source == overlayPanel)
            Close();
    }
}
