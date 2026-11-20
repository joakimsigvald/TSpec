# Changing rooms and categories on their rows — design

Built in Web only: the API already updates rooms and categories, so it, Core and the subdomain
graph do not change.

**The room row and the category row get an editing state, as the booking row has.** Each splits in
two, the row that shows the values and the row that changes them, as `BookingRow` and
`ChangeBookingRow` do. "Ändra" opens the changing row, "Avbryt" and a save close it. The fields keep
the accessible names they have today, such as "Sängar i rum 101" and "Beskrivning av Standard".

**"Spara" is dimmed until something is changed, and while a field is empty, as on a booking's
row.** The category row's form, which today stops an emptied description, goes away with it.

**A save is attempted on the row, not the section,** so a refusal shows under its field and the
row stays open. A room's row shows a missing category under Kategori and a disallowed number of
beds under Sängar, as the add-room form does, and every other refusal under "Spara"; a category's
row shows every refusal under "Spara". The section's message above the table is then left to
deleting and cancelling bookings. A save reloads the same lists as today.

**Texts in `sv.json`:** "Ändra" and "Avbryt" for rooms and for categories. The other texts stay.
