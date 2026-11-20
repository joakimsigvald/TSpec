# MyHotel - Glossary

**Purpose:** the domain terms MyHotel uses that a reader must understand, and the vague concepts that
need a clear definition: the name to use in code, followed by what it means
(CODING-STANDARD-BACKEND.md 1b), and no detail the code or the specifications already hold.

Where the code still uses another word, it is renamed on touch; the glossary lists the target, not
the variants. The PR that introduces, renames or retires a concept updates its row. A row marked `*`
is provisional: the term is not used in code yet, and is the product owner's to rename.

| Term | Meaning |
|---|---|
| `Room` | A room the hotel can let |
| `RoomNumber` | A room's identity, which never changes: renumbering a room is deleting it and adding another |
| `BedCount` | How many beds a room has |
| `Rules` | The limits on what the hotel accepts, published for Web to offer its choices within |
| `RoomCategory` | A kind of room the hotel keeps, identified by its name. Every room has one |
| `Booking` | A room held for a guest for a period of nights, identified by its booking number |
| `BookingNumber` | A booking's identity, issued in order by the hotel and never by the caller |
| `GuestName` | Who a booking is for |
| `GuestCount` | How many guests a booking is for |
| `BookingPeriod` | A booking's nights: the guest arrives on `From` and departs on `To`. Nights are half-open, `[From, To)`, so the departure night is free for the next booking |
| `Overlaps` | Two bookings of one room share a night, neither of them cancelled |
| `Today` | The hotel's date: the server's, in its own time zone |
| `BookingStage` | What has happened to a booking, as stored: `Booked`, `CheckedIn`, `CheckedOut` or `Cancelled` |
| `BookingStatus` | `Open`, `CheckedIn`, `CheckedOut`, `NoShow`, `Past` or `Cancelled`: worked out from the booking's `BookingStage`, its dates and today |
| `Open` | A booking neither checked in nor cancelled whose arrival day has not passed |
| `CheckIn` | To mark an open booking checked in, on its arrival day |
| `CheckedIn` | A checked-in booking whose departure day has not passed |
| `CheckOut` | To mark a checked-in booking checked out, on its departure day |
| `CheckedOut` | A booking that was checked out, for good |
| `NoShow` | A booking whose arrival day passed while it was open, for good |
| `Past` | A checked-in booking whose departure day passed without a check-out |
| `Cancellable` | An open booking, or a no-show whose departure day has not passed |
| `Cancel` | To mark a cancellable booking cancelled. It is kept as a record, and its nights are free again |
| `Held` | A room an open or checked-in booking, or a cancellable no-show, still holds; it cannot be deleted |
| `Refusal` | A request the hotel turns down, answered with an error code |
| `OutOfService` * | A room temporarily off inventory, taking no new bookings. Hotel usage - *out of order* is maintenance |
