# TSpec — instructions for coding agents

## Documentation must stay in sync with implemented features

Two documents describe TSpec to its users:

- `README.md` — the full human documentation (also rendered on nuget.org)
- `TSpec-agent-reference.md` — the condensed agent reference shipped in the NuGet package

Update one when a change alters what its reader must know to write a test correctly: new or changed
surface, a semantic they would otherwise get wrong, a behaviour that would surprise them. 

Update `PackageVersion` and `PackageReleaseNotes` in `Core/Core.csproj` when preparing a release
(docs/packaging-only = patch, new functionality = minor).

## Working cycle

Test first: write the failing test, run it and see it fail for the right reason, then implement.
Once it passes, refactor what you touched to clean code, keep the suite green, then report.

## Code style

- Short methods that do one thing, on one level of abstraction.
- No clever constructs: don't make one construct do too much, and keep a top-level decision apart
  from the handling of each case.
- A class that needs section comments should be split; propose the split and ask before doing it.
- Comments: none by default. Write one only where a reader would otherwise change the code and break
  something — a constraint, an invariant, a rejected alternative — never a `<summary>` restating a
  well-named member. The same restraint holds for release notes, plan entries and test comments: one
  line unless a second earns its place.
- An early return puts the condition on its own line and the return, indented, on the next, followed
  by an empty line — never `if (x) return y;` on one line.
- A `TryX` method returns `bool` with an `out` parameter, never a sentinel value.

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

## MyHotel

`SampleProjects/MyHotel/` is the reference application TSpec is developed against, not part of the
shipped package. It is layered per the Neat architecture — `MyHotel` (host), `Entry`, `Contract`,
`Core`, `Infra` — with two spec projects, `MyHotel.Spec` (black-box, HTTP) and `Core.Spec` (domain
rules). It has its own rules: read `SampleProjects/MyHotel/CLAUDE.md` before changing anything
under it.

Note `SampleProjects/MyHotel/Core/` is MyHotel's business layer and is unrelated to `Core/`, which
is TSpec itself.
