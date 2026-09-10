---
title: Development roadmap
order: 30
---

Phased plan to evolve SkyUI from the [current state](current-state.md) toward a **production-ready toolkit for realistic desktop and mobile applications**, addressing gaps in [gap-analysis.md](gap-analysis.md).

Durations assume a small core team; workstreams within a phase can run in parallel where noted.

## Product vision

| Pillar | Description |
|--------|-------------|
| **Sky Design System** | Semantic tokens, themes (dark/light/high contrast), icons, spacing, typography, motion |
| **Sky Essentials** | Broad control catalog and app shell patterns with consistent API and documentation |
| **Sky Data / Pro** | Virtual grid, filter builder, tree checklist, diagram, scheduling/timeline |
| **Sky Platform** | Desktop, iOS, Android, and browser validated with touch and adaptive layout |

**Positioning:** Cross-platform .NET apps that need **data-heavy UI**, **diagram/media** tooling, and **credible mobile UX** — built on Avalonia.

---

## Progress snapshot

SkyUI has moved beyond an early prototype. The roadmap below is **forward-looking** from this baseline.

### Largely complete

| Area | Status |
|------|--------|
| **Design system** | Semantic tokens, Dark / Light / High contrast, density, accent override, Inter + Noto fonts, icon set |
| **Navigation shell** | `SkyNavigationView` (expanded / compact / bottom), `SkyTabView`, `SkyBreadcrumb` |
| **Forms (core)** | `SkyFormField`, validators, search/password, radio group, slider |
| **Menus** | `SkyMenuBar`, `SkyContextMenu`, `SkyMenuFlyout`, accelerators |
| **Pickers** | `SkyDatePicker`, `SkyCalendar`, `SkyTimePicker`, culture formatting |
| **Feedback** | `SkyDialogHost`, `SkySnackbarHost`, `SkyAlert`, `SkyBanner`, progress, skeleton |
| **Layout** | `SkyCard`, `SkyDivider`, `SkyExpander`, `SkyResponsiveGrid`, breakpoints |
| **Lists** | `CheckedListBox`, `SkyVirtualTreeView` |
| **Primitives** | `SkyTooltip`, `SkyPopover`, loading button state |
| **Data / diagram (v1)** | `SkyVirtualDataGrid`, `FilterEditor`, `DiagramSurface`, `VideoTimeline` |
| **Tests** | Unit + headless suites (~190+ tests) |
| **Docs** | Static site, SDK guide, control guides |

### Still blocking realistic apps

| Gap | Why it matters |
|-----|----------------|
| **No CI / NuGet publish** | Cannot ship or adopt as a dependency in production pipelines |
| **Thin app chrome** | No command bar, split view, drawer, status bar, or master-detail templates |
| **Forms gaps** | No autocomplete, numeric up/down, date range, masked input, validation summary |
| **Grid / filter v1 limits** | No resize, multi-select, in-grid filter, grouping, async paging — LOB apps stall here |
| **Mobile unproven** | No safe area, sheets, touch policy, keyboard insets, or mobile sample app |
| **No reference apps** | Demo gallery ≠ settings app, field-service app, or CRUD shell |

---

## Target package architecture

```text
SkyUI.Core          Tokens, shared interfaces, attached properties
SkyUI               Essentials controls + default theme hooks
SkyUI.Themes.Sky    Default dark/light/high-contrast theme resources
SkyUI.Icons         Icon font or SVG set
SkyUI.Data          Grid, filter (optional split from core)
SkyUI.Diagram       Diagram surface and routers (optional split)
SkyUI.Mobile        Touch, safe area, sheets (optional; may merge into SkyUI initially)
```

Dependency direction: `Core` → `SkyUI` → (`Data`, `Diagram`, `Mobile`); `Themes` and `Icons` plug into `SkyUI`.

---

## Phase 0 — Foundation (4–8 weeks remaining)

**Goal:** Operate SkyUI as a shippable product.

| Workstream | Features | Status |
|------------|----------|--------|
| Repository layout | `SkyUI.slnx`, demo, unit + headless tests | Done |
| CI/CD | Build on PR; `dotnet format`; analyzers; test gate; **GitHub Pages** deploy from `docs/` | **Todo** |
| Packaging | `PackageId`, authors, repository URL, README in package, symbol packages, **0.x semver** | **Partial** (local feed via `scripts/pack-local.sh`; not published to nuget.org) |
| Documentation v1 | README, install guide, control index, screenshots | Partial |
| Engineering policy | CONTRIBUTING, control checklist, breaking-change policy | **Todo** |
| Reference samples | `samples/SettingsApp`, `samples/CrudListDetail` (minimal, NuGet-consuming) | **Done** |

