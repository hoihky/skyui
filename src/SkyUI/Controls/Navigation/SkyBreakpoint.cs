namespace SkyUI.Controls;

/// <summary>Layout breakpoints aligned with <c>DESIGN.md</c> (width in device pixels).</summary>
public static class SkyBreakpoint
{
    public const double MobileSmall = 425;
    public const double Mobile = 576;
    public const double Tablet = 768;
    public const double TabletLarge = 896;
    public const double DesktopSmall = 1024;
    public const double Desktop = 1280;

    /// <summary>Resolves auto navigation mode from control width.</summary>
    public static SkyNavigationDisplayMode ResolveNavigationDisplayMode(double width) =>
        width >= DesktopSmall ? SkyNavigationDisplayMode.Expanded
        : width >= Tablet ? SkyNavigationDisplayMode.Compact
        : SkyNavigationDisplayMode.Bottom;
}
