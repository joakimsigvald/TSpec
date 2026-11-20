# Check-in and check-out

A guest is checked in on the day they arrive and checked out on the day they depart, and a guest
who never arrives is a no-show.

A booking is **open** until its arrival day has passed, while it is neither checked in nor
cancelled. Once checked in it is **checked in**, and once checked out it is **checked out** for
good. An open booking whose arrival day passes becomes a **no-show** for good. A checked-in booking
whose departure day passes without a check-out is **past**.

- An open booking can be checked in on its arrival day, and a checked-in booking checked out on its
  departure day. Any other check-in or check-out is refused.
- An open booking can be cancelled, and so can a no-show until its departure day has passed.
  Cancelling any other booking is refused.
- A room can't be deleted while it has an open or checked-in booking, or a no-show that can still
  be cancelled.
- "Avboka alla" cancels the room's open bookings and the no-shows that can still be cancelled.
- The bookings that already exist and whose arrival day has passed are checked in.

In the UI, the last column of the bookings table shows:

- for an open booking, "Avboka", and on its arrival day also "Checka in";
- for a checked-in booking, "Incheckad", and on its departure day also "Checka ut";
- for a checked-out booking, "Utcheckad";
- for a no-show, "No show", and "Avboka" until its departure day has passed.

A check-in or check-out shows on its row immediately, and so does its effect on the room's
"Avboka alla" and "Ta bort". "Avboka alla" gets two new texts:

- its tooltip when disabled: "Rummet har inga aktuella eller kommande bokningar." becomes
  "Rummet har inga bokningar att avboka.";
- its question: "Avboka alla aktuella och kommande bokningar för rum 101?" becomes
  "Avboka alla bokningar som går att avboka för rum 101?".

Not in this feature: checking in after the arrival day or out before the departure day; undoing a
check-in or check-out.
