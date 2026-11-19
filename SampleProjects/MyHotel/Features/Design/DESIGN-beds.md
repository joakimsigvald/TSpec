# Beds per room — design

Built in Contract, Core, Entry and Web. The project and subdomain graphs do not change.

**A new refusal, `InvalidBedCount`, `400`**, for fewer than one or more than nine beds, when a room
is added or updated. It is checked first, as a renumbered room is, so a room that is both taken and
has ten beds is refused for its beds.

**The limits 1 and 9 are constants in Contract's `Rooms`**, beside `Room`: Core checks them, and
Entry answers with them.

**A new resource, `GET /rules`**, answers the hotel's rules for Web to show, today only the limits:
`{ minBedCount: 1, maxBedCount: 9 }`. The endpoint returns the constants directly, with no service
behind it, since there is no logic to hold.

**Web gets a `rules` folder** with its API call, and a query for the rules beside those for rooms
and bookings, reloaded by no change since the rules never change while the hotel runs. The dev
server forwards the new route to Host.

**Web's "Sängar" becomes a dropdown from the lowest to the highest number of beds the rules give**,
in the add-room form and on each room's row. In the form it starts at the lowest, 1, and returns to
it after adding. The refusal gets its text in `sv.json`, "Otillåtet antal sängar.", without the
numbers so Web holds them nowhere, though the UI cannot cause it.
