# Check-in and check-out — review

1. **`WhenCheckOut` does not state that a past booking is refused.** The requirements allow a
   check-out on the departure day only. `WhenCheckIn` states both sides of its day, arrives tomorrow
   and arrived yesterday, but `WhenCheckOut` only the day before. Add `GivenTheBookingIsPast`, checked
   in with `Period = new(EarlierDay, Yesterday)`, that throws `NotDueForCheckOut`.

2. **`BookingStage` is a new concept the glossary lacks** (GLOSSARY.md: the PR that introduces a
   concept updates its row). It is what the store keeps and what `Core.Spec`'s givens read
   (`Stage = BookingStage.CheckedIn`). It shares `CheckedIn` with `BookingStatus` in another sense:
   the stage `CheckedIn` becomes the status `Past` once the departure day has passed. Add a row
   `BookingStage`: "What has happened to a booking, as stored: `Booked`, `CheckedIn`, `CheckedOut`
   or `Cancelled`". Let the `BookingStatus` row say it is worked out from the booking's
   `BookingStage`, its dates and today.

3. **`IRoomBookings.HeldRooms` has a summary that repeats the glossary's `Held`.** The member is
   now named for the glossary term, and the glossary defines it (CODING-STANDARD.md I,
   CODING-STANDARD-BACKEND.md 9e). Its sibling `RoomsWithCancellableBookings`, named for
   `Cancellable` the same way, has no summary. Delete the member summary.

4. **In the bookings table, a status runs into the button after it.** Tried in the browser,
   "Incheckad" and "No show" touch "Checka ut" and "Avboka", and the row reads
   "IncheckadChecka ut". Set the status apart from the buttons with a rule in `styles.css`
   (CODING-STANDARD-FRONTEND.md 7c).

5. **`queries.ts`'s comment was rewrapped only in part.** Its second line runs to 130 characters,
   while the rest wrap at about 100. Rewrap the comment.
