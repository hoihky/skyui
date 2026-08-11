using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace SkyUI.Controls;

/// <summary>Full-width banner for app-level notices with optional action content.</summary>
public class SkyBanner : TemplatedControl
{
    public const string CloseButtonPartName = "PART_CloseButton";
    public const string ActionHostPartName = "PART_ActionHost";

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<SkyBanner, string?>(nameof(Title));

    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<SkyBanner, string?>(nameof(Message));

    public static readonly StyledProperty<SkyFeedbackVariant> VariantProperty =
        AvaloniaProperty.Register<SkyBanner, SkyFeedbackVariant>(nameof(Variant), SkyFeedbackVariant.Info);

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<SkyBanner, bool>(nameof(IsOpen), true);

    public static readonly StyledProperty<bool> IsCloseableProperty =
        AvaloniaProperty.Register<SkyBanner, bool>(nameof(IsCloseable), true);

    public static readonly StyledProperty<object?> ActionContentProperty =
        AvaloniaProperty.Register<SkyBanner, object?>(nameof(ActionContent));

    public static readonly RoutedEvent<RoutedEventArgs> CloseRequestedEvent =
        RoutedEvent.Register<SkyBanner, RoutedEventArgs>(nameof(CloseRequested), RoutingStrategies.Bubble);

    private Button? _closeButton;

    static SkyBanner()
    {
        VariantProperty.Changed.AddClassHandler<SkyBanner>((b, _) => b.SyncVariantClass());
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public SkyFeedbackVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public bool IsCloseable
    {
        get => GetValue(IsCloseableProperty);
        set => SetValue(IsCloseableProperty, value);
    }

    public object? ActionContent
    {
        get => GetValue(ActionContentProperty);
        set => SetValue(ActionContentProperty, value);
    }

    public event EventHandler<RoutedEventArgs>? CloseRequested
    {
        add => AddHandler(CloseRequestedEvent, value);
        remove => RemoveHandler(CloseRequestedEvent, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        SyncVariantClass();

        if (_closeButton is not null)
            _closeButton.Click -= OnCloseClick;

        _closeButton = e.NameScope.Find(CloseButtonPartName) as Button;
        if (_closeButton is not null)
            _closeButton.Click += OnCloseClick;
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        IsOpen = false;
        RaiseEvent(new RoutedEventArgs(CloseRequestedEvent));
    }

    private void SyncVariantClass()
    {
        foreach (var name in Enum.GetNames<SkyFeedbackVariant>())
            Classes.Remove($"sky-feedback-{name.ToLowerInvariant()}");

        Classes.Add($"sky-feedback-{Variant.ToString().ToLowerInvariant()}");
    }
}
