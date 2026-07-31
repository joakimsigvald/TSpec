# MyHotel

A small hotel-booking API — the reference application TSpec is developed against, not a shipped
product.

TSpec's own `Core.Test` is a framework testing itself: its specifications describe the TSpec API
rather than a domain, so they say little about how TSpec reads when applied to real software.
MyHotel is the counterpart — an ordinary ASP.NET Core minimal API, driven out entirely by
specifications in [`MyHotel.Spec`](../MyHotel.Spec) — and the proving ground for `SPECIFICATION.md`
generation ([SPEC-GENERATION-PLAN.md](../SPEC-GENERATION-PLAN.md)).

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

A green run regenerates [`MyHotel.Spec/SPECIFICATION.md`](../MyHotel.Spec/SPECIFICATION.md). It is a
generated file — review it in diffs, never edit it by hand. Its version comes from `<Version>` in
[MyHotel.csproj](MyHotel.csproj).

## Endpoints

| Method | Path | Returns |
|---|---|---|
| `GET` | `/version` | `{ "version": "0.1.0" }` — read from the assembly, so it tracks `<Version>` in [MyHotel.csproj](MyHotel.csproj) |
| `GET` | `/rooms` | `200` with every room in the order it was created; `[]` when there are none |
| `POST` | `/rooms` | `201` with the room and a `Location` header; `409` if the room number is taken |
| `GET` | `/rooms/{roomNumber}` | `200` with the room, or `404` |
| `PUT` | `/rooms/{roomNumber}` | `200` with the updated room, or `404` |
| `DELETE` | `/rooms/{roomNumber}` | `204` and the room is gone, or `404` |

A room is `{ "roomNumber": "101", "bedCount": 2 }`. The room number is the identity: it appears in
the URL and cannot be changed — to renumber a room, delete it and add a new one. Rooms are held in
memory, in creation order, and are gone when the process stops.

Still to come: delete, update.
