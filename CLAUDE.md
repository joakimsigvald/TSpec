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

- Test project uses xunit v3 with an exe runner; `dotnet test` swallows output. Instead:
  `dotnet build Core.Test -f net10.0`, then run `Core.Test/bin/Debug/net10.0/TSpec.Test.exe`
  (filter with `-class Namespace.ClassName`).
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
