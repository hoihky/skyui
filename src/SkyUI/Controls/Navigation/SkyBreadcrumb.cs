using Avalonia;
using Avalonia.Controls;

namespace SkyUI.Controls;

/// <summary>Horizontal breadcrumb trail.</summary>
public class SkyBreadcrumb : ItemsControl
{
    public static readonly StyledProperty<string> SeparatorProperty =
        AvaloniaProperty.Register<SkyBreadcrumb, string>(nameof(Separator), "/");

    public string Separator
    {
        get => GetValue(SeparatorProperty);
        set => SetValue(SeparatorProperty, value);
    }
}
