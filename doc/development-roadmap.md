# Development roadmap

Phased plan to evolve SkyUI from the [current state](./current-state.md) toward a commercial-grade, multiplatform toolkit, addressing gaps in [gap-analysis.md](./gap-analysis.md).

Durations assume a small core team; workstreams within a phase can run in parallel where noted.

## Product vision

| Pillar | Description |
|--------|-------------|
| **Sky Design System** | Semantic tokens, themes (dark/light/high contrast), icons, spacing, typography, motion |
| **Sky Essentials** | Broad control catalog and app shell patterns with consistent API and documentation |
| **Sky Data / Pro** | Virtual grid, filter builder, tree checklist, diagram, scheduling/timeline |
| **Sky Platform** | Desktop, iOS, Android, and browser validated with touch and adaptive layout |

**Positioning:** Cross-platform .NET apps that need **data-heavy UI** and **diagram/media** tooling, built on Avalonia.

---

## Target package architecture

Start as a single package; split when consumers need smaller browser bundles.

```text
SkyUI.Core          Tokens, shared interfaces, attached properties
SkyUI               Essentials controls + default theme hooks
SkyUI.Themes.Sky    Default dark/light/high-contrast theme resources
SkyUI.Icons         Icon font or SVG set
SkyUI.Data          Grid, filter (optional split from core)
SkyUI.Diagram       Diagram surface and routers (optional split)
```

Dependency direction: `Core` → `SkyUI` → (`Data`, `Diagram`); `Themes` and `Icons` plug into `SkyUI`.

---

## Phase 0 — Foundation (8–12 weeks)

**Goal:** Operate SkyUI as a shippable product.

### Deliverables

| Workstream | Features |
|------------|----------|
| Repository layout | Root `skyui.sln`; `src/SkyUI`, `src/SkyUI.Demo`, `tests/SkyUI.UnitTests`, `tests/SkyUI.HeadlessTests` (Avalonia headless); optional `samples/` |
| CI/CD | Build on PR; `dotnet format`; analyzers (including Avalonia); test gate; NuGet artifact on version tag |
| Packaging | `PackageId`, authors, repository URL, README in package, symbol packages, **0.x semver** |
| Documentation v1 | Root README: install, include theme, control index; XML comments on public API; screenshots from demo |
| Engineering policy | CONTRIBUTING; control checklist (template parts, theme AXAML, demo page, tests); breaking-change policy |

### Exit criteria

- Green CI on every PR.
- Publishable NuGet package (even if unlisted initially).
- Every public control has XML docs and a demo entry.

---

## Phase 1 — Design system 2.0 (10–14 weeks)

**Goal:** White-label-ready theming without losing the current look.

### Planned features

| Feature | Description |
|---------|-------------|
| **Sky token layer** | `SkyTokens.axaml`: semantic colors (`Background`, `Surface`, `Accent`, `Danger`, …), 8px spacing scale, radius and elevation scales, typography roles |
| **Theme mapping** | Sky-branded preset (`ContentFirstDark`); palettes under `Themes/SkyDark/`; legacy `Spotify*` / `Themes/Sky/` shims retained |
| **Theme API** | Single `SkyTheme` include; accent override (attached property or `SkyThemeOptions`) |
| **Light + high contrast** | Full brush sets; focus rings; target WCAG AA contrast for text and controls |
| **Icons** | `SkyUI.Icons` or embedded font; sizes 16/20/24 used in buttons, nav, list items |
| **Typography** | Ship open fonts (e.g. Inter + Noto CJK) aligned with `DESIGN.md` hierarchy |
| **Density** | Comfortable (default) and compact modes via `SkyDensityTokens` + `SkyThemeProperties.Density` |

### Exit criteria

- Demo toggles Dark / Light / High contrast.
- Sample third-party app sets accent color without copying theme XAML.

---

## Phase 2 — Essentials catalog (16–24 weeks, parallel sprints)

**Goal:** Cover majority of line-of-business UI with documented, tested controls.

**API strategy (choose one and apply consistently):**

- **Option A:** `Sky*` templated controls wrapping Avalonia primitives, or  
- **Option B:** Documented `Classes="sky"` + behaviors/helpers.

### Sprint themes

| Sprint | Controls and behaviors |
|--------|-------------------------|
| Feedback | Modal/dialog host, snackbar queue, banner, progress ring, skeleton placeholder — **SkyAlert, SkyBanner, SkySnackbarHost, SkyDialogHost, SkyProgressRing, SkySkeleton** |
| Navigation | Navigation view (sidebar / compact / bottom on narrow width), styled tab control, breadcrumb — **SkyNavigationView, SkyTabView, SkyBreadcrumb** |
| Forms | Form field wrapper (label, hint, validation, error state), radio group, slider, password and search variants — **SkyFormField, SkyRadioGroup, SkySlider, SkySearchBox, SkyPasswordBox** |
| Menus | Context menu and menubar styles; keyboard accelerators documented — **SkyMenuBar, SkyContextMenu, SkyMenuFlyout, SkyAccelerator, SkyMenuGestures** ([menu-accelerators.md](./menu-accelerators.md)) |
| Pickers | Date picker, calendar month view, time picker; culture-aware formatting — **SkyDatePicker, SkyCalendar, SkyTimePicker, SkyPickerFormat** ([picker-formatting.md](./picker-formatting.md)) |
| Lists | Virtual tree view (checkboxes optional); patterns reused from `CheckedListBox` — **SkyVirtualTreeView** ([virtual-tree.md](./virtual-tree.md)) |
| Layout | Card, divider, expander; responsive grid helpers — **SkyCard, SkyDivider, SkyExpander, SkyResponsiveGrid, SkyGridLayout** ([layout.md](./layout.md)) |
| Primitives polish | Tooltip, popover, loading button state, disabled consistency — **SkyTooltip, SkyPopover, SkyTooltipProperties, SkyButtonProperties** ([primitives-polish.md](./primitives-polish.md)) |

