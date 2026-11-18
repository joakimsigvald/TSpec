# Number of guests in a booking — design

Built in every layer, Web included. The subdomain graph does not change: Bookings already reads the
rooms.

**A booking's dates become one field, its period.** A booking already has six fields, the most a
record may have (CODING-STANDARD-BACKEND.md 3d), so adding the guest count would make seven. `From`
and `To` therefore become one field, a `BookingPeriod`, the name the period already has in the
refusal `InvalidBookingPeriod` and in Web. A booking, a booking request and a stored booking each hold one, so the dates are
written one way everywhere. On the wire a booking reads
`{ bookingNumber, roomNumber, guestName, guestCount, period: { from, to }, status }`.

**A booking request carries the guest count**, and the booking answered carries it back.

**Two new refusals:**

- `InvalidGuestCount`, `400`, for fewer than one guest, checked with the period.
- `TooManyGuests`, `409`, for more guests than the room has beds, checked once the room is found.

**The bookings in your `bookings.json` are rewritten once, by hand**, to the new shape with one
guest each: no code handles a booking stored the old way.

**Web's booking form gets its new order**, with the period first, and a number field for the guest
count. Both refusals show under it. The bookings table gets the "Gäster" column, and the texts are
added to `sv.json`.

**`GLOSSARY.md` gets `GuestCount`, and `BookingPeriod` in place of `From`, `To`.**
