# SkyUI

**SkyUI** is a cross-platform UI library for [.NET](https://dotnet.microsoft.com/) built on [Avalonia](https://avaloniaui.net/). It provides a cohesive design system, a growing catalog of styled controls, and optional data and diagram modules for line-of-business and productivity apps.

The default **ContentFirstDark** preset is a content-first dark theme: near-black surfaces, semantic tokens, and a functional accent color so your data—not chrome—stays in focus. Light and high-contrast palettes are included.

## What you get

| Layer | Packages | Purpose |
|-------|----------|---------|
| **Design system** | `SkyUI.Core`, `SkyUI.Themes.Sky`, `SkyUI.Fonts`, `SkyUI.Icons` | Semantic tokens, dark/light/HC palettes, typography, icons, density |
| **Essentials** | `SkyUI` | Buttons, forms, navigation, menus, pickers, lists, layout, feedback, and more |
| **Data** | `SkyUI.Data` | Virtual data grid, filter editor |
| **Diagram** | `SkyUI.Diagram` | Diagram surface with pluggable edge routing |

Controls are consumed as `Sky*` types (for example `SkyCard`, `SkyDatePicker`, `SkyNavigationView`) or as Avalonia primitives with `Classes="sky"`. XAML xmlns: `https://skyui.dev`.

### Essentials catalog (highlights)

- **Primitives** — tooltips, popovers, loading buttons, consistent disabled states
- **Forms** — `SkyFormField`, validation, radio groups, sliders, search/password inputs
- **Navigation** — `SkyNavigationView`, tabs, breadcrumbs, responsive breakpoints
- **Menus** — menu bar, context menu, flyouts, keyboard accelerators
- **Pickers** — date/time pickers, calendar, culture-aware formatting
- **Lists** — virtual tree view with optional checkboxes
- **Layout** — cards, dividers, expanders, responsive grids
- **Feedback** — dialogs, snackbars, banners, progress ring, skeleton

See the interactive gallery in `SkyUI.Demo` for every control and variant.

## Requirements

- .NET 10 (`net10.0`)
- Avalonia 12.1.x

## Quick start

Add project references (or future NuGet packages) for the assemblies you need, then include the theme in `App.axaml`:

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:sky="https://skyui.dev"
             x:Class="MyApp.App"
             RequestedThemeVariant="Dark">

    <Application.Styles>
        <FluentTheme />
        <StyleInclude Source="avares://SkyUI.Themes.Sky/Themes/SkyTheme.axaml" />
    </Application.Styles>
</Application>
```

Use a control:

```xml
<sky:SkyCard Header="Settings" IsHoverable="True">
    <Button Classes="sky sky-primary" Content="Save" />
</sky:SkyCard>
```

Optional accent override:

```xml
<Application sky:SkyThemeProperties.AccentOverride="#1ED760" ...>
```

For data controls, swap the style include:

```xml
<StyleInclude Source="avares://SkyUI.Data/Themes/SkyTheme.WithData.axaml" />
```

Constants are also available in code: `SkyThemeUris.Theme`, `SkyThemeUris.ThemeWithData`.

## Build and run

```bash
dotnet build SkyUI.slnx
dotnet run --project src/SkyUI.Demo
```

Run tests:

```bash
dotnet test
```

## Repository layout

```text
src/
  SkyUI.Core/           Tokens, theme attached properties
  SkyUI/                Essentials controls
  SkyUI.Themes.Sky/     Default Sky theme (Dark / Light / HighContrast)
  SkyUI.Fonts/          Inter + Noto Sans SC typography
  SkyUI.Icons/          Icon set and button icon helpers
  SkyUI.Data/           Virtual grid + filter editor
  SkyUI.Diagram/        Diagram surface
  SkyUI.Demo/           Control gallery
tests/
  SkyUI.UnitTests/
  SkyUI.HeadlessTests/
doc/                    Product docs, roadmap, control guides
```

## Documentation

| Document | Description |
|----------|-------------|
| [doc/README.md](doc/README.md) | Documentation index |
| [doc/current-state.md](doc/current-state.md) | Repository inventory |
| [doc/development-roadmap.md](doc/development-roadmap.md) | Phased product plan |
| [doc/design-tokens.md](doc/design-tokens.md) | Token layer and theming API |
| [src/SkyUI.Themes.Sky/DESIGN.md](src/SkyUI.Themes.Sky/DESIGN.md) | Visual specification (ContentFirstDark) |

Control guides (menus, pickers, layout, primitives, and more) live under `doc/`.

## License

MIT — see [LICENSE](LICENSE).
