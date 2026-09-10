# SkyUI sample applications

Reference apps that consume SkyUI from a **local NuGet feed** (not published to nuget.org).

## Prerequisites

- .NET 10 SDK
- Repository built at least once

## Pack local NuGet packages

From the repository root:

```bash
chmod +x scripts/pack-local.sh
./scripts/pack-local.sh
```

Packages are written to `artifacts/packages` at version **0.1.0-local**.  
The root `nuget.config` registers this folder as the `skyui-local` source.

If you use `-p:UseSkyUIProjectReferences=false` and see missing types (e.g. `SkySheetHost`), repack and clear the cached local package:

```bash
./scripts/pack-local.sh
rm -rf ~/.nuget/packages/skyui
dotnet restore samples/ThemeBuilderApp/ThemeBuilderApp.csproj --force-evaluate -p:UseSkyUIProjectReferences=false
```

## Build samples

Inside the repository, samples use **project references** to SkyUI (via `SkyUI.SampleReferences.props`) so they always compile against the latest source — including new controls like `SkySheetHost`.

To validate the **NuGet-only** workflow instead:

```bash
./scripts/pack-local.sh
dotnet build samples/ThemeBuilderApp/ThemeBuilderApp.csproj -p:UseSkyUIProjectReferences=false
```

## Build samples (all)

```bash
dotnet build samples/SettingsApp/SettingsApp.csproj
dotnet build samples/CrudListDetail/CrudListDetail.csproj
dotnet build samples/ThemeBuilderApp/ThemeBuilderApp.csproj
```

## Run

```bash
dotnet run --project samples/SettingsApp
dotnet run --project samples/CrudListDetail
dotnet run --project samples/ThemeBuilderApp
```

## Samples

| App | Demonstrates |
|-----|----------------|
| **SettingsApp** | `SkyNavigationView`, `SkyFormField`, validators, theme/density/accent, snackbar, JSON settings persistence |
| **CrudListDetail** | Master-detail layout, `SkyVirtualDataGrid`, `IVirtualGridDataSource` adapter, repository pattern, CRUD MVVM |
| **ThemeBuilderApp** | Live theme builder: variant, density, accent via `SkyTheme` APIs (no XAML copy) |

Both apps use:

- **MVVM** with `ViewModelBase`, `RelayCommand`, `AsyncRelayCommand`
- **Dependency injection** via `Microsoft.Extensions.DependencyInjection`
- **SOLID** — repository/store abstractions, facades (`ISnackbarNotifier`), composition roots
- **No underscore-prefixed** backing fields in sample code

`SampleInfrastructure` is a shared project reference (not a NuGet package) for MVVM primitives only.
