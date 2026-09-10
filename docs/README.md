# SkyUI documentation

Product and engineering documentation for evolving SkyUI into a commercial-grade, multiplatform UI toolkit on [Avalonia](https://avaloniaui.net/).

**Start here:** [index.html](./index.html) — SkyUI product page with architecture overview, programming guide, and quick start (Spark-style layout).

## HTML docs

| Document | Description |
|----------|-------------|
| [Current state](./current-state.html) | Repository inventory (stack, controls, theme, gaps). |
| [Gap analysis](./gap-analysis.html) | Commercial toolkit comparison and prioritized gaps. |
| [Development roadmap](./development-roadmap.html) | Phased plan, exit criteria, package architecture. |
| [Design tokens](./design-tokens.html) | Palette, semantic resources, legacy aliases. |
| [Layout controls](./layout.html) | Cards, dividers, expanders, responsive grids. |
| [Menu accelerators](./menu-accelerators.html) | Keyboard shortcuts and platform notes. |
| [Picker formatting](./picker-formatting.html) | Culture-aware date/time formatting. |
| [Virtual tree](./virtual-tree.html) | SkyVirtualTreeView MVVM patterns. |
| [Primitives polish](./primitives-polish.html) | Tooltips, popovers, loading button states. |
| [Rename strategy](./rename-strategy.html) | Sky brand migration notes. |
| [Legacy sunset](./legacy-sunset.html) | Spotify alias deprecation timeline (1.x). |
| [Avalonia 12 upgrade](./avalonia-12-upgrade.html) | Avalonia 12 migration notes. |

## Source layout

| Path | Purpose |
|------|---------|
| `index.html` | Product landing page (edit `sdk-guide-body.html`, then run `build-index.py`) |
| `sdk-guide-body.html` | Source fragment for `index.html` |
| `pages/*.md` | Markdown sources for topic HTML pages |
| `theme/` | MDWeb layout for topic pages |
| `assets/css/portfolio.css` | Styles for `index.html` |
| `assets/css/site.css` | Styles for generated topic pages |
| `build-docs.sh` | Regenerate topic HTML + `index.html` |

```bash
./doc/build-docs.sh
```
