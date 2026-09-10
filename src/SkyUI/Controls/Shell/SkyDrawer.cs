using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using SkyUI.Core.Theming;

namespace SkyUI.Controls;

/// <summary>Slide-in side panel for filters, settings, or navigation on narrow widths.</summary>
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
    private Control? drawerPanel;
    private CancellationTokenSource? animationCancellation;
    private bool templateApplied;

    static SkyDrawer()
    {
        IsOpenProperty.Changed.AddClassHandler<SkyDrawer>((drawer, e) => _ = drawer.OnIsOpenChangedAsync(e));
        PlacementProperty.Changed.AddClassHandler<SkyDrawer>((drawer, _) => drawer.SyncPlacementClass());
        DrawerWidthProperty.Changed.AddClassHandler<SkyDrawer>((drawer, _) => drawer.SyncClosedTransform());
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
        drawerPanel = e.NameScope.Find(DrawerPanelPartName) as Control;

        if (closeButton is not null)
            closeButton.Click += OnCloseClick;

        if (overlayPanel is not null)
            overlayPanel.PointerPressed += OnOverlayPointerPressed;

        templateApplied = true;
        SyncPlacementClass();

        if (IsOpen)
        {
            IsHitTestVisible = true;
            PrepareDrawerForOpen();
            overlayPanel!.IsVisible = true;
            overlayPanel.IsHitTestVisible = true;
            overlayPanel.Opacity = 1;
            drawerPanel!.Opacity = 1;
            SyncClosedTransform(0);
        }
        else
        {
            SetClosedState();
        }
    }

    private void SyncPlacementClass()
    {
        Classes.Set("sky-drawer-left", Placement == SkyDrawerPlacement.Left);
        Classes.Set("sky-drawer-right", Placement == SkyDrawerPlacement.Right);
    }

    private async Task OnIsOpenChangedAsync(AvaloniaPropertyChangedEventArgs change)
    {
        var isOpen = change.GetNewValue<bool>();
        var wasOpen = change.GetOldValue<bool>();

        if (!templateApplied || overlayPanel is null || drawerPanel is null)
        {
            IsHitTestVisible = isOpen;
            if (!isOpen && wasOpen)
                RaiseClosed();

            return;
        }

        animationCancellation?.Cancel();
        animationCancellation = new CancellationTokenSource();
        var token = animationCancellation.Token;

        try
        {
            if (isOpen)
            {
                IsHitTestVisible = true;
                await PlayOpenAnimationAsync(token);
            }
            else
            {
                await PlayCloseAnimationAsync(token);
            }
        }
        catch (OperationCanceledException)
        {
            // Superseded by a newer transition.
        }
        finally
        {
            if (!IsOpen)
            {
                SetClosedState();
                if (wasOpen)
                    RaiseClosed();
            }
        }
    }

    private double GetClosedOffsetX() =>
        Placement == SkyDrawerPlacement.Left ? -DrawerWidth : DrawerWidth;

    private void PrepareDrawerForOpen()
    {
        if (overlayPanel is null || drawerPanel is null)
            return;

        overlayPanel.IsVisible = true;
        overlayPanel.IsHitTestVisible = true;
        overlayPanel.Opacity = 0;
        drawerPanel.Opacity = 0;
        SyncClosedTransform();
    }

    private void SyncClosedTransform(double? offset = null)
    {
        if (drawerPanel is null)
            return;

        var closedOffset = offset ?? GetClosedOffsetX();
        if (drawerPanel.RenderTransform is TranslateTransform translate)
            translate.X = closedOffset;
        else
            drawerPanel.RenderTransform = new TranslateTransform(closedOffset, 0);
    }

    private async Task PlayOpenAnimationAsync(CancellationToken token)
    {
        PrepareDrawerForOpen();

        var fromOffset = GetClosedOffsetX();
        await Task.WhenAll(
            SkyMotionAnimator.Default.FadeAsync(overlayPanel!, 0, 1, SkyMotionDurations.Enter, token),
            SkyMotionAnimator.Default.FadeAsync(drawerPanel!, 0, 1, SkyMotionDurations.Enter, token),
            SkyMotionAnimator.Default.TranslateXAsync(drawerPanel!, fromOffset, 0, SkyMotionDurations.Enter, token));
    }

    private async Task PlayCloseAnimationAsync(CancellationToken token)
    {
        var toOffset = GetClosedOffsetX();
        await Task.WhenAll(
            SkyMotionAnimator.Default.FadeAsync(overlayPanel!, overlayPanel!.Opacity, 0, SkyMotionDurations.Exit, token),
            SkyMotionAnimator.Default.FadeAsync(drawerPanel!, drawerPanel!.Opacity, 0, SkyMotionDurations.Exit, token),
            SkyMotionAnimator.Default.TranslateXAsync(drawerPanel!, 0, toOffset, SkyMotionDurations.Exit, token));
    }

    private void SetClosedState()
    {
        IsHitTestVisible = false;

        if (overlayPanel is null)
            return;

        overlayPanel.IsVisible = false;
        overlayPanel.IsHitTestVisible = false;
        overlayPanel.Opacity = 0;

        if (drawerPanel is null)
            return;

        drawerPanel.Opacity = 0;
        SyncClosedTransform();
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
