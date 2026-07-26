using Avalonia.Media;

namespace SkyUI.Icons;

/// <summary>Optional symbol font mapping (Segoe Fluent Icons code points on Windows).</summary>
public static class SkyIconFont
{
    public static FontFamily FamilyWithFallback { get; } = new(
        "Segoe Fluent Icons,Segoe MDL2 Assets,avares://Avalonia.Fonts.Inter/Assets#Inter");

    public static string? TryGetGlyph(SkyIconKind kind) =>
        kind switch
        {
            SkyIconKind.Search => "\uE721",
            SkyIconKind.Close => "\uE711",
            SkyIconKind.ChevronDown => "\uE70D",
            SkyIconKind.ChevronRight => "\uE76C",
            SkyIconKind.Check => "\uE73E",
            SkyIconKind.Add => "\uE710",
            SkyIconKind.Remove => "\uE738",
            SkyIconKind.Play => "\uE768",
            SkyIconKind.Pause => "\uE769",
            SkyIconKind.Home => "\uE80F",
            SkyIconKind.Settings => "\uE713",
            _ => null,
        };
}