### Per-control definition of done

- Theme AXAML (all interaction states).
- Demo page in `SkyUI.Demo`.
- Headless or unit tests for critical behavior (open/close, selection, validation state).
- Accessibility pass: name, keyboard, focus visible.

### Exit criteria

- Reference sample: settings + list + form app built only from Sky docs and packages.
- Essentials list published in README/doc site.

---

## Phase 3 — Multiplatform and adaptive layout (12–16 weeks)

**Goal:** Prove desktop, mobile, and browser on Avalonia.

### Planned features

| Feature | Description |
|---------|-------------|
| TFM matrix | `net10.0` plus optional `net10.0-android`, `net10.0-ios`, browser WASM project |
| Package splitting | Heavy modules optional for WASM size (`SkyUI.Data`, `SkyUI.Diagram`) |
| **SkyUI.Mobile** (or unified demo) | Phone: bottom nav; tablet: split; desktop: existing gallery |
| Touch policy | Minimum 44dp targets; press-and-hold where needed on diagram/timeline |
| Safe area | Attached properties for notched devices and browser viewport |
| Input | Soft keyboard avoidance for text fields; scroll integration |
| Browser | Supported browser list; WASM size budget; lazy-load large demos |
| Breakpoints | Implement `DESIGN.md` breakpoint table as `SkyBreakpoint` + markup helpers |

### Exit criteria

- CI builds mobile and browser targets.
- Documented manual test matrix.
- `SkyVirtualDataGrid` usable on tablet (scroll, column policy).

---

## Phase 4 — Data and productivity (20+ weeks, ongoing)

**Goal:** Mid-tier commercial data controls; deepen existing modules.

### SkyVirtualDataGrid 2.0

| Feature | Description |
|---------|-------------|
| Column resize | Drag handles; optional auto-fit; star columns |
| Selection | Multi-select, checkbox column, select all |
| Header UX | Context menu, column chooser |
| Filtering | Filter row or header filters bound to `FilterDocument` / expression model |
| Grouping | Optional group rows and summaries |
| Data source | Async paging contract; cancellation; threading documentation |
| Export | CSV (existing), Excel, PDF table via `ISkyDataGridExporter` plugins |
| Accessibility | Full keyboard navigation; automation peers |

### FilterEditor 2.0

| Feature | Description |
|---------|-------------|
| Persistence | Save/load JSON; named filters |
| Editors | Per-type value UI (date, enum, numeric) |
| SQL | Parameterized exporters; dialect plugins (SQLite, PostgreSQL, SQL Server) |
| Integration | Official sample: filter → grid data source |

### Scheduling (optional Phase 4b)

| Feature | Description |
|---------|-------------|
| SkyScheduler | Day/week/month views; reuse timeline rendering where possible |

### Exit criteria

- Shipped sample: CRM list + advanced filter + export.
- Performance target documented: e.g. 1M logical rows with smooth scroll on desktop (virtualization on).

---

## Phase 5 — Diagram Pro (12–16 weeks)

| Feature | Description |
|---------|-------------|
| Routers | `OrthogonalEdgePathComputer`, `BezierEdgePathComputer` |
| Viewport | Zoom/pan (wheel + pinch), snap grid |
| Commands | Undo/redo (`IUndoStack`) |
| Editing | Multi-select, align/distribute |
| Model | Edge labels, port validation rules |
| Export | SVG/PNG |
| Touch | Pinch-zoom, long-press connect |

### Exit criteria

- Flow-editor demo with router swap documented in API docs.
- Undo/redo covers move, resize, connect, delete.

---

## Phase 6 — Commercial hardening (continuous from Phase 2)

| Area | Features |
|------|----------|
| Accessibility | WCAG 2.1 AA audit; known gaps list; focus and contrast fixes |
| Localization | Resource files; RTL layout tests; date/number formatting |
| Security | Safe filter SQL guidance; no dynamic SQL concatenation in default exporters |
| Licensing | MIT core; optional commercial license for Pro packages if desired |
| Support | Issue templates; monthly minor releases; LTS branch policy |
| Visual regression | Screenshot baselines per control in CI |
| Telemetry | None in library; opt-in analytics only in demo if any |

---

## Milestone timeline (indicative)

```text
Q1–Q2   Phase 0 + Phase 1 start
Q2–Q3   Phase 1 complete + Phase 2 Essentials (first waves)
Q3–Q4   Phase 3 multiplatform + Phase 2 completion
Year 2  Phase 4 Data + Phase 5 Diagram; Phase 6 continuous
```

Adjust quarters to team size; Phase 4 can overlap Phase 3 once grid API is stable.

---

## Success metrics

| Metric | Target |
|--------|--------|
| NuGet downloads | Steady growth after Phase 0 launch |
| CI reliability | >99% green main branch |
| API coverage | 100% public members documented |
| a11y | WCAG AA on Essentials; grid/tree documented exceptions |
| Platforms | 4 targets in CI: Windows, macOS/Linux, Android or iOS, Browser |
| Performance | Published grid virtualization benchmarks |

---

## Related documents

- [README](./README.md)
- [Current state](./current-state.md)
- [Gap analysis](./gap-analysis.md)
