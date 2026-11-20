# Check-in and check-out — build notes

Built. `Core.Spec`, `Host.Spec`, `Architecture.Spec` and Web's tests are green, and the new specs
were seen failing first: Core on missing states, saves and refusals, Host on the missing routes and
refusal codes, and Web on the old wire shape. `Host/bookings.json` was rewritten by hand: 10006,
10007 and 10008 are checked in, 10009 is booked and the three cancelled bookings stay cancelled.

Deviations from the design:

- **Two more renames, for the same reason as the design's own.** Once Open changes meaning,
  `CancelOpenBookings` and `IOpenBookings` would no longer be true (CODING-STANDARD-BACKEND.md 1b).
  So `IBookingService.CancelOpenBookings` is now `CancelBookings`: its specs are `WhenCancelBookings`
  in both suites, and Web's call is `cancelRoomBookings`. Core's `IOpenBookings.RoomNumbers()` is
  now `IRoomBookings`, with `HeldRooms()` and `RoomsWithCancellableBookings()`.
- **CODING-STANDARD-FRONTEND.md 4a** gave "a room states whether it has open bookings" as its
  example. It now reads "whether a booking holds it".
