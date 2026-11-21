# Room price and payment

Every room has a price per night, every booking a price, and a booking is paid with "Betala".

- A room's price per night is a whole number of kronor, given when the room is added and changed
  like its beds. A price below 1 kr is refused.
- A booking gets its price per night from its room when it is made, and keeps it when the room's
  price changes. Its price is its nights times its price per night.
- A changed booking keeps its price per night when its nights change or it moves to a room in the
  same category. Moved to a room in another category, it gets that room's price per night.
- An open or checked-in booking that is not paid can be paid, all of it at once. Any other payment
  is refused.
- A paid booking stays paid when it is changed, even if its price rises, and can be cancelled.
- The rooms that already exist cost 1000 kr per night, and so do their bookings.

In the UI:

- The rooms table and the add-room form get "Pris per natt", changed with "Ändra" like Sängar.
- The bookings table gets "Pris", the booking's price.
- An open or checked-in booking's row gets "Betala" in its last column. Once paid, the row shows
  "Betald" in its place, immediately.

Not in this feature: paying part of the price, undoing a payment, refunding a cancelled booking.
