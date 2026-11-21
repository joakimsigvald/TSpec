# Room price and payment — design

Built in every layer, Web included. The project and subdomain graphs do not change: Bookings
already reads the rooms.

**A room gets its price per night**, a whole number of kronor, sent and answered with the room: on
the wire `{ roomNumber, bedCount, roomCategory, pricePerNight }`. A new refusal,
`InvalidPricePerNight`, `400`, for a price below 1 kr when a room is added or updated, is checked
right after its beds. The minimum is a constant in Core, the only place that needs it.

**A booking stores its price per night**, taken from its room when it is booked. A change keeps it,
unless the new room's category differs from the old room's: then it takes the new room's price. The
categories compared are the rooms' at the time of the change, and a room that no longer exists
counts as another category.

**A booking stores whether it is paid**, beside what has happened to it, since a paid booking is
still checked in, checked out or cancelled.

**A booking answers its price and whether it is paid, grouped as its payment:**
`{ bookingNumber, info, period, state, payment: { price, paid } }`. The price is worked out when it
is read, its nights times its price per night. Booking and changing carry no price: the hotel sets
it.

**Paying is a booking action, `Pay`**, which an open or checked-in booking allows while it is not
paid, so Web shows "Betala" from the actions as it does "Checka in". A new endpoint,
`POST /bookings/{bookingNumber}/pay`, answers `204` as checking in does, and refuses any other
payment with `409 BookingNotPayable`.

**Your `rooms.json` and `bookings.json` are rewritten once, by hand**: every room and booking gets
1000 kr per night, and no booking is paid. No code handles one stored without a price.

**In Web:**

- the rooms table gets "Pris per natt" after "Sängar", the add-room form a number field for it, and
  a room's editing row the same field;
- in the add-room form the field starts empty, and "Lägg till rum" is dimmed until it is filled;
- the bookings table gets "Pris" after "Till";
- prices show in Swedish format: "3 000 kr";
- a paid booking's row shows "Betald" where "Betala" stood, before "Ändra";
- a payment reloads the bookings and the rooms, as every booking change does.

**Texts in `sv.json`:** "Pris per natt", "Pris per natt för rum 101" (the field's accessible name on
a row), "Pris", "Betala" and "Betald"; and the refusals "Otillåtet pris per natt.", under the price
field, and "Bokningen kan inte betalas.", above the bookings table as a refused cancel is.

**In `GLOSSARY.md`, PricePerNight, Price, Pay and Paid become terms.** README's "What it does"
follows.
