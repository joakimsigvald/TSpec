# MyHotel - Glossary

One term per domain concept: the name to use in code, followed by what it means (CODING-STANDARD-BACKEND.md 1b).
Where the code still uses another word, it is renamed on touch; the glossary lists the target, not the
variants. The PR that introduces, renames or retires a concept updates its row. A row marked `*` is
provisional: the term is not used in code yet, and is the product owner's to rename.

| Term | Meaning |
|---|---|
| `Room` | A room the hotel can let: its room number and its bed count. Rooms are kept in the order they were added |
| `RoomOverview` | A room as the hotel answers with it: the room, and whether it has open bookings, which the hotel works out and a caller never sends |
| `RoomNumber` | A room's identity, a string (`"101"`): it appears in the URL and never changes. Renumbering a room is deleting it and adding another; an update that carries another number is a `RoomNumberMismatch` |
| `BedCount` | How many beds a room has. The UI calls it Beds |
| `Booking` | A room held for a guest for a period of nights, identified by its booking number. Bookings are kept in the order they were made |
| `BookingRequest` | What a caller books with: a booking without its number, which the hotel assigns, and its status |
| `StoredBooking` | A booking as the store keeps it: whether it was cancelled, but not its status, which changes with the date |
| `BookingStatus` | `Open`, `Past` or `Cancelled`. Worked out from the booking's dates and today whenever it is read, except that it was cancelled |
| `Today` | The hotel's date: the server's, in its own time zone, read through an injected `TimeProvider` (8e) |
| `Open` | A booking that is not cancelled and is current - arrives today or earlier and departs today or later - or future - arrives after today. A room with open bookings cannot be deleted (`RoomHasOpenBookings`) |
| `Past` | A booking that departed before today. Kept as a record, and cannot be cancelled |
| `BookingNumber` | A booking's identity, issued in order by the hotel and never by the caller. A number is spent when it is issued: cancelling does not return it, and a restart carries on from the last one used |
| `GuestName` | Who a booking is for. The UI calls it Guest. Personal data: never logged (8d) |
| `From`, `To` | A booking's period: the guest arrives on `From` and departs on `To`. Nights are half-open, `[From, To)`, so the departure night is free for the next booking; a period lasts at least one night, or it is an `InvalidBookingPeriod` |
| `Overlaps` | Two bookings of one room share a night, neither of them cancelled. A booking that overlaps another is refused as `RoomAlreadyBooked`; two stays meeting at a date do not overlap |
| `Book` | To make a booking: the room exists, the period is valid, no booking overlaps it, and it gets the next booking number |
| `Cancel` | To mark an open booking cancelled. It is kept as a record and its nights are free again; its number is not. A past or cancelled booking is refused as `BookingNotOpen` |
| `CancelOpenBookings` | To cancel a room's open bookings at once, leaving its past and cancelled ones as they are. The UI calls it Avboka alla |
| `BookingNumberSeed` | Where the hotel starts its numbering: `LastUsed`, the number counted as already used, so the first booking gets the one after it. Configured as `BookingNumbers:Seed`, 10000 as shipped, 0 when unset |
| `LastUsedNumber` | The last booking number issued, kept with the bookings so numbering survives a restart. Null until the first is issued - the one moment the seed applies |
| `Refusal` | A request the hotel turns down: a status and an error code, the name of the Contract exception it stands for (`RoomNotFound`, `BookingNotFound`, ...), sent as an `ErrorBody`. Web shows the code's text (1i) |
| `OutOfService` * | A room temporarily off inventory: it takes no new bookings, and shows as such in `GET`. Hotel usage - *out of order* is maintenance. What becomes of its existing bookings is undecided (BACKLOG.md, item 3) |
