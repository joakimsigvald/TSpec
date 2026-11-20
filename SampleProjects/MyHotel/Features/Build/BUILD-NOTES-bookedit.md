# Changing a booking — build notes

Built. `Core.Spec`, `Host.Spec`, `Architecture.Spec` and Web's tests are green, and the new specs
were seen failing first: Core on a stubbed `Change`, Host on the missing route (405), Web on the
missing "Ändra". Web's existing tests that read a row up to its status, or list its buttons, now
end with "Ändra". The glossary has a row for `Change`.

`TESTING.md` gained a rule during the build: leave to generation what a claim does not rest on.
The three `WhenChangeTo…` refusals follow it, and were seen failing when their check was broken;
`WhenChange` and `WhenChangeBooking` keep `new BookingRequest(…)`, as their claims rest on every
field. That rule and `WhenBookDepartureBeforeArrival.cs` were changed outside this build and are
left as found.

Deviations from the design:

- **The form and the row share more than the room order and `fieldOf`.** They also share what is
  entered and how it becomes a `BookingRequest`, in `bookingEntry.ts`, which the row would
  otherwise copy. `useEntered`'s parameter is now `initial`, since the row starts from the
  booking's values.
- **`styles.css` narrows the guest field of a row being changed.** Tried in the browser, the row
  pushed "Spara" and "Avbryt" onto two lines and made every other row's status and "Ändra" wrap.
