# Sky design tokens

SkyUI separates **palette** (theme-specific colors), **semantic tokens** (stable names for UI), and **legacy preset aliases** (Spotify resource keys and style classes).

## Layering

```text
SkyPalette.{Dark|Light|HighContrast}.axaml  →  SkyPalette* color slots
        ↓
SkyResources.Themed.axaml (ThemeDictionaries) + SkyResources.Variant.axaml
        ↓
SkyTokens.axaml                →  Sky* brushes, spacing, radius, elevation, typography, focus ring
SkyDensityTokens.axaml         →  Comfortable padding/min-heights (compact via runtime override)
        ↓
SkySpotifyLegacyResources      →  Spotify* brushes/fonts (backward compatible)
        ↓
SkyPreset.Primitives + SkyPreset.Focus  →  Control styles; :focus-visible rings (WCAG 2.4.7)
```

Load order is defined in `SkyResources.Themed.axaml`, included by **`SkyPreset.axaml`** (public Sky theme).

Set `Application.RequestedThemeVariant` to `Dark`, `Light`, or `HighContrast` to swap full brush sets.

## C# API

| Type | Purpose |
|------|---------|
| `SkyPaletteKeys` | Resource key names for palette slots |
| `SkyTokenKeys` | Resource key names for semantic tokens |
| `SkyTokenUris` | `avares://` URI for `SkyTokens.axaml` |
| `SkyPresetUris` | `SkyPresetUris.Theme` — preset; use `RequestedThemeVariant` for Light / HighContrast |
| `SkyThemeClasses` | Style class names (`sky`, `sky-primary`, …) |
| `SkyThemeUris` | App style includes (`PresetDark`, `SkyData`, …) |
| `ISkyColorPalette` | Strategy contract; `SkyDarkColorPalette`, `SkyLightColorPalette`, `SkyHighContrastColorPalette` |
| `SkyContrast` | WCAG contrast helpers (used by palette unit tests) |
| `SkyThemeVariants` | `SkyThemeVariants.HighContrast` custom variant for `RequestedThemeVariant` |
| `SkyDensity` / `SkyDensityKeys` | Comfortable vs compact control metrics (`SkyDensityTokens.axaml`) |
| `SkyResourceKeys` | Obsolete-friendly aliases over `SkyTokenKeys.Brush` |

## XAML

**Recommended (preset only):**

```xml
xmlns:sky="https://skyui.dev"
<Application sky:SkyThemeProperties.AccentOverride="#1ED760">
  <Application.Styles>
    <StyleInclude Source="avares://SkyUI.Themes.Sky/Themes/SkyTheme.axaml" />
  </Application.Styles>
</Application>
```

**With data controls (grid, filter):**

```xml
<StyleInclude Source="avares://SkyUI.Data/Themes/SkyTheme.WithData.axaml" />
```

**Code (accent after load):**

```csharp
SkyTheme.Apply(application, new SkyThemeOptions { AccentColor = Color.Parse("#1ED760") });
SkyTheme.Apply(application, new SkyThemeOptions { Density = SkyDensity.Compact });
```

Set `sky:SkyThemeProperties.Density="Compact"` on `Application` (XAML) to switch without code.

Constants: `SkyTheme.IncludeUri`, `SkyThemeUris.Theme`, `SkyThemeUris.ThemeWithData`.

**On controls:** `Classes="sky sky-primary"` (legacy `spotify` / `spotify-primary` still styled).

**Resources:** `{DynamicResource SkyTextPrimaryBrush}` (legacy `{DynamicResource SpotifyTextPrimaryBrush}` still works).

## Typography (`SkyUI.Fonts`)

1. Register fonts before the app starts:

```csharp
AppBuilder.Configure<App>()
    .ConfigureSkyFonts(); // Inter + embedded Noto Sans SC
```

2. Theme preset merges `SkyTypographyResources.axaml` (Inter / Noto stacks on `SkyFontFamilyUi` / `SkyFontFamilyTitle`).
3. Use role classes: `sky-section-title`, `sky-body`, `sky-button-label-upper`, `sky-nav-link-bold`, etc. (see `SkyTypographyClasses`).


1. Add or adjust `SkyPalette.*.axaml` with the same `SkyPaletteKeys` slots (keep `SkyPaletteContrastTests` passing).
2. Register the palette in `SkyResources.Themed.axaml` `ThemeDictionaries` if adding a new variant.
3. Extend semantics in `SkyTokens.axaml` only when adding new cross-theme roles.

Visual reference for the Dark preset values: `src/SkyUI.Themes.Sky/DESIGN.md` (content-first dark spec).
