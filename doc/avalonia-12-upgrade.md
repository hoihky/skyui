# Avalonia 12 upgrade

SkyUI targets **Avalonia 12.1.1** on **.NET 10** (central version in `Directory.Build.props`).

## Evaluation (11.3.12 → 12.1.1)

| Area | Impact on SkyUI |
|------|-----------------|
| **.NET** | Already on `net10.0` — no TFM change required. |
| **Compiled bindings** | Already `AvaloniaUseCompiledBindingsByDefault=true` — aligned with v12 default. |
| **`Avalonia.Diagnostics`** | Removed from `SkyUI.Demo`. Use [Avalonia Plus Dev Tools](https://docs.avaloniaui.net/docs/tools/developer-tools/installation) + `AvaloniaUI.DiagnosticsSupport` if licensed. |
| **C# `Binding`** | `Binding(path, mode)` ctor removed; use `{ Mode = … }` initializer (fixed in `SkyAccordionItem`). Other `new Binding(path) { … }` usages remain valid (`Binding` → reflection binding). |
| **Clipboard / DnD** | No `IDataObject` usage in repo. |
| **Window decorations** | Demo uses default window chrome — no `SystemDecorations` usage. |
| **Tests** | All unit + headless tests pass after upgrade. |

## Packages (per project)

All Avalonia packages use `$(AvaloniaVersion)`:

- `Avalonia`, `Avalonia.Desktop`, `Avalonia.Themes.Fluent` (demo)
- `Avalonia.Fonts.Inter` (`SkyUI.Fonts`)

## Optional: Dev Tools (Debug)

If you have Avalonia Plus:

```xml
<PackageReference Include="AvaloniaUI.DiagnosticsSupport" Version="2.2.3" />
```

```csharp
// After app startup (e.g. MainWindow loaded):
this.AttachDeveloperTools();
```

## References

- [Breaking changes in Avalonia 12](https://docs.avaloniaui.net/docs/avalonia12-breaking-changes)
- [12.1.1 release](https://github.com/AvaloniaUI/Avalonia/releases)
