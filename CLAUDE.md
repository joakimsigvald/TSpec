# TSpec — instructions for coding agents

## Documentation must stay in sync with implemented features

Two documents describe each package to its users, TSpec's at the root and TSpec.Architecture.CSharp's
in `Architecture.CSharp/`:

- `README.md` — the full human documentation (also rendered on nuget.org)
- `agent-reference.md` — the condensed agent reference shipped in the package

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
- `dotnet test Analyzers.Test` runs the specs of the analyzers shipped in the package, on net10.0
  only: they run inside the compiler, so the test's runtime makes no difference.
- `dotnet test Architecture.CSharp.Test` runs the specs of the package `TSpec.Architecture.CSharp` on
  all three frameworks. The package reaches TSpec through its public API only, as any extension package
  would — never `InternalsVisibleTo`; what it lacks, TSpec adds as public API, in `TSpec.Extensibility`
  where only an extension package needs it. It references TSpec from NuGet, so an extension point it
  needs is released in TSpec first.

## Releasing

1. Update `PackageVersion` and `PackageReleaseNotes` in `Core/Core.csproj`, and the agent
   reference's "covers TSpec x.y" line — it ships inside the package and had gone four minor
   versions stale by 1.5.0.
2. Run the full suite on all three target frameworks.

`TSpec.Architecture.CSharp` is released on its own, slower cadence and version: update the same two
properties in `Architecture.CSharp/Architecture.CSharp.csproj` and its agent reference's "covers"
line, and run its suite on all three frameworks. TSpec keeps its public API, `TSpec.Extensibility` included, stable within a major
version, or the package breaks on a TSpec minor.

## MyHotel

`SampleProjects/MyHotel/` is the reference application TSpec is developed against, not part of the
shipped package. It is layered per the Neat architecture — `MyHotel` (host), `Entry`, `Contract`,
`Core`, `Infra` — with three spec projects, `MyHotel.Spec` (black-box, HTTP), `Core.Spec` (domain
rules) and `Architecture.Spec` (the project graph). It has its own rules: read `SampleProjects/MyHotel/CLAUDE.md` before changing anything
under it.

Note `SampleProjects/MyHotel/Core/` is MyHotel's business layer and is unrelated to `Core/`, which
is TSpec itself.
