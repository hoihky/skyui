using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.VisualTree;

namespace SkyUI.Controls;

/// <summary>Applies device safe-area insets as padding around content.</summary>
public class SkySafeArea : ContentControl
{
    public static readonly StyledProperty<bool> ApplyTopProperty =
        AvaloniaProperty.Register<SkySafeArea, bool>(nameof(ApplyTop), true);

    public static readonly StyledProperty<bool> ApplyBottomProperty =
        AvaloniaProperty.Register<SkySafeArea, bool>(nameof(ApplyBottom), true);

    public static readonly StyledProperty<bool> ApplyLeftProperty =
        AvaloniaProperty.Register<SkySafeArea, bool>(nameof(ApplyLeft), true);

    public static readonly StyledProperty<bool> ApplyRightProperty =
        AvaloniaProperty.Register<SkySafeArea, bool>(nameof(ApplyRight), true);

    private IInsetsManager? insetsManager;

    public SkySafeArea()
    {
        Classes.Add("sky");
        Classes.Add("sky-safe-area");
    }

    public bool ApplyTop
    {
        get => GetValue(ApplyTopProperty);
        set => SetValue(ApplyTopProperty, value);
    }

    public bool ApplyBottom
    {
        get => GetValue(ApplyBottomProperty);
        set => SetValue(ApplyBottomProperty, value);
    }

    public bool ApplyLeft
    {
        get => GetValue(ApplyLeftProperty);
        set => SetValue(ApplyLeftProperty, value);
    }

    public bool ApplyRight
    {
        get => GetValue(ApplyRightProperty);
        set => SetValue(ApplyRightProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        DetachInsets();
        insetsManager = TopLevel.GetTopLevel(this)?.InsetsManager;
        if (insetsManager is null)
            return;

        insetsManager.SafeAreaChanged += OnSafeAreaChanged;
        ApplyInsets(insetsManager.SafeAreaPadding);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        DetachInsets();
        base.OnDetachedFromVisualTree(e);
    }

    private void DetachInsets()
    {
        if (insetsManager is null)
            return;

        insetsManager.SafeAreaChanged -= OnSafeAreaChanged;
        insetsManager = null;
    }

    private void OnSafeAreaChanged(object? sender, EventArgs e)
    {
        if (insetsManager is not null)
            ApplyInsets(insetsManager.SafeAreaPadding);
    }

    private void ApplyInsets(Thickness safeArea)
    {
        Padding = new Thickness(
            ApplyLeft ? safeArea.Left : 0,
            ApplyTop ? safeArea.Top : 0,
            ApplyRight ? safeArea.Right : 0,
            ApplyBottom ? safeArea.Bottom : 0);
    }
}
