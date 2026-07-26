# Current state

Snapshot of the SkyUI codebase used as the baseline for [gap analysis](./gap-analysis.md) and the [development roadmap](./development-roadmap.md).

## Stack and packaging

| Area | Status |
|------|--------|
| Solution | `SkyUI.slnx` at repository root |
| UI framework | Avalonia 11.3.12 (`Directory.Build.props` pins `AvaloniaVersion`) |
| Target framework | .NET 10 (`net10.0`) |
| Packages | See [repository layout](#repository-layout) below |
| Demo app | `src/SkyUI.Demo` — desktop gallery referencing all packages |
| Automated tests | `tests/SkyUI.UnitTests`, `tests/SkyUI.HeadlessTests` |
| Public API docs | `doc/`; visual preset spec in `src/SkyUI.Themes.Sky/DESIGN.md` |
| License | MIT (`LICENSE`) |
| XAML xmlns | `https://skyui.dev` (split per assembly via `XmlnsDefinition`) |

## Repository layout

```text
skyui/
  SkyUI.slnx
  Directory.Build.props
  src/
    SkyUI.Core/           Tokens, theme attached properties
    SkyUI.Icons/          Icon identifiers (assets to follow)
    SkyUI/                Essentials controls + SkyThemeUris hooks
    SkyUI.Themes.Sky/     Default dark theme (Sky preset) + primitive styles
    SkyUI.Data/           Virtual grid + filter editor + SkyDataTheme.axaml
    SkyUI.Diagram/        Diagram surface and routers
    SkyUI.Demo/           Control gallery
  tests/
    SkyUI.UnitTests/
    SkyUI.HeadlessTests/
  doc/
```

### Package dependencies

```text
SkyUI.Core
SkyUI.Icons
SkyUI          → Core, Icons
SkyUI.Themes.Sky → Core, SkyUI (templates target essentials controls)
SkyUI.Data     → Core, SkyUI
SkyUI.Diagram  → Core, SkyUI
```

Apps typically reference `SkyUI`, `SkyUI.Themes.Sky`, and optionally `SkyUI.Data` / `SkyUI.Diagram`. Include styles:

- `avares://SkyUI.Themes.Sky/Themes/Sky/SkyTheme.axaml` (or `SkyUI.Theme.SkyThemeUris.Sky`)
- `avares://SkyUI.Data/Themes/SkyDataTheme.axaml` when using data controls

## Design system

- **Theme package**: `src/SkyUI.Themes.Sky/Themes/Sky/` — `SkyResources.axaml`, `SkyTheme.axaml`, control templates for essentials.
- **Data theme**: `src/SkyUI.Data/Themes/SkyDataTheme.axaml` — grid and filter templates (optional split for smaller browser bundles).

## Custom controls (library)

| Control | Namespace | Package |
|---------|-----------|---------|
| `SkyVirtualDataGrid` | `SkyUI.DataGrid` | `SkyUI.Data` |
| `FilterEditor` | `SkyUI.FilterEditor` | `SkyUI.Data` |
| `DiagramSurface` | `SkyUI.Diagram.Presentation` | `SkyUI.Diagram` |
| `CheckedListBox` | `SkyUI.Controls` | `SkyUI` |
| `VideoTimeline` | `SkyUI.Controls` | `SkyUI` |
| `SkyAccordion` / `SkyAccordionItem` | `SkyUI.Controls` | `SkyUI` |
| `Avatar`, `Badge`, `Chip` | `SkyUI.Controls` | `SkyUI` |
| `SkyPlaceholderControl` | `SkyUI.Controls` | `SkyUI` |

## Data grid capabilities (today)

- Data via `IVirtualGridDataSource` (windowed `GetRow`, `RowCount`, `StructureChanged`, optional `ApplySort`).
- Columns: `SkyDataGridColumn` — header, fixed width, binding path, read-only, sort direction, optional cell template.
- Events: sorting, row formatting, column/row reorder, cell edit committed.
- Export: `ExportToCsvAsync` via `SkyDataGridCsvExporter`.

## Filter editor capabilities (today)

- Logical groups and conditions (`FilterDocument`, `FilterGroupNode`, `FilterConditionNode`).
- Compare operators: equal, not equal, ordering, contains/starts/ends with, is null / is not null.
- Field descriptors and data kinds; `BasicFilterSqlExporter` and custom `IFilterSqlExporter`.

## Diagram module (today)

- Contracts: `IDiagramModel`, `IDiagramNode`, `IDiagramEdge`, `IDiagramPort`, `IDiagramSelection`, `IEdgePathComputer`, `IDiagramNodePresenterFactory`, `IDiagramSceneHitTester`.
- Default straight routing: `StraightEdgePathComputer` (orthogonal/spline intended as replacements).
- Interaction on `DiagramSurface`: move, resize, connect, reconnect, clipboard.

## Demo gallery pages

Overview, Buttons, Avatar, Chip, Badge, Text field, Checkbox & Switch, Select, List, Accordion, Placeholder, Diagram, CheckedListBox, Filter editor, Virtual DataGrid, Video timeline.

## Architectural strengths

- **Pluggable boundaries** in Diagram and Filter (routers, exporters, presenters, hit testing).
- **Virtual data** pattern suitable for large or server-backed grids.
- **Cohesive dark theme** for primitives and custom templates.
- **XmlnsDefinition** for product-style XAML consumption.

## Known limitations (baseline)

- Desktop-only demo; multiplatform not validated.
- No automated quality gates or published NuGet package.
- Thin coverage of app chrome (navigation shell, dialogs, toasts, pickers, charts).
- Grid lacks many enterprise features (resize, multi-select, grouping, in-grid filter, etc.).
- Branding tied to “Spotify” naming in resources and design doc — consider product-neutral tokens for commercial positioning.
