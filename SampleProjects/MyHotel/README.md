# MyHotel

A small hotel-booking API — the reference application TSpec is developed against, not a shipped
product.

TSpec's own `Core.Test` is a framework testing itself: its specifications describe the TSpec API
rather than a domain, so they say little about how TSpec reads when applied to real software.
MyHotel is the counterpart — an ordinary ASP.NET Core minimal API, driven out entirely by
specifications — and the proving ground for `SPECIFICATION.md` generation
([SPEC-GENERATION-PLAN.md](../../SPEC-GENERATION-PLAN.md)).

Being layered per **Neat**: `Contract` is the public shape and references nothing, `Entry` holds the
endpoints, `Core` holds the logic, and `MyHotel` is the host that wires them together. Entry and Core
cannot see each other, so nothing internal can leak out through the API by accident. Two spec
projects state different things: [`MyHotel.Spec`](MyHotel.Spec) the HTTP contract, and
[`Core.Spec`](Core.Spec) the domain rules. Development rules are in [CLAUDE.md](CLAUDE.md).

## Running it

```bash
dotnet run --project SampleProjects/MyHotel/MyHotel
```

Opens [Scalar](https://scalar.com) at `/scalar`, an interactive UI over the OpenAPI document at
`/openapi/v1.json`. In Visual Studio, set `MyHotel` as the startup project — VS keeps that choice
in `.vs/`, which is not under source control.

## Running the specifications

`dotnet test` swallows the xunit v3 runner's output, so run the executable directly:

```bash
dotnet build SampleProjects/MyHotel/MyHotel.Spec -f net10.0
```

```bash
SampleProjects/MyHotel/MyHotel.Spec/bin/Debug/net10.0/MyHotel.Spec.exe
```

A green run regenerates [`MyHotel.Spec/SPECIFICATION.md`](MyHotel.Spec/SPECIFICATION.md). It is a
generated file — review it in diffs, never edit it by hand. Its version comes from `<Version>` in
[MyHotel.csproj](MyHotel/MyHotel.csproj).

## Endpoints

| Method | Path | Returns |
|---|---|---|
| `GET` | `/version` | `{ "version": "0.1.0" }` — read from the assembly, so it tracks `<Version>` in [MyHotel.csproj](MyHotel.csproj) |
| `GET` | `/rooms` | `200` with every room in the order it was created; `[]` when there are none |
| `POST` | `/rooms` | `201` with the room and a `Location` header; `409` if the room number is taken |
| `GET` | `/rooms/{roomNumber}` | `200` with the room, or `404` |
| `PUT` | `/rooms/{roomNumber}` | `200` with the updated room, or `404` |
| `DELETE` | `/rooms/{roomNumber}` | `204` and the room is gone, or `404` |
| `GET` | `/bookings` | `200` with every booking in the order it was made; `[]` when there are none |
| `POST` | `/bookings` | `201` with the booking and a `Location` header; `400` if the period is not at least one night; `404` if there is no such room; `409` if the room is already booked for any of those nights |
| `GET` | `/bookings/{id}` | `200` with the booking, or `404` |
| `DELETE` | `/bookings/{id}` | `204` and the booking is cancelled, or `404` |

A room is `{ "roomNumber": "101", "bedCount": 2 }`. The room number is the identity: it appears in
the URL and cannot be changed — to renumber a room, delete it and add a new one.

A booking is
`{ "id": 1, "roomNumber": "101", "guestName": "Smith", "from": "2026-08-10", "to": "2026-08-12" }`.
It is booked with the same fields minus the `id`, which the hotel assigns. Nights are half-open,
`[from, to)`: the guest departs on `to`, so that night is free for the next booking and two stays
meeting at a date do not collide.

Rooms and bookings are each kept in a JSON file, in creation order, so they outlive the process.

`400`, `404` and `409` carry `{ "error": "…" }`. They come from exceptions `Core` throws and
`Contract` declares; anything else reaches the pipeline unhandled and answers `500`.
