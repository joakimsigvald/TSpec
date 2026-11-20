# Changing a booking

Every booking can be changed: its room, number of guests, guest and period. Its booking number
stays.

- A change is refused on the same grounds as a new booking: no night, no guest, more guests than
  the room has beds, a room that doesn't exist, or a room another booking already holds for some of
  the nights. A cancelled booking holds no nights, so it can be moved onto nights that are booked.
- A changed booking is still checked in, checked out or cancelled if it was, and what can be done
  with it follows its new dates: an open booking moved to arrive today can be checked in.

In the UI:

- Every row of the bookings table gets "Ändra", last in its last column.
- "Ändra" turns the row's Rum, Gäster, Gäst, Från and Till into fields holding the booking's
  values: Rum a dropdown of the rooms as in the booking form, Från and Till date fields without
  the buttons. The last column then holds "Spara" and "Avbryt".
- "Spara" is dimmed until something is changed. A saved change shows on the row immediately, and so
  does its effect on the rooms' "Avboka alla" and "Ta bort". A refusal shows under its field with
  the booking form's texts, and the row stays open for editing.
- "Avbryt" shows the row as it was.
