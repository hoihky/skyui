---
title: Sky design tokens
order: 40
---

SkyUI separates **palette** (theme-specific colors), **semantic tokens** (stable public names), **visual presets** (control chrome), and **legacy aliases** (pre-rename resource keys and style classes).

## Naming (public brand: Sky)

| Layer | Path / ID | Purpose |
|-------|-----------|---------|
| **Brand** | `sky` style classes, `Sky*` brushes | Public API — use in apps and docs |
| **Theme resources** | `Themes/SkyDark/` | Palettes, themed dictionaries, control templates |
| **Default preset** | `Presets/ContentFirstDark/` (`SkyPresetIds.ContentFirstDark`) | Content-first dark primitives (pill buttons, immersive surfaces) |
| **Legacy shims** | `Themes/Sky/` | Redirects old `avares://…/Themes/Sky/…` URIs |
| **Legacy brushes** | `SkyLegacyBrushAliases.axaml` | `Spotify*` keys → `SkyPalette*` (deprecated) |
| **Legacy classes** | `spotify*` | Aliased in primitives; use `sky*` in new code |

See [rename-strategy.md](rename-strategy.md) for migration notes.

## Layering

```text
Themes/SkyDark/SkyPalette.{Dark|Light|HighContrast}.axaml  →  SkyPalette* slots
        ↓
SkyResources.Themed.axaml + SkyResources.Variant.axaml
        ↓
SkyTokens.axaml + SkyDensityTokens.axaml  →  Sky* semantic brushes, spacing, density
        ↓
SkyLegacyBrushAliases.axaml  →  Spotify* brush aliases (backward compatible)
        ↓
Presets/ContentFirstDark/SkyPreset.Primitives + Focus  →  Control styles (WCAG focus rings)
```

Load order is in `SkyResources.Themed.axaml`, included by **`ContentFirstDark.axaml`** (default via `Themes/SkyTheme.axaml`).

Set `Application.RequestedThemeVariant` to `Dark`, `Light`, or `HighContrast` to swap palette brush sets.

## C# API

| Type | Purpose |
|------|---------|
| `SkyPresetIds` | Preset identifiers (`ContentFirstDark`) |
| `SkyPaletteKeys` | Resource key names for palette slots |
| `SkyTokenKeys` | Resource key names for semantic tokens |
| `SkyTokenUris` | `avares://` URI for `SkyTokens.axaml` |
| `SkyPresetUris` | `Theme` (default), `ContentFirstDark`; use `RequestedThemeVariant` for Light / HC |
| `SkyThemeClasses` | Style class names (`sky`, `sky-primary`, …) |
| `SkyThemeUris` | App style includes (`Theme`, `ContentFirstDark`, `ThemeWithData`, …) |
| `ISkyColorPalette` | Strategy contract; `SkyDarkColorPalette`, `SkyLightColorPalette`, `SkyHighContrastColorPalette` |
| `SkyContrast` | WCAG contrast helpers (used by palette unit tests) |
| `SkyThemeVariants` | `SkyThemeVariants.HighContrast` custom variant for `RequestedThemeVariant` |
| `SkyDensity` / `SkyDensityKeys` | Comfortable vs compact control metrics |
| `SkyResourceKeys` | Obsolete-friendly aliases over `SkyTokenKeys.Brush` |

## XAML

**Recommended (default Sky theme):**

```xml
xmlns:sky="https://skyui.dev"
<Application sky:SkyThemeProperties.AccentOverride="#1ED760">
  <Application.Styles>
    <StyleInclude Source="avares://SkyUI.Themes.Sky/Themes/SkyTheme.axaml" />
  </Application.Styles>
</Application>
```

**Explicit preset include:**

```xml
<StyleInclude Source="avares://SkyUI.Themes.Sky/Themes/SkyDark/Presets/ContentFirstDark/ContentFirstDark.axaml" />
```

**With data controls (grid, filter):**

```xml
<StyleInclude Source="avares://SkyUI.Data/Themes/SkyTheme.WithData.axaml" />
```

**Code:**

```csharp
SkyTheme.Apply(application, new SkyThemeOptions { AccentColor = Color.Parse("#1ED760") });
SkyTheme.Apply(application, new SkyThemeOptions { Density = SkyDensity.Compact });
```

Constants: `SkyTheme.IncludeUri`, `SkyPresetUris.ContentFirstDark`, `SkyThemeUris.Theme`, `SkyThemeUris.ThemeWithData`.

**On controls:** `Classes="sky sky-primary"` (legacy `spotify` classes still styled).

**Resources:** `{DynamicResource SkyTextPrimaryBrush}` (legacy `{DynamicResource SpotifyTextPrimaryBrush}` still works).

## Typography (`SkyUI.Fonts`)

1. Register fonts before the app starts:

```csharp
AppBuilder.Configure<App>()
    .ConfigureSkyFonts(); // Inter + embedded Noto Sans SC
```

2. Theme preset merges `SkyTypographyResources.axaml` (Inter / Noto stacks on `SkyFontFamilyUi` / `SkyFontFamilyTitle`).
3. Use role classes: `sky-section-title`, `sky-body`, `sky-button-label-upper`, `sky-nav-link-bold`, etc. (see `SkyTypographyClasses`).

## Extending palettes

1. Add or adjust `Themes/SkyDark/SkyPalette.*.axaml` with the same `SkyPaletteKeys` slots (keep `SkyPaletteContrastTests` passing).
2. Register the palette in `SkyResources.Themed.axaml` `ThemeDictionaries` if adding a new variant.
3. Extend semantics in `SkyTokens.axaml` only when adding new cross-theme roles.

Visual reference for the ContentFirstDark preset: `src/SkyUI.Themes.Sky/DESIGN.md` (internal content-first dark spec).
