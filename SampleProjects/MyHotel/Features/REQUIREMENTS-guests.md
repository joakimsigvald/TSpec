# Number of guests in a booking

A booking holds how many guests it is for, and the booking form puts the period first.

- A booking is made for at least one guest and at most as many guests as the room has beds;
  otherwise it is refused.
- The bookings that already exist are for one guest.

In the UI:

- The booking form's fields come in this order: Från and Till with their buttons, "Rumsnummer",
  "Antal gäster", "Gäst", then "Boka".
- "Antal gäster" is an empty number field, as "Sängar" is when adding a room, and booking is
  stopped by the form until it is filled in. A refusal of the number shows under it: "En bokning
  måste gälla minst en gäst." or "Rummet har för få sängar."
- The bookings table gets a "Gäster" column after "Rum".
