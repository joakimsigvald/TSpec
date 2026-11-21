# Room price and payment — build notes

Built. `Core.Spec`, `Host.Spec`, `Architecture.Spec` and Web's tests and lint are green, and the new
specs were seen failing first: Core on stubbed prices and a `Pay` that did nothing, Host on the
missing route (404) and the unmapped refusal (500), Web on the missing fields, columns and buttons.
`WhenCancel`'s paid booking passed from the start and was seen failing when cancelling refused a
paid booking. `Core.Spec`'s `BookingServiceSpec` now leaves every booking unpaid unless a
specification says otherwise, so `WhenGet`'s open and checked-in states name `Pay` too.
`Host/rooms.json` and `Host/bookings.json` were rewritten by hand: 1000 kr per night everywhere,
nothing paid.

Deviations from the design:

- **`main` is 54rem wide, not 48rem.** Tried in the browser on a copy of the PO's data, the "Pris"
  column made the bookings table need about 814px of 768: every date broke at its hyphens, and a
  checked-in row's "Ändra" wrapped under "Checka ut" and "Betala". While a booking is being changed,
  the other rows still wrap, as they did before this feature at the old width.
- **A status keeps a gap from a button or a status before it**, as it already did from a button
  after it: "Incheckad" and "Betald" ran together.
- **"Spara" on a room's row is dimmed while its price field is empty**, as on a booking's row; the
  design dims only "Lägg till rum". Otherwise an emptied field is sent as 0 kr.
- **A booking being changed shows its price, unchanged, in the "Pris" column**, which the design
  leaves open for that row.
