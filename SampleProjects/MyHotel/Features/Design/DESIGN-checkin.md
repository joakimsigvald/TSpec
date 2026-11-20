# Check-in and check-out — design

Built in every layer, Web included. The subdomain graph does not change: Rooms keeps asking
Bookings through the interface it declares.

**A booking stores what has happened to it: booked, checked in, checked out or cancelled**, in
place of whether it was cancelled, so it can never be both cancelled and checked in. Its status is
worked out from that, its dates and today whenever it is read, so it changes by itself:

- booked: open until its arrival day has passed, then a no-show;
- checked in: checked in until its departure day has passed, then past;
- checked out and cancelled stay as they are.

**Your `bookings.json` is rewritten once, by hand**: a booking that isn't cancelled and whose
arrival day has passed is stored as checked in. No code handles a booking stored the old way.

**A booking states what can be done with it today: cancel, check in or check out.** Web shows a
button for each, so it repeats none of the rules about days (CODING-STANDARD-FRONTEND.md 6d).

**A booking's fields are grouped:** its room number, guest name and guest count become its info,
and its status and actions its state. On the wire a booking reads
`{ bookingNumber, info: { roomNumber, guestName, guestCount }, period: { from, to }, state: { status, actions } }`.

**Two new endpoints, `POST /bookings/{bookingNumber}/check-in` and `.../check-out`**, answering
`204` as cancelling does. Each refuses what can't be done today with `409`: `NotDueForCheckIn`,
`NotDueForCheckOut`.

**Cancelling takes the cancellable bookings: the open ones, and the no-shows until their departure
day has passed.** That holds for one booking and for a room's bookings at once. Since a no-show can
be cancelled without being open, the refusal `BookingNotOpen` is renamed `BookingNotCancellable`.

**A room states two things instead of one: whether it is held, and whether it has bookings to
cancel.** Held means a booking still holds it: an open or checked-in booking, or a cancellable
no-show. "Ta bort" is disabled while the room is held, "Avboka alla" while it has nothing to
cancel. Deleting a held room is refused as before, but since a checked-in guest is not an open
booking, `RoomHasOpenBookings` is renamed `RoomHeld`. On the wire, `hasOpenBookings` becomes `held`
and `hasCancellableBookings`.

**A check-in or check-out reloads the bookings and the rooms**, as every booking change already
does.

**Texts in `sv.json`:**

- new: "Checka in", "Checka ut", "Incheckad", "Utcheckad" and "No show";
- new refusals, shown above the bookings table as a refused cancel is: "Bokningen kan inte checkas
  in." and "Bokningen kan inte checkas ut.";
- a refused cancel, which now also covers checked-in and checked-out bookings: "Bokningen är redan
  avbokad eller passerad." becomes "Bokningen kan inte avbokas.";
- the two "Avboka alla" texts the requirements give.

**In `GLOSSARY.md`, Open, Past and Cancel change meaning, and CheckIn, CheckOut, CheckedIn,
CheckedOut, NoShow, Cancellable and Held become terms.** README's "What it does" follows.
