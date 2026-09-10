using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using SkyUI.Core.Theming;

namespace SkyUI.Controls;

/// <summary>Bottom sheet host with scrim overlay (mobile-style panel).</summary>
public class SkySheetHost : TemplatedControl
{
    public const string CloseButtonPartName = "PART_CloseButton";
    public const string OverlayPartName = "PART_Overlay";
    public const string SheetPanelPartName = "PART_SheetPanel";

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<SkySheetHost, bool>(nameof(IsOpen));

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<SkySheetHost, string?>(nameof(Title));

    public static readonly StyledProperty<object?> SheetContentProperty =
        AvaloniaProperty.Register<SkySheetHost, object?>(nameof(SheetContent));

    public static readonly RoutedEvent<RoutedEventArgs> ClosedEvent =
        RoutedEvent.Register<SkySheetHost, RoutedEventArgs>(nameof(Closed), RoutingStrategies.Bubble);

    private Button? closeButton;
    private Panel? overlayPanel;
    private Control? sheetPanel;
    private CancellationTokenSource? animationCancellation;
    private bool templateApplied;

    static SkySheetHost()
    {
        IsOpenProperty.Changed.AddClassHandler<SkySheetHost>((host, e) => _ = host.OnIsOpenChangedAsync(e));
    }

    public SkySheetHost()
    {
        IsHitTestVisible = false;
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

    public object? SheetContent
    {
        get => GetValue(SheetContentProperty);
        set => SetValue(SheetContentProperty, value);
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
        RaiseEvent(new RoutedEventArgs(ClosedEvent));
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
        sheetPanel = e.NameScope.Find(SheetPanelPartName) as Control;

        if (closeButton is not null)
            closeButton.Click += OnCloseClick;

        if (overlayPanel is not null)
            overlayPanel.PointerPressed += OnOverlayPointerPressed;

        templateApplied = true;
        if (IsOpen)
        {
            IsHitTestVisible = true;
            _ = PlayOpenAnimationAsync(CancellationToken.None);
        }
        else
        {
            SetClosedState();
        }
    }

    private async Task OnIsOpenChangedAsync(AvaloniaPropertyChangedEventArgs change)
    {
        var isOpen = change.GetNewValue<bool>();

        if (!templateApplied || overlayPanel is null || sheetPanel is null)
        {
            IsHitTestVisible = isOpen;
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
                SetClosedState();
        }
    }

    private async Task PlayOpenAnimationAsync(CancellationToken token)
    {
        overlayPanel!.IsVisible = true;
        overlayPanel.IsHitTestVisible = true;
        overlayPanel.Opacity = 0;
        sheetPanel!.Opacity = 0;

        const double offset = 24d;
        await Task.WhenAll(
            SkyMotionAnimator.Default.FadeAsync(overlayPanel, 0, 1, SkyMotionDurations.Enter, token),
            SkyMotionAnimator.Default.FadeAsync(sheetPanel, 0, 1, SkyMotionDurations.Enter, token),
            SkyMotionAnimator.Default.TranslateYAsync(sheetPanel, offset, 0, SkyMotionDurations.Enter, token));
    }

    private async Task PlayCloseAnimationAsync(CancellationToken token)
    {
        await Task.WhenAll(
            SkyMotionAnimator.Default.FadeAsync(overlayPanel!, overlayPanel!.Opacity, 0, SkyMotionDurations.Exit, token),
            SkyMotionAnimator.Default.FadeAsync(sheetPanel!, sheetPanel!.Opacity, 0, SkyMotionDurations.Exit, token),
            SkyMotionAnimator.Default.TranslateYAsync(sheetPanel!, 0, 24, SkyMotionDurations.Exit, token));
    }

    private void SetClosedState()
    {
        IsHitTestVisible = false;

        if (overlayPanel is null)
            return;

        overlayPanel.IsVisible = false;
        overlayPanel.IsHitTestVisible = false;
        overlayPanel.Opacity = 0;

        if (sheetPanel is null)
            return;

        sheetPanel.Opacity = 0;
        if (sheetPanel.RenderTransform is TranslateTransform translate)
            translate.Y = 24;
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();

    private void OnOverlayPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source == overlayPanel)
            Close();
    }
}
