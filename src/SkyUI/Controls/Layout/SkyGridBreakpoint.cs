namespace SkyUI.Controls;

/// <summary>Responsive column resolution for layout grids (aligned with <see cref="SkyBreakpoint"/>).</summary>
public static class SkyGridBreakpoint
{
    public static int ResolveColumns(double width, SkyResponsiveColumnProfile? profile = null)
    {
        profile ??= SkyResponsiveColumnProfile.Default;

        if (width < SkyBreakpoint.MobileSmall)
            return profile.MobileSmall;
        if (width < SkyBreakpoint.Mobile)
            return profile.Mobile;
        if (width < SkyBreakpoint.Tablet)
            return profile.Tablet;
        if (width < SkyBreakpoint.TabletLarge)
            return profile.TabletLarge;
        if (width < SkyBreakpoint.DesktopSmall)
            return profile.DesktopSmall;
        if (width < SkyBreakpoint.Desktop)
            return profile.Desktop;

        return profile.LargeDesktop;
    }
}
