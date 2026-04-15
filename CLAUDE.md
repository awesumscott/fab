# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project vision

Fab is an experimental "new browser" / CMS stack. See `Plans.txt` at the repo root for the design intent: websites served as structured JSON (not HTML), fully client-side styling, optional images, and client-specific rendering. The C# solution under `fab-cs/` is the reference implementation of the host + CMS + desktop client pieces.

## Solution layout (`fab-cs/Fab.sln`, net8.0)

- **Fab.Data** — POCO model library only. `Article`, `ContentBase` (polymorphic: `Paragraph`, `Image`), `OrderedContentEntry`. `ContentBase` uses `[JsonDerivedType]` for polymorphic JSON. `[Accessibility]` attribute marks fields that clients may optionally omit (see `Plans.txt` re: accessibility data toggle).
- **Fab.Core** — Shared EF Core + hosting glue. Owns `CmsWorkingDbContext` (TPT mapping: `ContentBase` → `ContentBases` table, with per-type tables for `Paragraph`/`Image`) and `FabGlobal`, the static host/service-provider holder. `FabGlobal.ConfigureFab` is the single place that wires `fab.json` config and the SQLite `DbConnection` — both the web host and shell call into it.
- **Fab.Host** — ASP.NET Core minimal-API (`WebApplication.CreateSlimBuilder`) exposing `/articles` and `/articles/{id}` that eager-load `Entries.Content`. This is the JSON server clients fetch from.
- **Fab.Shell** — Console CMS editor. Builds a generic host via `FabGlobal.BuildHost`, registers `ShellCommand`s (`ListDbCommand`, `PopulateDbCommand`) as transient services, and drives them through `Commands/Menu.cs`. New shell features should follow the `ShellCommand` + `Menu` pattern and register in `Program.cs`.
- **Fab.Client.Core** — UI-agnostic client library (`net10.0`). Hosts the ViewModels (`ArticleViewModel`, `OrderedContentEntryViewModel`, `ParagraphViewModel`, `ImageViewModel`, `MainWindowViewModel`), the `IContentClient` abstraction + `HttpContentClient` implementation, `FabClientOptions` (base URL), and `AddFabClient` DI extension. Frontends (WPF, future Avalonia/WASM) reference this and supply only UI.
- **Fab.Client.Avalonia** — cross-platform desktop client (`net10.0`, Avalonia 11). References `Fab.Client.Core` only. `Program.cs` boots the Avalonia app; `App.axaml.cs` builds an `IHost`, calls `AddFabClient`, resolves `MainWindow` + `MainWindowViewModel`, kicks off `LoadFirstArticleAsync`. axaml equivalents of the WPF views live in `Views/`. Runtime needs native deps (SkiaSharp, fontconfig) — trivially present on Windows/macOS, `apt install libfontconfig1` on Linux.
- **Fab.Client.Desktop** — WPF (`net10.0-windows`, `UseWPF`) shell: `App`, `MainWindow`, XAML views + `DataTemplate`s mapping VMs from `Fab.Client.Core.ViewModels` to views in `Fab.Client.Desktop.Views`. Boots its own host via `Host.CreateApplicationBuilder`, calls `AddFabClient`, resolves `MainWindow` + VM from DI. No EF / `CmsWorkingDbContext` reference — all data comes from the Host via HTTP.
- **Fab.Editor.Core** — reflection-driven generic-form editor library (`net10.0`). References `Fab.Data` + `Fab.Core`. Given any POCO, `EditGenericModelViewModel.BuildFields` introspects its properties and emits child `IEditableField` VMs: `string` → `EditTextFieldViewModel` (two-way binding via `PropertyInfo`), `List<T>` → `EditListViewModel` (child editors for each item), `IUnique`-implementing nested types → recursive `EditGenericModelViewModel`. Other types are skipped. `EditorMainWindowViewModel` holds a DbContext for the app's lifetime, loads an article with `Include(Entries).ThenInclude(Content)`, and exposes a Save command that calls `SaveChangesAsync` on the change-tracked entity.
- **Fab.Editor.Desktop** — WPF (`net10.0-windows`) editor app. References `Fab.Editor.Core` + `Fab.Core`. Unlike `Fab.Client.Desktop`, this one **talks to SQLite directly** via `AddFabDatabaseFactory` — aligns with `Plans.txt` ("databases are created offline and uploaded, no web-based auth"). Host stays read-only HTTP; the editor does the writes locally.

## Dependency direction

`Fab.Data` → (no refs). `Fab.Core` → Data (EF + hosting glue). `Fab.Client.Core` → Data (no EF). `Fab.Editor.Core` → Data + Core (EF OK — editor is local-first). `Fab.Host` and `Fab.Shell` → `Fab.Core` (+ Data). `Fab.Client.Desktop` → `Fab.Client.Core` only. `Fab.Editor.Desktop` → `Fab.Editor.Core` + `Fab.Core`. Keep Data POCO-only; keep `Fab.Client.Core` EF-free so WASM/Avalonia can reuse it.

## Config

Each executable project has its own `fab.json` copied to output (`PreserveNewest`) containing `ConnectionStrings:DbConnection` for the SQLite database. The current checked-in paths point at `d:\dev\fab.db` (Windows). Update per-environment before running on non-Windows. `InvariantGlobalization` is on for Host and Shell.

## Common commands

Run from `fab-cs/`:

```
dotnet build Fab.sln
dotnet run --project Fab.Host
dotnet run --project Fab.Shell
dotnet run --project Fab.Client.Desktop   # Windows only (WPF)
dotnet run --project Fab.Editor.Desktop   # Windows only (WPF)
```

Run integration tests:

```
dotnet test Fab.Host.Tests
```

Tests boot `Fab.Host` in-process via `WebApplicationFactory<Program>` against a fresh temp-file SQLite database (schema via `EnsureCreated`, fixture cleans up on dispose). Connection string is overridden per-fixture — tests never touch your real DB.

No linter config beyond default analyzers.

EF migrations live in `Fab.Core/Migrations/` (the assembly that owns `CmsWorkingDbContext`). To add a new migration:

```
dotnet ef migrations add <Name> --project Fab.Core --startup-project Fab.Host
```

`Fab.Editor.Desktop` and `Fab.Shell.PopulateDbCommand` call `Database.MigrateAsync()` on startup to apply pending migrations. `Fab.Host` deliberately does not — per `Plans.txt`, databases are authored offline and uploaded; the Host is a read-only consumer that should not mutate schema. Tests use `Database.EnsureCreatedAsync()` against a throwaway temp-file DB (no migration history), which EF accepts with a warning.
