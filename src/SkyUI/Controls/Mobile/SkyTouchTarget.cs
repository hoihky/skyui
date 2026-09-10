using Avalonia;
using Avalonia.Controls;

namespace SkyUI.Controls;

/// <summary>Attached properties that enforce minimum touch target size (44 logical px).</summary>
public static class SkyTouchTarget
{
    public const double RecommendedSize = 44;

    public static readonly AttachedProperty<bool> EnsureProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>("EnsureTouchTarget", typeof(SkyTouchTarget), defaultValue: false);

    public static readonly AttachedProperty<double> MinSizeProperty =
        AvaloniaProperty.RegisterAttached<Control, double>("MinTouchSize", typeof(SkyTouchTarget), RecommendedSize);

    static SkyTouchTarget()
    {
        EnsureProperty.Changed.AddClassHandler<Control>(OnEnsureChanged);
        MinSizeProperty.Changed.AddClassHandler<Control>(OnMinSizeChanged);
    }

    public static bool GetEnsureTouchTarget(Control control) => control.GetValue(EnsureProperty);

    public static void SetEnsureTouchTarget(Control control, bool value) => control.SetValue(EnsureProperty, value);

    public static double GetMinTouchSize(Control control) => control.GetValue(MinSizeProperty);

    public static void SetMinTouchSize(Control control, double value) => control.SetValue(MinSizeProperty, value);

    private static void OnEnsureChanged(Control control, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.GetNewValue<bool>())
            Apply(control);
    }

    private static void OnMinSizeChanged(Control control, AvaloniaPropertyChangedEventArgs e)
    {
        if (GetEnsureTouchTarget(control))
            Apply(control);
    }

    internal static void Apply(Control control)
    {
        var min = GetMinTouchSize(control);
        if (control.MinWidth < min)
            control.MinWidth = min;
        if (control.MinHeight < min)
            control.MinHeight = min;
    }
}
