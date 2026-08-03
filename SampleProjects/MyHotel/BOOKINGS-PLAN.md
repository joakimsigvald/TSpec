# Bookings — working plan

Feature work on MyHotel, started 2026-08-03. What belongs here is what the code and
[`SPECIFICATION.md`](MyHotel.Spec/SPECIFICATION.md) cannot say: the product owner's rulings, the
order they are being built in, and what is still undecided. Delete this file when the last step
lands.

Generator-side consequences of this work — what the bigger documents exposed — live in
[SPEC-GENERATION-PLAN.md](../../SPEC-GENERATION-PLAN.md) §5, not here.

## Done

**1. The bookings resource.** `POST /bookings` (`201` + `Location`), `GET /bookings`,
`GET /bookings/{id}`, `DELETE /bookings/{id}`. Refusals: `400` when the period is not at least one
night, `404` for an unknown room or booking, `409` when the room is taken for any of those nights.

Decisions taken along the way, none of them recoverable from the code alone:

- **Nights are half-open, `[from, to)`.** The guest departs on `to`, so that night is free for the
  next booking and two stays meeting at a date do not collide. This is the overlap law `Core.Spec`
  states; the boundary case has its own branch so the rule cannot be weakened silently.
- **The id is the hotel's, not the caller's** — hence `BookingRequest` beside `Booking`. Ids are
  `max + 1` over the whole book, so a cancelled booking's number is never reused.
- **Bookings get their own store and file**, `IBookingStore`/`bookings.json` beside the room store.
  Whole-list load and save, like rooms, for the same reason: the rules stay in Core.
- **A cancelled booking frees its nights** — stated under `When book room`, not under cancelling,
  because the act that observes it is booking.
- **Core went vertical**: `Core/Rooms/` and `Core/Bookings/`, with both spec projects mirroring the
  production folders (`Core.Spec/Bookings/BookingService/`, `MyHotel.Spec/Bookings/`).

## Next, in order

2. **`GET /bookings?roomNumber=`** — one room's bookings, as a filter on the list endpoint rather
   than a route of its own.
3. **`PUT /bookings/{id}`** — amending a booking, re-stating the same refusals the booking endpoint
   makes. Not built with the resource, since cancel-and-rebook already covered it; the PO asked for
   it explicitly.
4. **Refuse `DELETE /rooms/{roomNumber}` while the room has bookings** (`409`). The first rule that
   crosses subdomains — worth watching what it does to Core's layering, since `RoomService` will
   need to see bookings.
5. **Out of service.** A room can be taken off inventory: `PUT /rooms/{n}/out-of-service` sets it,
   `DELETE /rooms/{n}/out-of-service` reverses it, and the state shows in `GET`. The term is hotel
   usage — *out of order* is maintenance, *out of service* is temporarily off inventory — and is
   the PO's to rename once it can be read in the document. A room out of service takes no new
   bookings; what happens to bookings it already has is **undecided**.

## Open questions

Three readings of the current text, put to the PO 2026-08-03 and not yet answered:

- **Heading names disagree between subjects.** `Core.Spec` reads `When book`, `When get`,
  `When list`, `When cancel` for bookings but `When add room`, `When get room` for rooms. Now that
  the folder names the subject, either the room headings are redundant or the booking ones are too
  thin — but they should not differ.
- **`new(2026, 8, 10)` hides its type.** Target-typed `new` renders as written, so a reader cannot
  see it is a `DateOnly`. Spelling `new DateOnly(2026, 8, 10)` in the spec source would say so, at
  some verbosity in every dated clause.
- **A refused input becomes its own section.** `When book zero nights` sits beside `When book room`
  rather than under it, because one `When` per spec class means a branch cannot vary the act.
  Making the dates a tag would buy the branch structure and cost the heading its values.
