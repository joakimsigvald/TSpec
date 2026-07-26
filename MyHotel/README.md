# MyHotel

A small hotel-booking API — the reference application TSpec is developed against, not a shipped
product.

TSpec's own `Core.Test` is a framework testing itself: its specifications describe the TSpec API
rather than a domain, so they say little about how TSpec reads when applied to real software.
MyHotel is the counterpart — an ordinary ASP.NET Core minimal API, driven out entirely by
specifications in [`MyHotel.Spec`](../MyHotel.Spec) — and the proving ground for `SPECIFICATION.md`
generation ([SPEC-GENERATION-PLAN.md](../SPEC-GENERATION-PLAN.md), Phase 3).

Deliberately simplistic: one project, no layering, everything in `Program.cs` until the code
objects, and specs that test over HTTP rather than reaching inside. Development rules are in
[CLAUDE.md](CLAUDE.md).

## Running it

```bash
dotnet run --project MyHotel
```

Opens [Scalar](https://scalar.com) at `/scalar`, an interactive UI over the OpenAPI document at
`/openapi/v1.json`. In Visual Studio, set `MyHotel` as the startup project — VS keeps that choice
in `.vs/`, which is not under source control.

## Running the specifications

`dotnet test` swallows the xunit v3 runner's output, so run the executable directly:

```bash
dotnet build MyHotel.Spec -f net10.0
```

```bash
MyHotel.Spec/bin/Debug/net10.0/MyHotel.Spec.exe
```

## Endpoints

| Method | Path | Returns |
|---|---|---|
| `GET` | `/version` | `{ "version": "0.1.0" }` |
