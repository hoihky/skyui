using Avalonia;
using Avalonia.Controls;

namespace SkyUI.Controls;

/// <summary>Attach Sky-styled tooltips to any control.</summary>
public static class SkyTooltipProperties
{
    public static readonly AttachedProperty<object?> TipProperty =
        AvaloniaProperty.RegisterAttached<Control, object?>("Tip", typeof(SkyTooltipProperties));

    static SkyTooltipProperties()
    {
        TipProperty.Changed.AddClassHandler<Control>(OnTipChanged);
    }

    public static object? GetTip(Control element) => element.GetValue(TipProperty);

    public static void SetTip(Control element, object? value) => element.SetValue(TipProperty, value);

    private static void OnTipChanged(Control control, AvaloniaPropertyChangedEventArgs e)
    {
        var tip = e.NewValue;
        if (tip is null)
        {
            ToolTip.SetTip(control, null);
            return;
        }

        ToolTip.SetTip(control, tip is SkyTooltip skyTooltip
            ? skyTooltip
            : new SkyTooltip { Content = tip });
    }
}
