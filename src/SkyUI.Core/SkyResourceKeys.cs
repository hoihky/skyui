namespace SkyUI.Core;

using SkyUI.Core.Theming;

/// <summary>
/// Obsolete convenience aliases; prefer <see cref="SkyTokenKeys"/> for new code.
/// </summary>
public static class SkyResourceKeys
{
    public const string Background = SkyTokenKeys.Brush.Background;
    public const string Surface = SkyTokenKeys.Brush.Surface;
    public const string SurfaceElevated = SkyTokenKeys.Brush.SurfaceElevated;
    public const string TextPrimary = SkyTokenKeys.Brush.TextPrimary;
    public const string TextSecondary = SkyTokenKeys.Brush.TextSecondary;
    public const string Accent = SkyTokenKeys.Brush.Accent;
    public const string OnAccent = SkyTokenKeys.Brush.OnAccent;
    public const string Border = SkyTokenKeys.Brush.Border;
    public const string Negative = SkyTokenKeys.Brush.Danger;
    public const string Warning = SkyTokenKeys.Brush.Warning;
    public const string Announcement = SkyTokenKeys.Brush.Info;

    /// <summary>Obsolete; use <see cref="TextPrimary"/> or <c>SkyTextPrimaryBrush</c>.</summary>
    public const string SpotifyTextPrimary = "SpotifyTextPrimaryBrush";

    /// <summary>Obsolete; use <see cref="Accent"/> or <c>SkyAccentBrush</c>.</summary>
    public const string SpotifyAccentGreen = "SpotifyAccentGreenBrush";
}
