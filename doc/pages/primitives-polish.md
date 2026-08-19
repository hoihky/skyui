---
title: Primitives polish
order: 90
---

Tooltip, popover, loading button state, and consistent disabled styling for Sky primitives.

## Components

| API | Use for |
|-----|---------|
| `SkyTooltip` | Themed tooltip surface |
| `SkyTooltipProperties.Tip` | Attach styled tooltips to any control |
| `SkyPopover` | Lightweight flyout panels (non-menu content) |
| `SkyButtonProperties.IsLoading` | Replace button content with a progress ring while async work runs |
| `SkyDisabledOpacity` | Shared token (`0.45`) for disabled control opacity |

## Tooltips

```xml
<Button Classes="sky"
        Content="Hover me"
        sky:SkyTooltipProperties.Tip="Quick action description" />
```

Rich content:

```xml
<sky:SkyTooltipProperties.Tip>
  <sky:SkyTooltip>
    <TextBlock Text="Supports any content." TextWrapping="Wrap" MaxWidth="220" />
  </sky:SkyTooltip>
</sky:SkyTooltipProperties.Tip>
```

You can also set `ToolTip.Tip` to a `SkyTooltip` instance directly.

## Popover

```xml
<Button Content="Open popover">
  <Button.Flyout>
    <sky:SkyPopover Placement="Bottom">
      <StackPanel Spacing="8">
        <TextBlock Text="Panel title" Classes="sky-section-title" />
        <TextBlock Text="Body copy." Classes="sky-body-secondary" />
      </StackPanel>
    </sky:SkyPopover>
  </Button.Flyout>
</Button>
```

Use `SkyMenuFlyout` for command menus; `SkyPopover` is for informational or form-like panels.

## Loading button

```xml
<Button Classes="sky sky-primary"
        Content="Save changes"
        Command="{Binding SaveCommand}"
        sky:SkyButtonProperties.IsLoading="{Binding IsSaving}" />
```

While loading:

- Content is replaced with an indeterminate `SkyProgressRing`
- The button is disabled to prevent duplicate submits
- `sky-loading` keeps full opacity (not the disabled fade)

## Disabled consistency

All `Classes="sky"` primitives use `SkyDisabledOpacity` (0.40) for chrome controls and dedicated disabled foreground/border brushes for text inputs.

Sky controls (`SkyDatePicker`, `SkyExpander`, `SkySlider`, etc.) follow the same token in `Primitives.Styles.axaml`.

## Related

- [Development roadmap](development-roadmap.md) — Primitives polish sprint
- [Menu accelerators](menu-accelerators.md) — `SkyMenuFlyout` for menus vs. `SkyPopover` for panels
