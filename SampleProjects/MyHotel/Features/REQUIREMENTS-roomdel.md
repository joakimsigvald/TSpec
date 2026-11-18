# Deleting a room with bookings

A room can't be deleted while guests hold it, and a room's bookings can be cancelled in one go.
Past and cancelled bookings are kept as a record.

A booking is **past** when it departs before today, **future** when it arrives after today, and
**current** when it arrives today or earlier and departs today or later. A room's **open** bookings
are its current and future bookings that aren't cancelled.

- Cancelling keeps the booking: it stays in the bookings list, shown as "Avbokad" where its
  "Avboka" button was, and its nights can be booked again.
- A past or cancelled booking can't be cancelled, and the list shows no "Avboka" for it.
- "Avboka alla", a button on each room's row next to "Ta bort", cancels the room's open bookings,
  after asking, under the caption "Avboka alla bokningar", "Avboka alla aktuella och kommande
  bokningar för rum 101?". They show as "Avbokad" in
  the bookings list immediately. On a room without open bookings it is disabled, with the tooltip
  "Rummet har inga aktuella eller kommande bokningar.".
- "Ta bort" asks "Ta bort rum 101?", under the caption "Ta bort rum", before deleting. On a room with open bookings it is disabled,
  with the tooltip "Rummet har aktuella eller kommande bokningar.", and a delete refused all the
  same shows that text above the rooms table.
- Both buttons change state immediately when a booking is made or cancelled.
- A deleted room's past and cancelled bookings stay in the bookings list.

Not in this feature: changing the status of a past booking, an admin function with permissions of
its own.
