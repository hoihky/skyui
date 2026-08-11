using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace SkyUI.Controls;

/// <summary>Inline alert for status messages (info, success, warning, error).</summary>
public class SkyAlert : TemplatedControl
{
    public const string CloseButtonPartName = "PART_CloseButton";

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<SkyAlert, string?>(nameof(Title));

    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<SkyAlert, string?>(nameof(Message));

    public static readonly StyledProperty<SkyFeedbackVariant> VariantProperty =
        AvaloniaProperty.Register<SkyAlert, SkyFeedbackVariant>(nameof(Variant), SkyFeedbackVariant.Neutral);

    public static readonly StyledProperty<bool> IsCloseableProperty =
        AvaloniaProperty.Register<SkyAlert, bool>(nameof(IsCloseable));

    public static readonly RoutedEvent<RoutedEventArgs> CloseRequestedEvent =
        RoutedEvent.Register<SkyAlert, RoutedEventArgs>(nameof(CloseRequested), RoutingStrategies.Bubble);

    private Button? _closeButton;

    static SkyAlert()
    {
        VariantProperty.Changed.AddClassHandler<SkyAlert>((a, _) => a.SyncVariantClass());
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

    public bool IsCloseable
    {
        get => GetValue(IsCloseableProperty);
        set => SetValue(IsCloseableProperty, value);
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

    private void OnCloseClick(object? sender, RoutedEventArgs e) =>
        RaiseEvent(new RoutedEventArgs(CloseRequestedEvent));

    private void SyncVariantClass()
    {
        foreach (var name in Enum.GetNames<SkyFeedbackVariant>())
            Classes.Remove($"sky-feedback-{name.ToLowerInvariant()}");

        Classes.Add($"sky-feedback-{Variant.ToString().ToLowerInvariant()}");
    }
}
