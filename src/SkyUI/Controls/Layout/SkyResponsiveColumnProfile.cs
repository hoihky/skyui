namespace SkyUI.Controls;

/// <summary>Column counts per <see cref="SkyBreakpoint"/> band for responsive grids.</summary>
public sealed class SkyResponsiveColumnProfile
{
    public int MobileSmall { get; init; } = 1;

    public int Mobile { get; init; } = 1;

    public int Tablet { get; init; } = 2;

    public int TabletLarge { get; init; } = 3;

    public int DesktopSmall { get; init; } = 4;

    public int Desktop { get; init; } = 5;

    public int LargeDesktop { get; init; } = 5;

    public static SkyResponsiveColumnProfile Default { get; } = new();
}
