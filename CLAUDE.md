# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project vision

Fab is an experimental "new browser" / CMS stack. See `Plans.txt` at the repo root for the design intent: websites served as structured JSON (not HTML), fully client-side styling, optional images, and client-specific rendering. The C# solution under `fab-cs/` is the reference implementation of the host + CMS + desktop client pieces.

## Solution layout (`fab-cs/Fab.sln`, net8.0)

- **Fab.Data** — POCO model library only. `Article`, `ContentBase` (polymorphic: `Paragraph`, `Image`), `OrderedContentEntry`. `ContentBase` uses `[JsonDerivedType]` for polymorphic JSON. `[Accessibility]` attribute marks fields that clients may optionally omit (see `Plans.txt` re: accessibility data toggle).
- **Fab.Core** — Shared EF Core + hosting glue. Owns `CmsWorkingDbContext` (TPT mapping: `ContentBase` → `ContentBases` table, with per-type tables for `Paragraph`/`Image`) and `FabGlobal`, the static host/service-provider holder. `FabGlobal.ConfigureFab` is the single place that wires `fab.json` config and the SQLite `DbConnection` — both the web host and shell call into it.
- **Fab.Host** — ASP.NET Core minimal-API (`WebApplication.CreateSlimBuilder`) exposing `/articles` and `/articles/{id}` that eager-load `Entries.Content`. This is the JSON server clients fetch from.
- **Fab.Shell** — Console CMS editor. Builds a generic host via `FabGlobal.BuildHost`, registers `ShellCommand`s (`ListDbCommand`, `PopulateDbCommand`) as transient services, and drives them through `Commands/Menu.cs`. New shell features should follow the `ShellCommand` + `Menu` pattern and register in `Program.cs`.
- **Fab.Client.Desktop** — WPF (`net8.0-windows`, `UseWPF`) reference client using `CommunityToolkit.Mvvm`. MVVM split: `ViewModels/` + `Views/` with one VM per content type mirroring the polymorphic model (`ArticleViewModel`, `ParagraphViewModel`, `OrderedContentEntryViewModel`). Client-side styling is a design goal — avoid baking server-driven visual decisions into views.

## Dependency direction

`Fab.Data` → (no refs). `Fab.Core` → Data. `Fab.Host`, `Fab.Shell`, `Fab.Client.Desktop` → Core (+ Data). Keep Data POCO-only; EF/hosting concerns belong in Core.

## Config

Each executable project has its own `fab.json` copied to output (`PreserveNewest`) containing `ConnectionStrings:DbConnection` for the SQLite database. The current checked-in paths point at `d:\dev\fab.db` (Windows). Update per-environment before running on non-Windows. `InvariantGlobalization` is on for Host and Shell.

## Common commands

Run from `fab-cs/`:

```
dotnet build Fab.sln
dotnet run --project Fab.Host
dotnet run --project Fab.Shell
dotnet run --project Fab.Client.Desktop   # Windows only (WPF)
```

Run integration tests:

```
dotnet test Fab.Host.Tests
```

Tests boot `Fab.Host` in-process via `WebApplicationFactory<Program>` against a fresh temp-file SQLite database (schema via `EnsureCreated`, fixture cleans up on dispose). Connection string is overridden per-fixture — tests never touch your real DB.

No linter config beyond default analyzers. EF Core tools are referenced in `Fab.Host` (`dotnet ef ... --project Fab.Host`) for migrations if/when added — the schema is currently created implicitly.