### Exit criteria

- Green CI on every PR; docs site deploys on merge to `main`.
- Publishable NuGet package (even if unlisted initially).
- At least one sample app references SkyUI from NuGet, not project reference. **Done** (`samples/SettingsApp`, `samples/CrudListDetail` via local `skyui-local` feed).

---

## Phase 1 — Design system polish (4–6 weeks)

**Goal:** Finish white-label and motion story; retire legacy naming.

| Feature | Description | Status |
|---------|-------------|--------|
| **Motion tokens** | Shared duration/easing resources; enter/exit for dialogs, snackbars, sheets | **Done** |
| **Forced-colors audit** | High-contrast palette pass on feedback + navigation; focus rings | **Done** |
| **Legacy alias sunset plan** | Document migration from `Spotify*` keys; deprecate in 1.x | **Done** ([legacy-sunset.md](legacy-sunset.md)) |
| **Theme builder sample** | Third-party app sets accent + density without copying XAML | **Done** (`samples/ThemeBuilderApp`) |

### Exit criteria

- Motion applied consistently on dialog, snackbar, sheet, and navigation transitions. **Done**
- WCAG AA contrast verified for text and focus rings on all three palettes. **Done** (unit tests)

---

## Phase 2 — Essentials wave 2: realistic app UI (14–20 weeks)

**Goal:** Close the gap between “control gallery” and **apps users actually ship** — settings, CRM, inventory, field service, admin portals.

Phase 2 (original) navigation, forms, menus, pickers, layout, and feedback controls are **shipped**. This wave adds **missing high-value controls** identified for desktop and shared mobile/desktop patterns.

### P0 — App shell and navigation (desktop + tablet)

| Control / pattern | Description | Desktop | Mobile |
|-------------------|-------------|:-------:|:------:|
| **SkyCommandBar** | Primary toolbar with icon buttons, labels, overflow menu | ✓ | ✓ |
| **SkyStatusBar** | Bottom status line (connection, selection count, progress) | ✓ | — |
| **SkySplitView** | Master-detail pane with collapsible list + detail | ✓ | ✓ |
| **SkyDrawer** | Slide-in panel for filters, settings, nav (overlay on narrow width) | ✓ | ✓ |
| **SkyPageHeader** | Title, subtitle, back button, action slots | ✓ | ✓ |
| **App templates** | `SkySettingsPage`, `SkyListDetailPage`, `SkyFormPage` XAML/C# scaffolds | ✅ | ✅ |

### P0 — Forms and input (LOB blocker)

| Control | Description |
|---------|-------------|
| **SkyAutocomplete** | Typeahead with async `ItemsSource`, free text, FormField integration |
| **SkyComboBoxField** | `SkyFormField` wrapper around styled ComboBox with validation |
| **SkyNumericUpDown** | Decimal/integer with min/max, step, culture formatting |
| **SkyDateRangePicker** | Start/end date with presets (today, this week, custom) |
| **SkyMaskedTextBox** | Phone, credit card, custom mask; pairs with `SkyFormField` |
| **SkyValidationSummary** | Form-level error list; links to first invalid field |
| **SkyEmptyState** | Illustration + title + action for empty lists, grids, search |

### P1 — Data presentation (beyond raw grid)

| Control | Description |
|---------|-------------|
| **SkyDataPager** | Page size, first/prev/next/last, total count; binds to grid or list |
| **SkyPagination** | Generic pagination for any `ItemsSource` |
| **SkyKpiTile** | Metric card (value, delta, sparkline slot) for dashboards |
| **SkyLoadingOverlay** | Semi-transparent blocker over panel/window during async work |

### P1 — Dialogs and system integration

| Feature | Description |
|---------|-------------|
| **SkyMessageBox** | Static `ShowAsync` API: info/warning/error/confirm; maps to `SkyDialogHost` |
| **SkyFilePicker** | Styled wrapper over Avalonia storage pickers; consistent API across platforms |
| **SkyClipboard** | Helper for copy/paste text and tabular data from grid selection |

### P2 — Lists and trees (productivity)

| Feature | Description |
|---------|-------------|
| **Drag-drop reorder** | `CheckedListBox` / `SkyVirtualTreeView` row reorder |
| **Inline edit** | Tree/list cell edit with commit/cancel |
| **Context row actions** | Swipe or overflow menu pattern (shared with mobile) |
| **Async tree loading** | `IAsyncTreeDataSource` with expand-to-load children |

### Per-control definition of done

- Theme AXAML (all interaction states + disabled).
- Demo page in `SkyUI.Demo`.
- Headless or unit tests for open/close, validation, selection.
- Accessibility: name, keyboard, `:focus-visible`.

