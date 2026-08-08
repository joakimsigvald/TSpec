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

## Open question for the PO

**Should a refused input stay its own section, or become a branch?** A class holds one `When`, so an
act that varies has to be its own class — which is why `Core.Spec` states booking as three sibling
sections that differ only in the dates:

```
### When book
Book(new BookingRequest(a Room's RoomNumber, a string, new(2026, 8, 10), new(2026, 8, 12)))
### When book zero nights
Book(new BookingRequest(a string, a second string, a DateOnly, the DateOnly))
### When book departure before arrival
Book(new BookingRequest(a string, a second string, new(2026, 8, 12), new(2026, 8, 10)))
```

Making the dates tags would give one `### When book` with `#### Given zero nights` and `#### Given
departure before arrival` under it, so the refusals read as conditions on booking rather than as
three acts. Nothing in TSpec changes either way — it is how the specs are written. Every
refused-input case meets this, so the answer sets a pattern for the suite.
