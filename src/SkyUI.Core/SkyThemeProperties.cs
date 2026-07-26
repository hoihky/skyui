using Avalonia;
using Avalonia.Media;

namespace SkyUI.Core;

/// <summary>Attached properties for runtime theme overrides (accent, density hooks).</summary>
public static class SkyThemeProperties
{
    public static readonly AttachedProperty<Color?> AccentOverrideProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, Color?>("AccentOverride", typeof(SkyThemeProperties));

    public static Color? GetAccentOverride(AvaloniaObject element) =>
        element.GetValue(AccentOverrideProperty);

    public static void SetAccentOverride(AvaloniaObject element, Color? value) =>
        element.SetValue(AccentOverrideProperty, value);
}
