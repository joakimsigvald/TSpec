# Changing a booking — design

Built in every layer, Web included. The subdomain graph does not change.

**A new endpoint, `PUT /bookings/{bookingNumber}`**, taking what booking takes: room, guest, guest
count and period. The booking number is only in the path, so a change can never carry another one.
It answers `200` with the changed booking, as updating a room does. A booking the hotel does not
have is `404 BookingNotFound`; every other refusal is booking's, with its status and code.

**Booking and changing check a booking with the same rules, kept in one place.** Changing differs
in two ways: the booking does not overlap itself, and a cancelled booking is not checked for
overlaps, since it holds no nights.

**A changed booking keeps what has happened to it**, checked in, checked out or cancelled, and gets
the new room, guest, guest count and period. Its status and actions are worked out from its new
dates when it is read, as now.

**The booking row gets an editing state.** "Ändra" turns it into fields, "Avbryt" back. The fields
stand on the row itself, since a table row cannot hold a form, so each gets an accessible name of
its own, as a room's "Sängar" does. "Spara" is dimmed until something is changed, and also while a
field is empty, as "Boka" is. A save reloads the bookings and the rooms, as every booking change
already does.

**The form and the row share two things**: the rooms in alphanumeric order for the "Rum" dropdown,
and which field a refusal shows under. A refusal no field owns, such as a booking that no longer
exists, shows under "Spara".

**Texts in `sv.json`:** "Ändra", "Spara", "Avbryt", and the fields' accessible names, such as
"Gäster i bokning 10012". The refusals keep the texts they have.
