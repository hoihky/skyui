---
title: Gap analysis
order: 20
---

This document compares the [current state](current-state.md) of SkyUI against expectations for a **commercial-grade, multiplatform UI toolkit**. Benchmarks are used conceptually: enterprise desktop suites (Telerik, DevExpress, Syncfusion), design systems (Fluent UI, Material), and cross-platform frameworks (Qt Quick Controls, Flutter).

## Executive summary

SkyUI is a **young Avalonia control library** with a **cohesive dark theme** and **several advanced, well-architected controls** (virtual grid, filter editor, diagram, tree checklist, video timeline). It is **not yet** a full commercial toolkit: missing product infrastructure (tests, CI, NuGet, API docs), **unproven mobile and web targets**, and a **narrow essential control catalog** relative to line-of-business needs.

A credible commercial position: **Avalonia multiplatform + Sky design system + data/diagram/media Pro tier**, rather than a custom rendering stack.

---

## 1. Platform and adaptive UX

| Gap | Detail |
|-----|--------|
| No validated mobile/web | Library depends on Avalonia core only; demo uses `Avalonia.Desktop`. Touch, safe areas, keyboard insets, and browser constraints are unproven. |
| No adaptive primitives | `DESIGN.md` defines breakpoints and collapsing sidebar/nav behavior; the library does not expose `SkyBreakpoint`, compact density, or bottom-nav vs sidebar patterns. |
| No platform services layer | Commercial kits often wrap file pickers, share sheets, biometrics, haptics, and deep links with consistent APIs. |

**Impact:** Cannot credibly market as “multiplatform” until build matrix, sample apps, and input policies exist for at least iOS, Android, and browser (WASM).

---

## 2. Component catalog breadth

SkyUI is **strong in data/diagram/media** and **thin on everyday app UI**.

### Missing or incomplete (typical commercial expectation)

| Category | Examples |
|----------|----------|
| Shell | Navigation view, tab view, breadcrumb, command bar, status bar, split view templates |
| Feedback | Modal/dialog host, snackbar/toast, banner, inline alert, skeleton loading, empty states |
| Forms | Form field (label, hint, validation, error), radio group, slider, numeric up/down, password/search field, autocomplete |
| Date/time | Calendar, date picker, date range, time picker |
| Menus | Context menu, menubar, overflow — Sky-styled and keyboard-complete |
| Data display | Themed virtual tree view, cards list, KPI tiles |
| Media / viz | Image carousel, charts, gauges (often required in suites; may be phased or partnered) |

Many primitives are **styled Avalonia controls** in the Spotify theme but lack a **unified Sky API** (`Sky*` controls or documented `sky` style contract).

**Impact:** Adopters must assemble app chrome from raw Avalonia or fork theme XAML.

---

## 3. Data grid and filtering

### SkyVirtualDataGrid — gaps

| Capability | Current | Commercial expectation |
|------------|---------|----------------------|
| Column sizing | Fixed pixel width | Resize, auto-fit, star/proportional columns |
| Selection | Single `SelectedRowIndex` | Multi-select, checkbox column, select-all |
| Filtering | External only | In-grid filter row, header filters; integration with `FilterEditor` |
| Grouping / summaries | None | Group rows, aggregates |
| Clipboard / fill | None | Copy/paste, fill down (enterprise) |
| Export | CSV | Excel, PDF; extensible exporter pipeline |
| Async data | Sync `GetRow` | Paging, cancellation, documented threading |
| Accessibility | Not audited | Grid navigation, screen reader names, focus model |

### FilterEditor — gaps

| Gap |
|-----|
| Save/load filter definitions (e.g. JSON), named views |
| Type-aware value editors (date, enum, numeric ranges) |
| Parameterized SQL (injection-safe exporters per dialect) |
| First-class integration samples with grid and query layer |
| Validation and empty/incomplete condition UX |

