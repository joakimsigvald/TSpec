# TSpec — instructions for coding agents

## Documentation must stay in sync with implemented features

Two documents describe TSpec to its users:

- `README.md` — the full human documentation (also rendered on nuget.org)
- `TSpec-agent-reference.md` — the condensed agent reference shipped in the NuGet package

Update one when a change alters what its reader must know to write a test correctly: new or changed
surface, a semantic they would otherwise get wrong, a behaviour that would surprise them. 

Update `PackageVersion` and `PackageReleaseNotes` in `Core/Core.csproj` when preparing a release
(docs/packaging-only = patch, new functionality = minor).

## Build and test

- `dotnet test Core.Test` builds and runs the suite on net8.0, net9.0 and net10.0; narrow it with
  `-f net10.0` and `--filter-class Namespace.ClassName`. It runs on Microsoft.Testing.Platform, opted
  in by `global.json` — without it, xunit.v3 4.x refuses `dotnet test` on the .NET 10 SDK. VSTest
  options such as `--logger` are refused.
- The library multi-targets net8.0/net9.0/net10.0 — run the full suite on all three before a release.

## Releasing

1. Update `PackageVersion` and `PackageReleaseNotes` in `Core/Core.csproj`, and the agent
   reference's "covers TSpec x.y" line — it ships inside the package and had gone four minor
   versions stale by 1.5.0.
2. Run the full suite on all three target frameworks.
3. `dotnet pack Core -c Release`, then upload `Core/bin/Release/TSpec.<version>.nupkg` **manually at
   nuget.org**. That folder keeps every previously packed version, so pick the file by name rather
   than globbing.
4. Optional: tag the published commit `v<version>` — worth it only if a GitHub Releases page is
   wanted. Without a tag, the commit a version shipped from is still findable with
   `git log -S "<version>" -- Core/Core.csproj`.

## MyHotel

`SampleProjects/MyHotel/` is the reference application TSpec is developed against, not part of the
shipped package. It is layered per the Neat architecture — `MyHotel` (host), `Entry`, `Contract`,
`Core`, `Infra` — with two spec projects, `MyHotel.Spec` (black-box, HTTP) and `Core.Spec` (domain
rules). It has its own rules: read `SampleProjects/MyHotel/CLAUDE.md` before changing anything
under it.

Note `SampleProjects/MyHotel/Core/` is MyHotel's business layer and is unrelated to `Core/`, which
is TSpec itself.
