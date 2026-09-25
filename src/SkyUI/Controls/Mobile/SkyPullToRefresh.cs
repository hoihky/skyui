using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using SkyUI.Core.Theming;

namespace SkyUI.Controls;

/// <summary>Wraps a <see cref="ScrollViewer"/> and exposes MVVM-friendly pull-to-refresh.</summary>
public class SkyPullToRefresh : ContentControl
{
    public const string IndicatorPartName = "PART_Indicator";
    public const string ProgressPartName = "PART_Progress";

    public static readonly StyledProperty<bool> IsRefreshingProperty =
        AvaloniaProperty.Register<SkyPullToRefresh, bool>(nameof(IsRefreshing));

    public static readonly StyledProperty<ICommand?> RefreshCommandProperty =
        AvaloniaProperty.Register<SkyPullToRefresh, ICommand?>(nameof(RefreshCommand));

    public static readonly StyledProperty<double> PullThresholdProperty =
        AvaloniaProperty.Register<SkyPullToRefresh, double>(nameof(PullThreshold), 64);

    public static readonly StyledProperty<double> MaxPullDistanceProperty =
        AvaloniaProperty.Register<SkyPullToRefresh, double>(nameof(MaxPullDistance), 112);

    public static readonly StyledProperty<double> IndicatorHeightProperty =
        AvaloniaProperty.Register<SkyPullToRefresh, double>(nameof(IndicatorHeight), 48);

    public static readonly DirectProperty<SkyPullToRefresh, double> PullOffsetProperty =
        AvaloniaProperty.RegisterDirect<SkyPullToRefresh, double>(
            nameof(PullOffset),
            control => control.PullOffset);

    public static readonly RoutedEvent<RoutedEventArgs> RefreshRequestedEvent =
        RoutedEvent.Register<SkyPullToRefresh, RoutedEventArgs>(nameof(RefreshRequested), RoutingStrategies.Bubble);

    private SkyPullToRefreshGestureInteractor? gestureInteractor;
    private ScrollViewer? scrollViewer;
    private Control? indicator;
    private SkyProgressRing? progressRing;
    private CancellationTokenSource? motionCts;
    private double pullOffset;

    static SkyPullToRefresh()
    {
        IsRefreshingProperty.Changed.AddClassHandler<SkyPullToRefresh>((control, e) =>
            control.OnIsRefreshingChanged(e.GetNewValue<bool>()));
    }

    public SkyPullToRefresh()
    {
        Classes.Add("sky");
        Classes.Add("sky-pull-to-refresh");
    }

    public bool IsRefreshing
    {
        get => GetValue(IsRefreshingProperty);
        set => SetValue(IsRefreshingProperty, value);
    }

    public ICommand? RefreshCommand
    {
        get => GetValue(RefreshCommandProperty);
        set => SetValue(RefreshCommandProperty, value);
    }

    public double PullThreshold
    {
        get => GetValue(PullThresholdProperty);
        set => SetValue(PullThresholdProperty, value);
    }

    public double MaxPullDistance
    {
        get => GetValue(MaxPullDistanceProperty);
        set => SetValue(MaxPullDistanceProperty, value);
    }

    public double IndicatorHeight
    {
        get => GetValue(IndicatorHeightProperty);
        set => SetValue(IndicatorHeightProperty, value);
    }

    public double PullOffset
    {
        get => pullOffset;
        private set => SetAndRaise(PullOffsetProperty, ref pullOffset, value);
    }

    public event EventHandler<RoutedEventArgs>? RefreshRequested
    {
        add => AddHandler(RefreshRequestedEvent, value);
        remove => RemoveHandler(RefreshRequestedEvent, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        indicator = e.NameScope.Find(IndicatorPartName) as Control;
        progressRing = e.NameScope.Find(ProgressPartName) as SkyProgressRing;
        AttachScrollHost();
        UpdateVisualState(animate: false);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ContentProperty)
            AttachScrollHost();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AttachScrollHost();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        CancelMotion();
        gestureInteractor?.Dispose();
        gestureInteractor = null;
        scrollViewer = null;
        base.OnDetachedFromVisualTree(e);
    }

