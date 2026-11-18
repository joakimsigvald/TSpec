# Deleting a room with bookings — design

Built in Core, Contract, Entry and Web, with one more field stored by Infra and one new package in
Web.

**A booking gets a status: open, past or cancelled.** Only "cancelled" is stored. Past and open are
worked out from the booking's dates and today's date whenever a booking is read, so a booking turns
past by itself. What is stored and what is sent part ways: the store keeps whether a booking is
cancelled, the API sends its status. Bookings already stored count as not cancelled.

**The hotel gets a clock.** Today is the server's local date, read from a clock Core is handed
(8e). The specs set it, so a booking's dates mean the same on every run.

**Cancelling marks the booking instead of removing it.** `DELETE /bookings/{bookingNumber}` keeps
its route and its answer. A past or cancelled booking is refused with `409`. Booking ignores
cancelled bookings when it looks for overlaps.

**Cancelling a room's open bookings is a new endpoint, `DELETE /rooms/{roomNumber}/bookings`**,
served by Bookings.

**A room states whether it has open bookings.** Every room the API returns says so; what a client
sends for a room stays as it is. Deleting a room that has open bookings is refused with `409`. Rooms
asks Bookings without depending on it: Rooms declares the question, and Bookings answers it. The
dependency still runs from Bookings to Rooms only, so the subdomain graph doesn't change.

**Web reads the API's lists through TanStack Query**, a new package, in place of the lists the page
and its sections keep by hand. Each change, once it succeeds, reloads the lists it affects: a room
change reloads the rooms, a booking change the bookings and the rooms. A list also reloads when the
window regains focus, so changes made in another browser show then, or after the next change. The
backend pushes nothing.

**Each room's buttons follow what the room states:** "Ta bort" is disabled while it has open
bookings, "Avboka alla" while it has none. The bookings list shows "Avboka", "Avbokad" or nothing,
from each booking's status.

**Both confirmations are a small dialog of Web's own**, with a caption, the question and "OK" and
"Avbryt", all from `sv.json`. It stands centred, just below the row it asks about, or above it when
the window ends first. The browser's own dialog was tried first, but it heads every question with
the site's address, and a page cannot change that.

**One new text:** a cancel refused because the booking is past or cancelled shows "Bokningen är
redan avbokad eller passerad." above the bookings table.

**Backlog item 3 is replaced by this feature** and leaves `BACKLOG.md`. In `GLOSSARY.md`, Cancel
changes meaning, and Past, Current, Future and Open become terms.