**Impact:** Existing modules are a **moat** if extended; without grid 2.0 and filter integration, they remain demo-grade.

---

## 4. Diagram module

| Present | Missing |
|---------|---------|
| Model, selection, move/resize, connect/reconnect, copy/paste | Orthogonal and bezier routers (only `StraightEdgePathComputer` today) |
| Pluggable `IEdgePathComputer` | Zoom/pan, snap grid, alignment guides |
| | Undo/redo command stack |
| | Multi-select marquee, grouping/subgraphs, layers |
| | Edge labels, minimap |
| | Export SVG/PNG, print |
| | Touch: pinch-zoom, long-press connect |

**Impact:** Suitable for early flow editors; not yet competitive with dedicated diagram SKUs.

---

## 5. Design system and theming

| Gap | Detail |
|-----|--------|
| Token model | `Spotify*` resource keys; no semantic `SkyToken` layer decoupled from one brand |
| Themes | Dark only in practice; no light, high contrast, or forced-colors story |
| Runtime branding | No documented accent override or theme builder API |
| Icons | No shared icon font or SVG set in the package |
| Motion | No shared transition/animation guidelines in code |
| Density | Comfortable / compact via `SkyDensity` and `SkyDensityTokens.axaml` |
| Legal / brand | Spotify-inspired DESIGN.md and naming may limit commercial positioning; need Sky-native design language |

**Impact:** Enterprise buyers expect white-label theming and accessibility modes.

---

## 6. Quality, developer experience, and go-to-market

| Gap | Impact |
|-----|--------|
| No unit/headless UI tests | High regression risk on virtualization and pointer input |
| No CI (build, test, format, analyzers) | Blocks team scaling and contributor trust |
| No API reference / doc site | Slow adoption |
| No semver policy, changelog, deprecation | Blocks enterprise procurement |
| No NuGet publish pipeline | No distribution |
| No visual regression tests | Theme and layout breaks undetected |
| No WCAG conformance statement | Blocks public sector and strict vendors |
| No localization (RTL, resources) | Blocks global products |
| Weak getting started | Root README is minimal |

---

## 7. Legal and positioning

| Topic | Notes |
|-------|--------|
| License | MIT on project code is compatible with commercial use of **your** code. |
| Legal / brand | Sky-branded public API (`sky`, `Sky*`); ContentFirstDark preset; legacy Spotify paths aliased — see [rename-strategy.md](rename-strategy.md) |
| Third-party fonts | Use bundled open fonts (e.g. Inter, Noto) for shipping products. |

---

## Priority matrix

| Priority | Item | Rationale |
|----------|------|-----------|
| P0 | CI, tests, NuGet, README | Enables all other work |
| P0 | Design tokens + light/high-contrast themes | Branding, accessibility, sales |
| P1 | Navigation shell, dialogs, snackbar | Required for real apps |
| P1 | Date/time pickers, form field wrapper | LOB blocker |
| P1 | Mobile/browser demo + adaptive navigation | Validates multiplatform claim |
| P2 | Grid 2.0 + filter integration | Extends existing strength |
| P2 | Themed virtual tree view | Complements `CheckedListBox` |
| P3 | Diagram zoom/undo/routers | Completes differentiator |
| P3 | Charts | High demand; consider embed or partner initially |

---

## Risks and mitigations

| Risk | Mitigation |
|------|------------|
| Avalonia API churn | Pin versions; headless integration tests; track release notes |
| WASM bundle size | Optional packages (`SkyUI.Data`, `SkyUI.Diagram`), trimming, lazy load |
| Scope vs large vendors | Focus Pro tier on data + diagram + timeline; essentials via disciplined styling |
| Virtualization bugs | Headless scroll tests; invariant tests for row index mapping |
| SQL injection in filters | Document safe exporters; parameterized queries only in official exporters |

---

## Related documents

- [Current state](current-state.md)
- [Development roadmap](development-roadmap.md)
