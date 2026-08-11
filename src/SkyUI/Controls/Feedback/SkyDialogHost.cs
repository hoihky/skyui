using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace SkyUI.Controls;

/// <summary>Modal dialog host with dimmed overlay (embed at root of page/window content).</summary>
public class SkyDialogHost : TemplatedControl
{
    public const string CloseButtonPartName = "PART_CloseButton";
    public const string PrimaryButtonPartName = "PART_PrimaryButton";
    public const string SecondaryButtonPartName = "PART_SecondaryButton";
    public const string OverlayPartName = "PART_Overlay";

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

    private Button? _closeButton;
    private Button? _primaryButton;
    private Button? _secondaryButton;
    private Panel? _overlay;

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

    public void Show() => IsOpen = true;

    public void Close()
    {
        IsOpen = false;
        RaiseEvent(new RoutedEventArgs(ClosedEvent));
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_closeButton is not null)
            _closeButton.Click -= OnCloseClick;

        if (_primaryButton is not null)
            _primaryButton.Click -= OnPrimaryClick;

        if (_secondaryButton is not null)
            _secondaryButton.Click -= OnSecondaryClick;

        if (_overlay is not null)
            _overlay.PointerPressed -= OnOverlayPointerPressed;

        _closeButton = e.NameScope.Find(CloseButtonPartName) as Button;
        _primaryButton = e.NameScope.Find(PrimaryButtonPartName) as Button;
        _secondaryButton = e.NameScope.Find(SecondaryButtonPartName) as Button;
        _overlay = e.NameScope.Find(OverlayPartName) as Panel;

        if (_closeButton is not null)
            _closeButton.Click += OnCloseClick;

        if (_primaryButton is not null)
            _primaryButton.Click += OnPrimaryClick;

        if (_secondaryButton is not null)
            _secondaryButton.Click += OnSecondaryClick;

        if (_overlay is not null)
            _overlay.PointerPressed += OnOverlayPointerPressed;
    }

    private void OnPrimaryClick(object? sender, RoutedEventArgs e) =>
        RaiseEvent(new RoutedEventArgs(PrimaryActionEvent));

    private void OnSecondaryClick(object? sender, RoutedEventArgs e) => Close();

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();

    private void OnOverlayPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source == _overlay)
            Close();
    }
}
