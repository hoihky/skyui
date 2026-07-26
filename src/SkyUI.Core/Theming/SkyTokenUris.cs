namespace SkyUI.Core.Theming;

/// <summary>
/// Avalonia <c>avares://</c> URIs for design-token resource dictionaries.
/// Themes merge a palette first, then <see cref="Tokens"/> (see <see cref="SkyPaletteKeys"/>).
/// </summary>
public static class SkyTokenUris
{
    public const string Tokens = "avares://SkyUI.Core/Themes/SkyTokens.axaml";
}
