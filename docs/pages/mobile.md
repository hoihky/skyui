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
| `SkyFab` | Floating action button with icon, optional extended label, safe-area margins |
| `SkyPullToRefresh` | Wraps a `ScrollViewer`; bind `IsRefreshing` + `RefreshCommand` |

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

`ShowAsync` returns `null` when the user cancels, taps the scrim, or closes the sheet with the header close button. Pass `sheetHost:` explicitly if you do not call `Attach`. Concurrent calls on the same host are serialized (queued) so only one sheet is active at a time.

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

## SkyFab

Place in a `Grid` above scrollable content (bottom-right by default):

```xml
<Grid>
  <sky:SkyPullToRefresh>...</sky:SkyPullToRefresh>
  <sky:SkyFab Icon="Add"
              Label="Compose"
              IsExtended="True"
              Command="{Binding AddCommand}" />
</Grid>
```

- `Command` / `CommandParameter` — MVVM command binding
- `HonorSafeArea` (default `true`) and `EdgeMargin` — keeps the FAB above home indicator / notch
- `IsExtended` + `Label` — pill-shaped extended FAB

## SkyPullToRefresh

Content must be a `ScrollViewer`:

```xml
<sky:SkyPullToRefresh IsRefreshing="{Binding IsRefreshing, Mode=TwoWay}"
                      RefreshCommand="{Binding RefreshCommand}">
  <ScrollViewer>
    <ItemsControl ItemsSource="{Binding Items}" />
  </ScrollViewer>
</sky:SkyPullToRefresh>
```

The view model sets `IsRefreshing = true` when refresh starts and `false` when async work completes. `RefreshRequested` is raised for code-behind handlers without a command.

## Desktop testing (without iOS/Android)

When device hosts are unavailable, use **`SkyUI.Demo` → Mobile** page:

```bash
dotnet run --project src/SkyUI.Demo
```

| Feature | Desktop | Notes |
|---------|---------|-------|
| `SkyPullToRefresh` | Yes | Mouse drag at scroll top (same pointer pipeline as touch) |
| `SkyFab` | Yes | Click, extended label, `Command` binding |
| `SkyTouchTarget` | Yes | Min 44×44 pt hit area is visible in layout |
| `SkyActionSheet` | Yes | Full sheet flow via `SkySheetHost` |
| `SkySheetHost` | Yes | Bottom sheet + scrim |
| `SkyNavigationView` (bottom) | Yes | **Phone preview** tab embeds `SkyUI.Demo.Mobile` shell |
| Tabs, breadcrumbs, alerts, snackbars | Yes | Inside phone preview sections |
| `SkyFormPage` / mobile forms layout | Yes | Layout only in phone preview |
| `SkySafeArea` | No* | `IInsetsManager` padding is usually zero on desktop |
| `SkyKeyboardInset` | No* | Soft keyboard / `IInputPane` not active on desktop |

\*Safe-area and keyboard inset still run without errors; they simply have no visible effect until you run on iOS or Android.

The **Interactive** tab exercises PTR, FAB, touch targets, action sheets, and filter sheets directly. The **Phone preview** tab hosts the shared `MobileAppView` in a 390×780 frame (same content as `SkyUI.Demo.Mobile`).

## Sample apps

| Project | Purpose |
|---------|---------|
| `SkyUI.Demo` → **Mobile** page | Desktop testing: interactive tab + phone preview |
| `SkyUI.Demo.Mobile` | Shared mobile shell (bottom nav, four demo sections) |
| `SkyUI.Demo.iOS` | iOS host (`net10.0-ios`) referencing `SkyUI.Demo.Mobile` |

```bash
dotnet run --project src/SkyUI.Demo          # Mobile page — desktop testing
dotnet build src/SkyUI.Demo.iOS/SkyUI.Demo.iOS.csproj -f net10.0-ios
```

`.NET for iOS` requires a compatible Xcode version for your installed workload. See [development-roadmap.md](development-roadmap.md) Phase 3 for remaining mobile work.

## Related

- [Layout controls](layout.md) — responsive grids and breakpoints
- [Design tokens](design-tokens.md) — motion tokens used by `SkySheetHost`
- [Development roadmap](development-roadmap.md) — Phase 3 mobile and adaptive UX
