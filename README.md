> **Disclaimer:** This project is an experimental, work-in-progress prototype built with the help of "vibe coding". Things will break. Features are currently missing, and the build scripts might not work at all. Please be aware that it may not be stable enough for production use now.

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
- **Mobile (early)** — safe area, keyboard inset, touch targets, action sheet, bottom sheet host

See the interactive gallery in `SkyUI.Demo` for every control and variant (including a **Mobile** page). Sample iOS host: `SkyUI.Demo.iOS` (`net10.0-ios`).

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

## Local NuGet packages and sample apps

SkyUI libraries can be packed to a **local feed** (not published to nuget.org):

```bash
./scripts/pack-local.sh
```

Packages are written to `artifacts/packages` at version **0.1.0-local**. The root `nuget.config` registers this folder as the `skyui-local` source.

Reference apps that consume SkyUI via `PackageReference`:

| Sample | Demonstrates |
|--------|----------------|
| `samples/SettingsApp` | Navigation shell, form fields, validators, theme/density/accent, snackbar, JSON settings |
| `samples/CrudListDetail` | Master-detail layout, virtual grid, repository pattern, CRUD MVVM |
| `samples/ThemeBuilderApp` | Live theme builder: variant, density, accent without copying theme XAML |

```bash
dotnet run --project samples/SettingsApp
dotnet run --project samples/CrudListDetail
dotnet run --project samples/ThemeBuilderApp
```

See [samples/README.md](samples/README.md) for architecture notes (MVVM, DI, SOLID).

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
  SkyUI.Demo.Mobile/    Shared mobile demo shell
  SkyUI.Demo.iOS/       iOS host (net10.0-ios)
tests/
  SkyUI.UnitTests/
  SkyUI.HeadlessTests/
samples/
  SettingsApp/          Settings shell (NuGet-consuming reference app)
  CrudListDetail/       List-detail CRUD (NuGet-consuming reference app)
  SampleInfrastructure/ Shared MVVM primitives for samples
docs/                   Product docs, roadmap, control guides
```

## Documentation

| Document | Description |
|----------|-------------|
| [docs/index.html](docs/index.html) | SDK introduction — architecture and programming guide |
| [docs/README.md](docs/README.md) | Documentation index |
| [docs/pages/current-state.html](docs/pages/current-state.html) | Repository inventory |
| [docs/pages/development-roadmap.html](docs/pages/development-roadmap.html) | Phased product plan |
| [docs/pages/design-tokens.html](docs/pages/design-tokens.html) | Token layer and theming API |
| [src/SkyUI.Themes.Sky/DESIGN.md](src/SkyUI.Themes.Sky/DESIGN.md) | Visual specification (ContentFirstDark) |

Control guides (menus, pickers, layout, primitives, and more) live under `docs/pages/`.

## License

MIT — see [LICENSE](LICENSE).
