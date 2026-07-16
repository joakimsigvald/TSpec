# TSpec — instructions for coding agents

## Documentation must stay in sync with code

On every change to public API or observable behavior, update **both**:

- `README.md` — the full human documentation (also rendered on nuget.org)
- `TSpec-agent-reference.md` — the condensed agent reference shipped in the NuGet package

A code change is not complete until both documents reflect it. Also update
`PackageVersion` and `PackageReleaseNotes` in `Core/Core.csproj` when preparing a release
(docs/packaging-only = patch, new functionality = minor).

## Build and test

- Test project uses xunit v3 with an exe runner; `dotnet test` swallows output. Instead:
  `dotnet build Core.Test -f net10.0`, then run `Core.Test/bin/Debug/net10.0/TSpec.Test.exe`
  (filter with `-class Namespace.ClassName`).
- The library multi-targets net8.0/net9.0/net10.0 — run the full suite on all three before a release.
