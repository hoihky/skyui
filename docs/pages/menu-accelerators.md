---
title: Menu keyboard accelerators
order: 60
---

SkyUI menus use Avalonia `MenuItem.InputGesture` (`KeyGesture`) for keyboard shortcuts. Accelerators are shown on the right side of each menu item and are handled by the platform when the menu is not open.

## Quick start

### XAML (MVVM)

```xml
<sky:SkyMenuBar>
  <MenuItem Header="_File">
    <MenuItem Header="_Save"
              InputGesture="{x:Static sky:SkyMenuGestures.Save}"
              Command="{Binding SaveCommand}" />
  </MenuItem>
</sky:SkyMenuBar>

<sky:SkyContextMenu>
  <MenuItem Header="Copy"
            InputGesture="{Binding CopyGesture}"
            Command="{Binding CopyCommand}" />
</sky:SkyContextMenu>
```

### View model

```csharp
public KeyGesture SaveGesture => SkyMenuGestures.Save;

public ICommand SaveCommand { get; }
```

Bind `InputGesture` to a `KeyGesture` property and `Command` to your `ICommand`. The menu item handles display; the command handles execution when clicked.

## Sky controls

| Control | Use for |
|---------|---------|
| `SkyMenuBar` | Top application menu (`Menu` with `sky` styling) |
| `SkyContextMenu` | Right-click menus |
| `SkyMenuFlyout` | Dropdown from buttons or toolbar items |

All apply `Classes="sky"` automatically. Child `MenuItem` elements inherit styling from the parent menu.

## Standard gestures (`SkyMenuGestures`)

| Gesture | Default | Action |
|---------|---------|--------|
| `New` | Ctrl+N | New document |
| `Open` | Ctrl+O | Open |
| `Save` | Ctrl+S | Save |
| `SaveAs` | Ctrl+Shift+S | Save as |
| `Close` | Ctrl+W | Close tab/window |
| `Quit` | Ctrl+Q | Quit application |
| `Undo` | Ctrl+Z | Undo |
| `Redo` | Ctrl+Y | Redo |
| `Cut` | Ctrl+X | Cut |
| `Copy` | Ctrl+C | Copy |
| `Paste` | Ctrl+V | Paste |
| `SelectAll` | Ctrl+A | Select all |
| `Delete` | Delete | Delete selection |
| `Find` | Ctrl+F | Find |

Use these constants instead of hard-coding strings so menus stay consistent across the app.

## Formatting and parsing (`SkyAccelerator`)

```csharp
// Platform-specific display (Ctrl+S on Windows, ⌘S on macOS)
var label = SkyAccelerator.Format(SkyMenuGestures.Save);

// Parse user-entered shortcut strings
var gesture = SkyAccelerator.Parse("Ctrl+Shift+S");
```

For bindings, use `SkyAcceleratorFormatConverter`:

```xml
<TextBlock Text="{Binding SaveGesture, Converter={x:Static sky:SkyAcceleratorFormatConverter.Instance}}" />
```

## Mnemonics (access keys)

Prefix a letter in `Header` with `_` to define a mnemonic:

```xml
<MenuItem Header="_File">
  <MenuItem Header="_Save" ... />
</MenuItem>
```

Press **Alt+F** (Windows/Linux) to open the File menu, then **S** for Save.

## Global shortcuts vs menu accelerators

| Mechanism | When it runs | Where to define |
|-----------|--------------|-----------------|
| `MenuItem.InputGesture` | When menu is focused or via platform integration | Menu XAML / VM `KeyGesture` |
| `KeyBinding` on `Window` | App-wide while window is active | `Window.KeyBindings` |
| `Command` + gesture | Same as menu item click | `ICommand` on menu item |

For app-wide shortcuts (e.g. Ctrl+S always saves), add `KeyBinding` on the window **and** set `InputGesture` on the menu item so the shortcut appears in the menu:

```xml
<Window KeyBindings="{Binding KeyBindings}">
  ...
</Window>
```

```csharp
new KeyBinding { Gesture = SkyMenuGestures.Save, Command = SaveCommand }
```

## Platform notes

- **Windows / Linux:** Modifiers display as `Ctrl`, `Alt`, `Shift`.
- **macOS:** Modifiers display as `⌘`, `⌥`, `⇧` via `PlatformKeyGestureConverter`.
- Use `SkyMenuGestures` with `KeyModifiers.Control`; Avalonia maps to Command (⌘) on macOS where appropriate.

## Styling

Menu styles live in `Themes/SkyDark/Controls/Menus.Styles.axaml`. Apply `Classes="sky"` on `Menu`, `ContextMenu`, or use `Sky*` menu controls. Focus rings use `SkyFocusRingBrush` for keyboard navigation (WCAG 2.4.7).

## Related

- [Design tokens](design-tokens.md) — semantic brushes used by menu chrome
- [Development roadmap](development-roadmap.md) — Menus sprint scope
