namespace SkyUI.Themes.Sky;

/// <summary>Stable style class names for the Sky Dark preset (<see cref="SkyPresetUris.Dark"/>).</summary>
public static class SkyThemeClasses
{
    public const string Root = "sky";

    public const string Primary = "sky-primary";
    public const string NavPill = "sky-nav-pill";
    public const string Outlined = "sky-outlined";
    public const string Light = "sky-light";
    public const string Subtle = "sky-subtle";
    public const string Play = "sky-play";

    public const string NavList = "sky-nav";
    public const string Card = "sky-card";

    public const string SectionTitle = "sky-section-title";
    public const string FeatureHeading = "sky-feature-heading";
    public const string BodySecondary = "sky-body-secondary";

    /// <summary>Deprecated: use <see cref="Root"/> and modifier classes. Still styled via preset aliases.</summary>
    public static class LegacySpotify
    {
        public const string Root = "spotify";
        public const string Primary = "spotify-primary";
        public const string NavPill = "spotify-nav-pill";
        public const string Outlined = "spotify-outlined";
        public const string Light = "spotify-light";
        public const string Subtle = "spotify-subtle";
        public const string Play = "spotify-play";
        public const string NavList = "spotify-nav";
        public const string Card = "spotify-card";
    }
}
