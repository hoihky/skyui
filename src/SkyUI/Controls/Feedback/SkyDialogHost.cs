using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using SkyUI.Core.Theming;

namespace SkyUI.Controls;

/// <summary>Modal dialog host with dimmed overlay (embed at root of page/window content).</summary>
public class SkyDialogHost : TemplatedControl
{
    public const string CloseButtonPartName = "PART_CloseButton";
    public const string PrimaryButtonPartName = "PART_PrimaryButton";
    public const string SecondaryButtonPartName = "PART_SecondaryButton";
    public const string OverlayPartName = "PART_Overlay";
    public const string DialogPanelPartName = "PART_DialogPanel";

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<SkyDialogHost, bool>(nameof(IsOpen));

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<SkyDialogHost, string?>(nameof(Title));

    public static readonly StyledProperty<object?> DialogContentProperty =
        AvaloniaProperty.Register<SkyDialogHost, object?>(nameof(DialogContent));

    public static readonly StyledProperty<string?> PrimaryButtonTextProperty =
        AvaloniaProperty.Register<SkyDialogHost, string?>(nameof(PrimaryButtonText));

    public static readonly StyledProperty<string?> SecondaryButtonTextProperty =
        AvaloniaProperty.Register<SkyDialogHost, string?>(nameof(SecondaryButtonText));

    public static readonly RoutedEvent<RoutedEventArgs> ClosedEvent =
        RoutedEvent.Register<SkyDialogHost, RoutedEventArgs>(nameof(Closed), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<RoutedEventArgs> PrimaryActionEvent =
        RoutedEvent.Register<SkyDialogHost, RoutedEventArgs>(nameof(PrimaryAction), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<RoutedEventArgs> SecondaryActionEvent =
        RoutedEvent.Register<SkyDialogHost, RoutedEventArgs>(nameof(SecondaryAction), RoutingStrategies.Bubble);

    private Button? closeButton;
    private Button? primaryButton;
    private Button? secondaryButton;
    private Panel? overlayPanel;
    private Control? dialogPanel;
    private CancellationTokenSource? animationCancellation;
    private bool templateApplied;

    static SkyDialogHost()
    {
        IsOpenProperty.Changed.AddClassHandler<SkyDialogHost>((host, e) => _ = host.OnIsOpenChangedAsync(e));
    }

    public SkyDialogHost()
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

    public object? DialogContent
    {
        get => GetValue(DialogContentProperty);
        set => SetValue(DialogContentProperty, value);
    }

    public string? PrimaryButtonText
    {
        get => GetValue(PrimaryButtonTextProperty);
        set => SetValue(PrimaryButtonTextProperty, value);
    }

    public string? SecondaryButtonText
    {
        get => GetValue(SecondaryButtonTextProperty);
        set => SetValue(SecondaryButtonTextProperty, value);
    }

    public event EventHandler<RoutedEventArgs>? Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    public event EventHandler<RoutedEventArgs>? PrimaryAction
    {
        add => AddHandler(PrimaryActionEvent, value);
        remove => RemoveHandler(PrimaryActionEvent, value);
    }

    public event EventHandler<RoutedEventArgs>? SecondaryAction
    {
        add => AddHandler(SecondaryActionEvent, value);
        remove => RemoveHandler(SecondaryActionEvent, value);
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

        if (primaryButton is not null)
            primaryButton.Click -= OnPrimaryClick;

        if (secondaryButton is not null)
            secondaryButton.Click -= OnSecondaryClick;

        if (overlayPanel is not null)
            overlayPanel.PointerPressed -= OnOverlayPointerPressed;

        closeButton = e.NameScope.Find(CloseButtonPartName) as Button;
        primaryButton = e.NameScope.Find(PrimaryButtonPartName) as Button;
        secondaryButton = e.NameScope.Find(SecondaryButtonPartName) as Button;
        overlayPanel = e.NameScope.Find(OverlayPartName) as Panel;
        dialogPanel = e.NameScope.Find(DialogPanelPartName) as Control;

        if (closeButton is not null)
            closeButton.Click += OnCloseClick;

        if (primaryButton is not null)
            primaryButton.Click += OnPrimaryClick;

        if (secondaryButton is not null)
            secondaryButton.Click += OnSecondaryClick;

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

        if (!templateApplied || overlayPanel is null || dialogPanel is null)
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
        dialogPanel!.Opacity = 0;

        await Task.WhenAll(
            SkyMotionAnimator.Default.FadeAsync(overlayPanel, 0, 1, SkyMotionDurations.Enter, token),
            SkyMotionAnimator.Default.ScaleAsync(dialogPanel, 0.96, 1, SkyMotionDurations.Enter, token),
            SkyMotionAnimator.Default.FadeAsync(dialogPanel, 0, 1, SkyMotionDurations.Enter, token));
    }

    private async Task PlayCloseAnimationAsync(CancellationToken token)
    {
        await Task.WhenAll(
            SkyMotionAnimator.Default.FadeAsync(dialogPanel!, dialogPanel!.Opacity, 0, SkyMotionDurations.Exit, token),
            SkyMotionAnimator.Default.FadeAsync(overlayPanel!, overlayPanel!.Opacity, 0, SkyMotionDurations.Exit, token));
    }

    private void SetClosedState()
    {
        IsHitTestVisible = false;

        if (overlayPanel is null)
            return;

        overlayPanel.IsVisible = false;
        overlayPanel.IsHitTestVisible = false;
        overlayPanel.Opacity = 0;

        if (dialogPanel is null)
            return;

        dialogPanel.Opacity = 0;
        if (dialogPanel.RenderTransform is ScaleTransform scale)
        {
            scale.ScaleX = 0.96;
            scale.ScaleY = 0.96;
        }
    }

    private void OnPrimaryClick(object? sender, RoutedEventArgs e) =>
        RaiseEvent(new RoutedEventArgs(PrimaryActionEvent));

    private void OnSecondaryClick(object? sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(SecondaryActionEvent));
        Close();
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();

    private void OnOverlayPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source == overlayPanel)
            Close();
    }
}
