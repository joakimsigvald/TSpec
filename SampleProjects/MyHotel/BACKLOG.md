# MyHotel — backlog

The feature work TSpec's documents are grown against. Moved here from the 2.0.0 generator working
notes when that file was retired; rules for changing anything under `SampleProjects/MyHotel/` are in
[CLAUDE.md](CLAUDE.md).

## Built: the bookings resource

- `POST /bookings` (`201` + `Location`), `GET /bookings`, `GET /bookings/{bookingNumber}`,
  `DELETE /bookings/{bookingNumber}`.
- Refusals: `400` (period under one night), `404` (unknown room or booking), `409` (room already
  taken for those nights).
- Nights are half-open, `[from, to)` — adjacent stays don't collide; the boundary has its own
  branch in both suites.
- `Booking.Id` renamed `BookingNumber` throughout (Contract, Core, Entry, both spec projects).
- Numbers come from `IBookingNumberGenerator`, seeded via `BookingNumbers:Seed` (10000 shipped, 0
  default). `BookingNumberSeed.LastUsed` is the number already counted as used, so `Next()` is
  `(LoadLastUsed() ?? seed.LastUsed) + 1`. Never re-seeds or decrements — a cancelled booking's
  number is not returned, since nothing here reads the bookings at all.
- `BookingStore` persists both bookings and the counter in one JSON file (`{ Bookings,
  LastUsedNumber }`), so numbering survives a restart with the bookings it counts.
- `MyHotel.Spec` asserts against the shipped seed rather than mocking — that suite states the HTTP
  surface, not every wrinkle of numbering. `Core.Spec` covers the generator directly, plus a
  two-call mock sequence proving the second booking gets the second number.
- Cancelling frees its nights, stated under booking rather than cancelling — the act that observes
  it is booking.
- Core went vertical: `Core/Rooms/`, `Core/Bookings/`.

## Next, in the order the PO set

1. **`GET /bookings?roomNumber=`** — one room's bookings, as a filter on the list endpoint rather
   than a route of its own.
2. **`PUT /bookings/{bookingNumber}`** — amending a booking, re-stating the refusals booking makes.
   Left out of the resource because cancel-and-rebook covered it; asked for explicitly.
3. **Refuse `DELETE /rooms/{roomNumber}` while the room has bookings** (`409`). The first rule
   crossing subdomains — watch what it does to Core's layering, since rooms must consult bookings.
4. **Out of service.** `PUT /rooms/{n}/out-of-service` sets it, `DELETE` on the same path reverses
   it, and the state shows in `GET`. The term is hotel usage — *out of order* is maintenance, *out
   of service* is temporarily off inventory — and is the PO's to rename once it can be read in the
   document. A room out of service takes no new bookings; what becomes of the bookings it already
   has is **undecided and needs a ruling before this is built**.

## Open questions for the PO

Also listed in [SPECIFICATION-IMPROVEMENT-PLAN.md](../../SPECIFICATION-IMPROVEMENT-PLAN.md) §6,
since each is answered by looking at the generated document.

- Whether `Core.Spec`'s booking headings (`When book`, `When get`) should match the rooms' fuller
  style (`When add room`).
- Whether `new(2026, 8, 10)` should say `new DateOnly(…)` so a reader can see the type.
- Whether a refused input should stay its own section or become a branch, by making the varying
  value a tag. One `When` per class is what forces sibling sections today; every refused-input case
  meets it.
