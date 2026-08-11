namespace SkyUI.Controls;

/// <summary>Navigation chrome for <see cref="SkyNavigationView"/>.</summary>
public enum SkyNavigationDisplayMode
{
    /// <summary>Pick expanded, compact, or bottom from width (<see cref="SkyBreakpoint"/>).</summary>
    Auto,

    /// <summary>Full sidebar with labels (desktop).</summary>
    Expanded,

    /// <summary>Narrow icon sidebar.</summary>
    Compact,

    /// <summary>Bottom bar; sidebar hidden.</summary>
    Bottom,
}
