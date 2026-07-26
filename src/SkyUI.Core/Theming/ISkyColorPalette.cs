namespace SkyUI.Core.Theming;

/// <summary>
/// Contract for a swappable color palette (Strategy pattern: dark, light, high-contrast presets).
/// Implementations are XAML resource dictionaries keyed by <see cref="SkyPaletteKeys"/>.
/// </summary>
public interface ISkyColorPalette
{
    /// <summary>Resource include URI loaded before <see cref="SkyTokenUris.Tokens"/>.</summary>
    string ResourceUri { get; }
}
