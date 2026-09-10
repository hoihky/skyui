---
title: Mobile primitives
order: 55
---

Early mobile primitives live in `SkyUI.Controls` (not a separate `SkyUI.Mobile` package yet). They target phone and tablet layouts on Avalonia iOS/Android while remaining usable on desktop for development and testing.

## Controls and APIs

| Control / API | Description |
|---------------|-------------|
| `SkySafeArea` | Content control that applies `IInsetsManager` safe-area padding (per-edge toggles) |
| `SkyKeyboardInset` | Adds bottom padding when `IInputPane` reports an open soft keyboard |
| `SkyTouchTarget` | Attached property `EnsureTouchTarget` — enforces 44×44 pt minimum hit size |
| `SkySheetHost` | Bottom sheet host with scrim and slide animation (shared with desktop feedback) |
| `SkyActionSheet` | Static `ShowAsync` — iOS-style action list over `SkySheetHost` |

## SkySafeArea

```xml
<sky:SkySafeArea>
  <Grid RowDefinitions="Auto,*">
    <!-- content respects notch / home indicator when insets are available -->
  </Grid>
</sky:SkySafeArea>
```

Per-edge toggles: `ApplyTop`, `ApplyBottom`, `ApplyLeft`, `ApplyRight` (all default to `true`). On desktop, insets are typically zero.

## SkyTouchTarget

```xml
<Button Classes="sky sky-subtle"
        Content="Delete"
        sky:SkyTouchTarget.EnsureTouchTarget="True" />
```

Optional `SkyTouchTarget.MinTouchSize` overrides the default **44** pt recommendation.

## SkyActionSheet

Attach a `SkySheetHost` once (usually at the root of the page), then show actions from code:

```csharp
SkyActionSheet.Attach(SheetHost);

var index = await SkyActionSheet.ShowAsync([
    new SkyActionSheetItem { Title = "Share" },
    new SkyActionSheetItem { Title = "Delete", IsDestructive = true },
], title: "Actions");

if (index is int selected)
    // user picked items[selected]
```

`ShowAsync` returns `null` when the user cancels. Pass `sheetHost:` explicitly if you do not call `Attach`.

## SkyKeyboardInset

Wrap scrollable form content so the bottom clears the soft keyboard on mobile:

```xml
<sky:SkyKeyboardInset>
  <ScrollViewer>
    <StackPanel>
      <sky:SkyFormField Label="Notes">
        <TextBox Classes="sky" />
      </sky:SkyFormField>
    </StackPanel>
  </ScrollViewer>
</sky:SkyKeyboardInset>
```

## Sample apps

| Project | Purpose |
|---------|---------|
| `SkyUI.Demo` → **Mobile** page | Desktop gallery for all mobile primitives (`MobileDemo.axaml`) |
| `SkyUI.Demo.Mobile` | Shared mobile shell (bottom nav, four demo sections) |
| `SkyUI.Demo.iOS` | iOS host (`net10.0-ios`) referencing `SkyUI.Demo.Mobile` |

```bash
dotnet run --project src/SkyUI.Demo          # Mobile page in desktop gallery
dotnet build src/SkyUI.Demo.iOS/SkyUI.Demo.iOS.csproj -f net10.0-ios
```

`.NET for iOS` requires a compatible Xcode version for your installed workload. See [development-roadmap.md](development-roadmap.md) Phase 3 for remaining mobile work (FAB, pull-to-refresh, CI matrix).

## Related

- [Layout controls](layout.md) — responsive grids and breakpoints
- [Design tokens](design-tokens.md) — motion tokens used by `SkySheetHost`
- [Development roadmap](development-roadmap.md) — Phase 3 mobile and adaptive UX
