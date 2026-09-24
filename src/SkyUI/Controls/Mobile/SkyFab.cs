using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.VisualTree;
using SkyUI.Icons;

namespace SkyUI.Controls;

/// <summary>Circular floating action button with optional extended label and safe-area margins.</summary>
public class SkyFab : TemplatedControl
{
    public const string ActionButtonPartName = "PART_ActionButton";
    public const double DefaultSize = 56;

    public static readonly StyledProperty<SkyIconKind> IconProperty =
        AvaloniaProperty.Register<SkyFab, SkyIconKind>(nameof(Icon), SkyIconKind.Add);

    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<SkyFab, string?>(nameof(Label));

    public static readonly StyledProperty<bool> IsExtendedProperty =
        AvaloniaProperty.Register<SkyFab, bool>(nameof(IsExtended));

    public static readonly StyledProperty<bool> HonorSafeAreaProperty =
        AvaloniaProperty.Register<SkyFab, bool>(nameof(HonorSafeArea), true);

    public static readonly StyledProperty<Thickness> EdgeMarginProperty =
        AvaloniaProperty.Register<SkyFab, Thickness>(nameof(EdgeMargin), new Thickness(16));

    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<SkyFab, ICommand?>(nameof(Command));

    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<SkyFab, object?>(nameof(CommandParameter));

    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
        RoutedEvent.Register<SkyFab, RoutedEventArgs>(nameof(Click), RoutingStrategies.Bubble);

    private readonly SkyFabInsetsCoordinator insetsCoordinator;
    private Button? actionButton;

    static SkyFab()
    {
        WidthProperty.OverrideDefaultValue<SkyFab>(DefaultSize);
        HeightProperty.OverrideDefaultValue<SkyFab>(DefaultSize);
        MinWidthProperty.OverrideDefaultValue<SkyFab>(DefaultSize);
        MinHeightProperty.OverrideDefaultValue<SkyFab>(DefaultSize);
        HorizontalAlignmentProperty.OverrideDefaultValue<SkyFab>(HorizontalAlignment.Right);
        VerticalAlignmentProperty.OverrideDefaultValue<SkyFab>(VerticalAlignment.Bottom);

        HonorSafeAreaProperty.Changed.AddClassHandler<SkyFab>((fab, _) => fab.ApplyInsets());
        EdgeMarginProperty.Changed.AddClassHandler<SkyFab>((fab, _) => fab.ApplyInsets());
        IsExtendedProperty.Changed.AddClassHandler<SkyFab>((fab, _) => fab.UpdateExtendedMetrics());
    }

    public SkyFab()
    {
        Classes.Add("sky");
        Classes.Add("sky-fab");
        insetsCoordinator = new SkyFabInsetsCoordinator(this);
    }

    public SkyIconKind Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public bool IsExtended
    {
        get => GetValue(IsExtendedProperty);
        set => SetValue(IsExtendedProperty, value);
    }

    public bool HonorSafeArea
    {
        get => GetValue(HonorSafeAreaProperty);
        set => SetValue(HonorSafeAreaProperty, value);
    }

    public Thickness EdgeMargin
    {
        get => GetValue(EdgeMarginProperty);
        set => SetValue(EdgeMarginProperty, value);
    }

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public event EventHandler<RoutedEventArgs>? Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (actionButton is not null)
            actionButton.Click -= OnActionButtonClick;

        actionButton = e.NameScope.Find(ActionButtonPartName) as Button;
        if (actionButton is not null)
            actionButton.Click += OnActionButtonClick;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateExtendedMetrics();
        insetsCoordinator.HonorSafeArea = HonorSafeArea;
        insetsCoordinator.EdgeMargin = EdgeMargin;
        insetsCoordinator.Attach();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        insetsCoordinator.Detach();
        base.OnDetachedFromVisualTree(e);
    }

    private void ApplyInsets()
    {
        insetsCoordinator.HonorSafeArea = HonorSafeArea;
        insetsCoordinator.EdgeMargin = EdgeMargin;
        insetsCoordinator.Attach();
    }

    private void UpdateExtendedMetrics()
    {
        if (IsExtended)
        {
            Width = double.NaN;
            Height = DefaultSize;
            MinWidth = DefaultSize;
            MinHeight = DefaultSize;
            return;
        }

        Width = DefaultSize;
        Height = DefaultSize;
        MinWidth = DefaultSize;
        MinHeight = DefaultSize;
    }

    private void OnActionButtonClick(object? sender, RoutedEventArgs e)
    {
        if (Command?.CanExecute(CommandParameter) == true)
            Command.Execute(CommandParameter);

        RaiseEvent(new RoutedEventArgs(ClickEvent));
    }
}
