# Legacy Spotify alias sunset plan

SkyUI 0.x keeps **Spotify-era** resource keys and style classes working via shims. This document defines the migration path and removal schedule for SkyUI 1.x.

## What is deprecated

| Legacy | Replacement | Remove in |
|--------|-------------|-----------|
| `Spotify*Brush` resource keys | `Sky*Brush` / `SkyPalette*` | 1.0 |
| `Classes="spotify"` | `Classes="sky"` | 1.0 |
| `spotify-primary`, `spotify-card`, etc. | `sky-primary`, `sky-card`, etc. | 1.0 |
| `avares://…/Themes/Sky/` shim URIs | `avares://SkyUI.Themes.Sky/Themes/SkyDark/` | 1.1 |
| `SkyResourceKeys.SpotifyAccentGreen` | `SkyTokenKeys.Brush.Accent` | 1.0 |
| `SkyThemeClasses.LegacySpotify` | `SkyThemeClasses` | 1.0 |

Shim files remain in 0.x releases so existing apps continue to build without changes.

## Migration checklist

1. **Style classes** — Replace `spotify` with `sky` on buttons, text fields, lists, and cards.
2. **Dynamic resources** — Replace `{DynamicResource SpotifyTextPrimaryBrush}` with `{DynamicResource SkyTextPrimaryBrush}`.
3. **Theme URIs** — Point `StyleInclude` at `avares://SkyUI.Themes.Sky/Themes/SkyTheme.axaml` (not `Themes/Sky/SkyTheme.axaml`).
4. **Accent override** — Use `sky:SkyThemeProperties.AccentOverride` instead of writing `SpotifyAccentGreen*` brushes manually.
5. **C# keys** — Use `SkyTokenKeys` and `SkyPaletteKeys` instead of `SkyResourceKeys.Spotify*`.

## Timeline

| Release | Policy |
|---------|--------|
| **0.x** | Legacy aliases fully supported; `[Obsolete]` on selected C# keys |
| **1.0** | Remove `Spotify*` brush aliases and `spotify*` style selectors from default preset |
| **1.0** | Provide `SkyUI.Legacy` optional package for one release cycle if needed |
| **1.1** | Remove `Themes/Sky/` URI shims |

## Analyzer guidance (future)

A Roslyn analyzer (`SKYUI001`) will flag `Spotify` resource references and `spotify` classes in XAML. Planned for 0.2.

## Related docs

- [Rename strategy](rename-strategy.md)
- [Design tokens](design-tokens.md)
