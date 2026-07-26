namespace SkyUI.Core.Theming;

/// <summary>Layout density for Sky primitives (padding and min heights).</summary>
public enum SkyDensity
{
    /// <summary>Default spacing from DESIGN.md (e.g. 40px buttons, 44px fields).</summary>
    Comfortable,

    /// <summary>Reduced padding and min heights for data-dense UIs.</summary>
    Compact,
}
