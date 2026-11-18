# MyHotel — backlog

**Purpose:** the features planned for MyHotel, in the order the product owner set, and the open
questions they raise.

## Next, in the order the PO set

1. **`GET /bookings?roomNumber=`** — one room's bookings, as a filter on the list endpoint rather
   than a route of its own.
2. **`PUT /bookings/{bookingNumber}`** — amending a booking, re-stating the refusals booking makes.
   Left out of the resource because cancel-and-rebook covered it; asked for explicitly.
3. **Out of service** (`GLOSSARY.md`). `PUT /rooms/{n}/out-of-service` sets it, `DELETE` on the
   same path reverses it, and the state shows in `GET`. What becomes of the bookings the room
   already has is **undecided and needs a ruling before this is built**.

## Open question for the PO

**Should a refused input stay its own section, or become a branch?** A class holds one `When`, so an
act that varies has to be its own class — which is why `Core.Spec` states booking as three sibling
sections that differ only in the dates:

```
### When book
Book(new BookingRequest(a Room's RoomNumber, a string, new(2026, 8, 10), new(2026, 8, 12)))
### When book zero nights
Book(new BookingRequest(a string, a second string, a DateOnly, the DateOnly))
### When book departure before arrival
Book(new BookingRequest(a string, a second string, new(2026, 8, 12), new(2026, 8, 10)))
```

Making the dates tags would give one `### When book` with `#### Given zero nights` and `#### Given
departure before arrival` under it, so the refusals read as conditions on booking rather than as
three acts. Nothing in TSpec changes either way — it is how the specs are written. Every
refused-input case meets this, so the answer sets a pattern for the suite.
