using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace SkyUI.Controls;

/// <summary>Pulsing placeholder block for loading states.</summary>
public class SkySkeleton : TemplatedControl
{
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<SkySkeleton, bool>(nameof(IsActive), true);

    static SkySkeleton()
    {
        WidthProperty.OverrideDefaultValue<SkySkeleton>(120);
        HeightProperty.OverrideDefaultValue<SkySkeleton>(16);
        MinHeightProperty.OverrideDefaultValue<SkySkeleton>(8);
        CornerRadiusProperty.OverrideDefaultValue<SkySkeleton>(new CornerRadius(6));
    }

    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }
}