    private void AttachScrollHost()
    {
        gestureInteractor?.Dispose();
        gestureInteractor = null;
        scrollViewer = Content as ScrollViewer;

        if (scrollViewer is null)
            return;

        gestureInteractor = new SkyPullToRefreshGestureInteractor(
            scrollViewer,
            () => !IsRefreshing,
            OnPullOffsetChanged,
            TryTriggerRefresh);

        gestureInteractor.Attach();
        UpdateVisualState(animate: false);
    }

    private void OnPullOffsetChanged(double offset)
    {
        if (IsRefreshing)
            return;

        var clamped = Math.Min(Math.Max(0, offset), MaxPullDistance);
        PullOffset = clamped;
        UpdateVisualState(animate: false);
    }

    private bool TryTriggerRefresh()
    {
        if (IsRefreshing || PullOffset < PullThreshold)
            return false;

        var commandStarted = false;
        if (RefreshCommand?.CanExecute(null) == true)
        {
            RefreshCommand.Execute(null);
            commandStarted = true;
        }

        RaiseEvent(new RoutedEventArgs(RefreshRequestedEvent));

        // The command/view model owns IsRefreshing when a command is bound.
        // Setting it here first would make CanExecute return false and strand the spinner.
        if (!commandStarted)
            IsRefreshing = true;

        return true;
    }

    private void OnIsRefreshingChanged(bool isRefreshing)
    {
        if (!isRefreshing)
        {
            gestureInteractor?.ResetPull(animate: false);
            PullOffset = 0;
        }

        _ = UpdateVisualStateAsync(isRefreshing ? SkyMotionDurations.Enter : SkyMotionDurations.Exit);
    }

    private void UpdateVisualState(bool animate) =>
        _ = UpdateVisualStateAsync(animate ? SkyMotionDurations.Exit : TimeSpan.Zero);

    private async Task UpdateVisualStateAsync(TimeSpan duration)
    {
        if (scrollViewer is null)
            return;

        CancelMotion();

        var targetOffset = IsRefreshing ? IndicatorHeight : PullOffset;
        ApplyIndicatorState();

        if (duration <= TimeSpan.Zero)
        {
            EnsureTranslate(scrollViewer).Y = targetOffset;
            return;
        }

        motionCts = new CancellationTokenSource();
        var token = motionCts.Token;
        var from = GetTranslateY(scrollViewer);

        try
        {
            await SkyMotionAnimator.Default.TranslateYAsync(scrollViewer, from, targetOffset, duration, token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (!token.IsCancellationRequested)
            EnsureTranslate(scrollViewer).Y = targetOffset;
    }

    private void ApplyIndicatorState()
    {
        var showIndicator = IsRefreshing;
        var indicatorOpacity = showIndicator
            ? 1
            : Math.Clamp(PullOffset / Math.Max(1, PullThreshold), 0, 1);

        if (indicator is not null)
        {
            indicator.Opacity = indicatorOpacity;
            indicator.IsVisible = showIndicator || indicatorOpacity > 0;
        }

        if (progressRing is not null)
            progressRing.IsIndeterminate = IsRefreshing;
    }

    private void CancelMotion()
    {
        motionCts?.Cancel();
        motionCts?.Dispose();
        motionCts = null;
    }

    private static double GetTranslateY(Visual target) =>
        target.RenderTransform is TranslateTransform translate ? translate.Y : 0;

    private static TranslateTransform EnsureTranslate(Visual target)
    {
        if (target.RenderTransform is TranslateTransform existing)
            return existing;

        var created = new TranslateTransform();
        target.RenderTransform = created;
        return created;
    }
}
