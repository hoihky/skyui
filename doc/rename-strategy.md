# Rename strategy (Sky brand)

## Goals

1. **Public brand** is **Sky** — style classes (`sky`), semantic brushes (`Sky*`), packages (`SkyUI.*`).
2. **Internal paths** describe what they are, not a third-party product.
3. **Backward compatibility** — old `Themes/Sky/` URIs and `Spotify*` resource keys keep working via shims.

## Layout (current)

```text
SkyUI.Themes.Sky/
  Themes/
    SkyTheme.axaml                    ← public entry (unchanged URI)
    SkyDark/                          ← palettes, resources, control templates
      SkyPalette.{Dark,Light,HighContrast}.axaml
      SkyResources.*
      SkyLegacyBrushAliases.axaml     ← was SkySpotifyLegacyResources
      Controls/
      Presets/
        ContentFirstDark/
          ContentFirstDark.axaml      ← default visual preset
          SkyPreset.Primitives.axaml
          SkyPreset.Focus.axaml
    Sky/                              ← legacy redirects only (do not add new files here)
```

## Preset ID

| ID | URI constant | Description |
|----|--------------|-------------|
| `ContentFirstDark` | `SkyPresetUris.ContentFirstDark` | Default immersive dark UI (pill geometry, content-first surfaces) |

`SkyPresetUris.Theme` and `SkyTheme.IncludeUri` still point at `Themes/SkyTheme.axaml`, which loads ContentFirstDark.

## What to use in new code

| Prefer | Avoid |
|--------|--------|
| `Classes="sky"` | `Classes="spotify"` |
| `{DynamicResource SkyAccentBrush}` | `{DynamicResource SpotifyAccentGreenBrush}` |
| `SkyThemeUris.Theme` | Hard-coded `Themes/Sky/…` paths |
| `SkyPresetUris.ContentFirstDark` | `SkyPresetUris.Dark` (obsolete alias) |
| `Themes/SkyDark/…` when referencing palette files | `Themes/Sky/…` (shim only) |

## Legacy compatibility

- **`Themes/Sky/**`** — thin XAML shims that `ResourceInclude` / `StyleInclude` the `SkyDark` or `ContentFirstDark` targets.
- **`Spotify*` brushes** — defined in `SkyLegacyBrushAliases.axaml`; shim file `SkySpotifyLegacyResources.axaml` remains under `Themes/Sky/`.
- **`spotify*` style classes** — still matched in `SkyPreset.Primitives.axaml` alongside `sky*`.
- **`SkyThemeClasses.LegacySpotify`** — documents deprecated class names.

## Future presets

Add siblings under `Themes/SkyDark/Presets/` (e.g. `ContentFirstLight`) and new `SkyPresetIds` constants. Keep `Themes/SkyTheme.axaml` as the default bundle or expose separate includes per preset.

## DESIGN.md

`src/SkyUI.Themes.Sky/DESIGN.md` remains an **internal visual reference** for ContentFirstDark (historically Spotify-inspired). It is not user-facing product branding.