### Exit criteria

- **Settings sample app** built only from SkyUI packages: nav + form fields + save snackbar.
- **List-detail sample app**: split view + virtual grid or list + detail form.
- Essentials catalog published on doc site with desktop/mobile applicability notes.

---

## Phase 3 — Mobile and adaptive UX (14–18 weeks)

**Goal:** SkyUI works on **phone and tablet**, not only desktop with a narrow window.

`SkyNavigationView` bottom mode and breakpoints exist but are **not validated** on iOS/Android. This phase makes mobile a first-class target.

### P0 — Platform matrix

| Target | Deliverable |
|--------|-------------|
| **TFM matrix** | `net10.0-android`, `net10.0-ios`; browser WASM optional |
| **SkyUI.Mobile.Demo** | Phone + tablet sample (or adaptive single app) |
| **CI builds** | Android + iOS (or at least compile gate); manual test matrix doc |
| **Package splitting** | `SkyUI.Data` / `SkyUI.Diagram` optional for WASM size |

### P0 — Mobile primitives (high value)

| Control / API | Description |
|---------------|-------------|
| **SkySafeArea** | Attached properties: `PaddingTop/Bottom/Left/Right` from device insets |
| **SkyBottomSheet** | Modal sheet from bottom; snap heights; drag-to-dismiss |
| **SkyActionSheet** | Destructive/primary action list (iOS-style) |
| **SkyFab** | Floating action button; position above safe area + bottom nav |
| **SkyPullToRefresh** | Wrapper control for scrollable content |
| **SkyKeyboardInset** | Adjusts bottom padding when soft keyboard opens |
| **SkyTouchTarget** | Enforces minimum 44×44 dp via attached property or style |

### P1 — Touch interaction policy

| Area | Policy |
|------|--------|
| **Hit targets** | All interactive Essentials meet 44 dp on mobile density |
| **Long-press** | Context menu on list/tree rows; connect handle on diagram |
| **Pinch-zoom** | Diagram surface and image viewer |
| **Scroll conflict** | Nested scroll (grid inside nav) documented patterns |
| **Hover vs press** | `:pointerover` styles do not replace pressed state on touch |

### P1 — Adaptive layouts

| Feature | Description |
|---------|-------------|
| **Compact density auto** | Optional auto-switch to `SkyDensity.Compact` below tablet breakpoint |
| **Master-detail adaptive** | Split view on tablet/desktop; navigate push on phone |
| **Sheet vs dialog** | `SkyDialogHost` uses full-screen sheet on narrow width |
| **Typography scale** | Optional larger body/label on mobile |

### P2 — Platform services (thin wrappers)

| Service | Description |
|---------|-------------|
| **ISkyShareService** | Share text/file via platform share sheet |
| **ISkyHapticService** | Light feedback on confirm/delete (mobile) |
| **ISkyBrowserService** | Open URL in system browser |
| **Deep link hooks** | Document Avalonia app link integration (not in-library) |

### Exit criteria

- Field-service **mobile sample**: bottom nav, form, pull-to-refresh list, bottom sheet filters.
- Tablet sample: split view + virtual grid with touch scroll.
- Documented manual test matrix: iOS, Android, Windows, macOS (minimum).

---

## Phase 4 — Data and productivity (20+ weeks, ongoing)

**Goal:** Mid-tier commercial data controls; deepen existing modules.

### SkyVirtualDataGrid 2.0

| Feature | Description | Priority |
|---------|-------------|----------|
| Column resize | Drag handles; auto-fit; star columns | P0 |
| Selection | Multi-select, checkbox column, select all | P0 |
| Async paging | `IAsyncVirtualGridDataSource`; cancellation; loading row | P0 |
| Header UX | Context menu, column chooser, freeze columns | P1 |
| Filtering | Filter row or header filters → `FilterDocument` | P1 |
| Clipboard | Copy/paste TSV; optional fill-down | P1 |
| Grouping | Group rows and summaries | P2 |
| Export | CSV (existing), Excel, PDF via `ISkyDataGridExporter` | P1 |
| Touch | Horizontal scroll, column resize on touch, row select | P1 |
| Accessibility | Full keyboard navigation; automation peers | P1 |

### FilterEditor 2.0

| Feature | Description |
|---------|-------------|
| Persistence | Save/load JSON; named filters |
| Editors | Per-type value UI (date range, enum, numeric between) |
| SQL | Parameterized exporters; dialect plugins |
| Integration | Official sample: filter → grid data source → export |

### Visualization (phased)

