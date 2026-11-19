# Room dropdown when booking

The booking form's room-number text field becomes a dropdown of the hotel's rooms.

- Each option is a room number, in alphanumeric order: 2, 12, 12A, 101.
- Until a room is chosen it shows "Välj rum", and booking is stopped by the form, as an empty field
  is today. After a booking it shows "Välj rum" again.
- A room added or deleted appears in or leaves the dropdown immediately.
- The label stays "Rumsnummer", and a refusal concerning the room (no such room, already booked)
  still shows under it.
