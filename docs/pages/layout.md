---
title: Layout controls
order: 50
---

SkyUI layout primitives for cards, dividers, expanders, and responsive grids.

## Controls

| Control | Use for |
|---------|---------|
| `SkyCard` | Elevated surface with optional header/footer |
| `SkyDivider` | Horizontal or vertical separator with optional label |
| `SkyExpander` | Single collapsible section |
| `SkyResponsiveGrid` | Auto-column `UniformGrid` driven by width |
| `SkyGridLayout` | Attached properties for responsive `Grid` columns |

## SkyCard

```xml
<sky:SkyCard Header="Title"
             Footer="Optional actions"
             IsHoverable="True">
  <TextBlock Text="Content" />
</sky:SkyCard>
```

`Border Classes="sky-card"` remains supported for simple containers.

## SkyDivider

```xml
<sky:SkyDivider />
<sky:SkyDivider Text="Section" />
<sky:SkyDivider Orientation="Vertical" Height="48" />
```

## SkyExpander

```xml
<sky:SkyExpander Header="Details" IsExpanded="True">
  <TextBlock Text="Hidden until expanded." />
</sky:SkyExpander>
```

Use `SkyAccordion` when stacking multiple expandable sections.

## Responsive grids

Breakpoints align with `SkyBreakpoint` / `DESIGN.md`:

| Width | Default columns |
|-------|-----------------|
| &lt; 425px | 1 |
| 425–576px | 1 |
| 576–768px | 2 |
| 768–896px | 3 |
| 896–1024px | 4 |
| 1024–1280px | 5 |
| ≥ 1280px | 5 |

### SkyResponsiveGrid

```xml
<sky:SkyResponsiveGrid>
  <Border Classes="sky-card" />
  <Border Classes="sky-card" />
</sky:SkyResponsiveGrid>
```

### SkyGridLayout (attached)

```xml
<Grid sky:SkyGridLayout.IsResponsive="True"
      sky:SkyGridLayout.AutoPlaceChildren="True">
  <Border />
  <Border />
</Grid>
```

Custom column counts via `SkyResponsiveColumnProfile` on `SkyResponsiveGrid.ColumnProfile` or `sky:SkyGridLayout.ColumnProfile`.

## Related

- [Development roadmap](development-roadmap.md) — Layout sprint
- `SkyBreakpoint` — shared width tokens for navigation and grids
