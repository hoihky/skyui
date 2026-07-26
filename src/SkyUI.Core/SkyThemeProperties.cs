using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using SkyUI.Core.Theming;

namespace SkyUI.Core;

/// <summary>Attached properties for runtime theme overrides. Set <see cref="AccentOverride"/> on <see cref="Application"/>.</summary>
public static class SkyThemeProperties
{
  public static readonly AttachedProperty<Color?> AccentOverrideProperty =
      AvaloniaProperty.RegisterAttached<AvaloniaObject, Color?>("AccentOverride", typeof(SkyThemeProperties));

  public static readonly AttachedProperty<SkyDensity> DensityProperty =
      AvaloniaProperty.RegisterAttached<AvaloniaObject, SkyDensity>(
          "Density",
          typeof(SkyThemeProperties),
          defaultValue: SkyDensity.Comfortable);

  static SkyThemeProperties()
  {
      AccentOverrideProperty.Changed.AddClassHandler<AvaloniaObject>(OnAccentOverrideChanged);
      DensityProperty.Changed.AddClassHandler<AvaloniaObject>(OnDensityChanged);
  }

  public static Color? GetAccentOverride(AvaloniaObject element) =>
      element.GetValue(AccentOverrideProperty);

  public static void SetAccentOverride(AvaloniaObject element, Color? value) =>
      element.SetValue(AccentOverrideProperty, value);

  public static SkyDensity GetDensity(AvaloniaObject element) =>
      element.GetValue(DensityProperty);

  public static void SetDensity(AvaloniaObject element, SkyDensity value) =>
      element.SetValue(DensityProperty, value);

  private static void OnAccentOverrideChanged(AvaloniaObject sender, AvaloniaPropertyChangedEventArgs e)
  {
    if (sender is not IResourceHost host)
      return;

    SkyAccentOverrideApplicator.Apply(host, e.NewValue as Color?);
  }

  private static void OnDensityChanged(AvaloniaObject sender, AvaloniaPropertyChangedEventArgs e)
  {
    if (sender is not IResourceHost host)
      return;

    SkyDensityApplicator.Apply(host, e.NewValue is SkyDensity d ? d : SkyDensity.Comfortable);
  }
}
