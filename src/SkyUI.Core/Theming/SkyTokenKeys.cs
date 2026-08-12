namespace SkyUI.Core.Theming;

/// <summary>
/// Stable resource keys for the Sky design token layer (<c>SkyTokens.axaml</c>).
/// Use with <see cref="Avalonia.Controls.ResourceDictionary"/> or <c>DynamicResource</c> in XAML.
/// </summary>
public static class SkyTokenKeys
{
    public static class Brush
    {
        public const string Background = "SkyBackgroundBrush";
        public const string Surface = "SkySurfaceBrush";
        public const string SurfaceElevated = "SkySurfaceElevatedBrush";
        public const string Card = "SkyCardBrush";
        public const string CardMid = "SkyCardMidBrush";
        public const string TextPrimary = "SkyTextPrimaryBrush";
        public const string TextSecondary = "SkyTextSecondaryBrush";
        public const string TextSecondaryBright = "SkyTextSecondaryBrightBrush";
        public const string TextLight = "SkyTextLightBrush";
        public const string Accent = "SkyAccentBrush";
        public const string AccentPressed = "SkyAccentPressedBrush";
        public const string OnAccent = "SkyOnAccentBrush";
        public const string Danger = "SkyDangerBrush";
        public const string Warning = "SkyWarningBrush";
        public const string Info = "SkyInfoBrush";
        public const string Border = "SkyBorderBrush";
        public const string BorderStrong = "SkyBorderStrongBrush";
        public const string Separator = "SkySeparatorBrush";
        public const string HoverTint = "SkyHoverTintBrush";
        public const string ListHover = "SkyListHoverBrush";
        public const string SelectedTint = "SkySelectedTintBrush";
        public const string SelectedBorder = "SkySelectedBorderBrush";
        public const string DisabledForeground = "SkyDisabledForegroundBrush";
        public const string DisabledBorder = "SkyDisabledBorderBrush";
        public const string AvatarFallback = "SkyAvatarFallbackBrush";
        public const string BadgeNeutral = "SkyBadgeNeutralBrush";
        public const string LightPill = "SkyLightPillBrush";
        public const string LightPillText = "SkyLightPillTextBrush";
        public const string FocusRing = "SkyFocusRingBrush";
    }

    /// <summary>8px-based spacing scale (see DESIGN.md).</summary>
    public static class Spacing
    {
        public const string Unit = "SkySpaceUnit";
        public const string Space0 = "SkySpace0";
        public const string Space1Px = "SkySpace1Px";
        public const string Space2Px = "SkySpace2Px";
        public const string Space3Px = "SkySpace3Px";
        public const string Space4Px = "SkySpace4Px";
        public const string Space5Px = "SkySpace5Px";
        public const string Space6Px = "SkySpace6Px";
        public const string Space8Px = "SkySpace8Px";
        public const string Space10Px = "SkySpace10Px";
        public const string Space12Px = "SkySpace12Px";
        public const string Space14Px = "SkySpace14Px";
        public const string Space15Px = "SkySpace15Px";
        public const string Space16Px = "SkySpace16Px";
        public const string Space20Px = "SkySpace20Px";
        public const string SpaceHalf = "SkySpaceHalf";
        public const string Space1 = "SkySpace1";
        public const string Space2 = "SkySpace2";
        public const string Space3 = "SkySpace3";
    }

    public static class Radius
    {
        public const string Minimal = "SkyRadiusMinimal";
        public const string Subtle = "SkyRadiusSubtle";
        public const string Standard = "SkyRadiusStandard";
        public const string Comfortable = "SkyRadiusComfortable";
        public const string Medium = "SkyRadiusMedium";
        public const string Large = "SkyRadiusLarge";
        public const string Pill = "SkyRadiusPill";
        public const string Full = "SkyRadiusFull";
        public const string Circle = "SkyRadiusCircle";
    }

    public static class Elevation
    {
        public const string Level0 = "SkyElevation0";
        public const string Level1 = "SkyElevation1";
        public const string Level2 = "SkyElevation2";
        public const string InsetBorder = "SkyElevationInsetBorder";
    }

    public static class Typography
    {
        public const string FontFamilyUi = "SkyFontFamilyUi";
        public const string FontFamilyTitle = "SkyFontFamilyTitle";
        public const string SizeSectionTitle = "SkyFontSizeSectionTitle";
        public const string SizeFeatureHeading = "SkyFontSizeFeatureHeading";
        public const string SizeBody = "SkyFontSizeBody";
        public const string SizeButton = "SkyFontSizeButton";
        public const string SizeCaption = "SkyFontSizeCaption";
        public const string SizeSmall = "SkyFontSizeSmall";
        public const string SizeBadge = "SkyFontSizeBadge";
        public const string SizeMicro = "SkyFontSizeMicro";
        public const string WeightRegular = "SkyFontWeightRegular";
        public const string WeightSemiBold = "SkyFontWeightSemiBold";
        public const string WeightBold = "SkyFontWeightBold";
        public const string LineHeightTight = "SkyLineHeightTight";
        public const string LineHeightSectionTitle = "SkyLineHeightSectionTitle";
        public const string LineHeightNormal = "SkyLineHeightNormal";
        public const string LineHeightCaption = "SkyLineHeightCaption";
        public const string LineHeightSmall = "SkyLineHeightSmall";
        public const string LineHeightBadge = "SkyLineHeightBadge";
        public const string LineHeightButton = "SkyLineHeightButton";
        public const string LineHeightMicro = "SkyLineHeightMicro";
        public const string LetterSpacingButton = "SkyLetterSpacingButton";
        public const string LetterSpacingButtonUpper = "SkyLetterSpacingButtonUpper";
    }

    public static class Focus
    {
        public const string RingThickness = "SkyFocusRingThickness";
    }

    public static class Icon
    {
        public const string SizeSmall = "SkyIconSizeSmall";
        public const string SizeMedium = "SkyIconSizeMedium";
        public const string SizeLarge = "SkyIconSizeLarge";
    }
}