| Feature | Description | Approach |
|---------|-------------|----------|
| **SkyChart** | Line, bar, area for dashboards | Lightweight in-house or embed (e.g. LiveCharts2) |
| **SkyGauge** | Radial/linear gauge for KPIs | Phase 4b |
| **SkyScheduler** | Day/week/month calendar views | Reuse timeline rendering; Phase 4c |

### Exit criteria

- CRM sample: list + advanced filter + grid 2.0 + export.
- Performance target: 1M logical rows, smooth scroll on desktop; 10k rows on tablet.

---

## Phase 5 — Diagram Pro (12–16 weeks)

| Feature | Description |
|---------|-------------|
| Routers | `OrthogonalEdgePathComputer`, `BezierEdgePathComputer` |
| Viewport | Zoom/pan (wheel + pinch), snap grid, alignment guides |
| Commands | Undo/redo (`IUndoStack`) |
| Editing | Multi-select marquee, align/distribute, grouping |
| Model | Edge labels, port validation rules |
| Export | SVG/PNG |
| Touch | Pinch-zoom, long-press connect |

### Exit criteria

- Flow-editor demo with router swap and undo/redo.
- Usable on tablet with touch (pinch, two-finger pan).

---

## Phase 6 — Commercial hardening (continuous from Phase 2)

| Area | Features |
|------|----------|
| Accessibility | WCAG 2.1 AA audit; grid/tree focus model documented |
| Localization | Resource files; RTL layout tests; date/number formatting |
| Security | Safe filter SQL guidance; no dynamic SQL in default exporters |
| Licensing | MIT core; optional commercial license for Pro packages |
| Support | Issue templates; monthly minor releases; LTS branch policy |
| Visual regression | Screenshot baselines per control in CI |
| Telemetry | None in library |

---

## Priority matrix (realistic apps)

Use this when sequencing work across phases.

| Priority | Item | Desktop | Mobile | Rationale |
|----------|------|:-------:|:------:|-----------|
| **P0** | CI, NuGet, GitHub Pages | ✓ | ✓ | Unblocks all adoption |
| **P0** | `SkySplitView` + list-detail template | ✓ | ✓ | Every LOB app needs master-detail |
| **P0** | `SkyCommandBar` + `SkyPageHeader` | ✓ | ✓ | App chrome beyond raw nav |
| **P0** | Autocomplete, numeric, date range, masked input | ✓ | ✓ | Forms in real apps |
| **P0** | `SkyMessageBox` + `SkyEmptyState` | ✓ | ✓ | Polish users expect |
| **P0** | Safe area + bottom sheet + keyboard inset | — | ✓ | Mobile unusable without these |
| **P0** | Mobile demo app + CI compile | — | ✓ | Proves multiplatform claim |
| **P1** | Grid 2.0 (resize, multi-select, async paging) | ✓ | ✓ | Data apps |
| **P1** | `SkyDataPager`, `SkyDrawer`, file picker | ✓ | ✓ | Common workflows |
| **P1** | Touch target policy + pull-to-refresh | — | ✓ | Mobile list apps |
| **P2** | Charts / KPI tiles | ✓ | ✓ | Dashboards |
| **P2** | Diagram zoom/undo/routers | ✓ | ✓ | Differentiator |
| **P3** | Scheduler, rich text, PDF viewer | ✓ | — | Vertical-specific |

---

## Milestone timeline (indicative)

```text
Now–Q2    Phase 0 (CI/NuGet/samples) + Phase 2 shell/forms wave
Q2–Q3     Phase 3 mobile primitives + adaptive samples
Q3–Q4     Phase 4 grid 2.0 + filter integration
Year 2    Phase 5 diagram; Phase 4b charts; Phase 6 continuous
```

Phase 3 can start in parallel once Phase 0 CI exists. Phase 4 grid work can overlap Phase 3 after async paging API is designed.

---

## Success metrics

| Metric | Target |
|--------|--------|
| NuGet downloads | Steady growth after Phase 0 launch |
| CI reliability | >99% green `main` branch |
| Reference apps | ≥2 samples (settings + list-detail) + 1 mobile field app |
| API coverage | 100% public members documented |
| a11y | WCAG AA on Essentials; grid/tree exceptions documented |
| Platforms | 4 targets in CI: Windows, macOS/Linux, Android or iOS, Browser (optional) |
| Performance | Published grid virtualization benchmarks |
| Mobile | Safe area + bottom sheet in mobile demo; touch target audit pass |

---

## Related documents

- [Current state](current-state.md)
- [Gap analysis](gap-analysis.md)
- [Layout controls](layout.md)
- [Design tokens](design-tokens.md)
