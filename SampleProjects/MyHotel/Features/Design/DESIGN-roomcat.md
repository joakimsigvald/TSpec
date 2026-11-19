# Room categories — design

Built in every layer, Web included. Categories belong to the Rooms subdomain, since rooms and
categories each check the other, so the subdomain graph does not change.

**Categories are a new resource, `/room-categories`**, kept in a JSON file of their own beside
`rooms.json`, as rooms and bookings are:

- `GET /room-categories` lists them, in the order they were added.
- `POST /room-categories` adds one, refused with `409` when the name is taken.
- `PUT /room-categories/{name}` changes the description, refused with `400` when the body names
  another category, as a room cannot be renumbered.
- `DELETE /room-categories/{name}` deletes one, refused with `409` while a room has it, or when it
  is the last.

A category the hotel does not have is answered with `404`. There is no endpoint for one category
alone: nothing needs it, so the reply to an add points nowhere.

**A category states whether any room has it**, worked out when it is read, as a room states
whether it has open bookings. Web dims "Ta bort" from it.

**A room gets its category's name.** Adding or updating a room with a category the hotel does not
have is refused with `404`, as booking a room the hotel does not have is.

**A new hotel starts with "Standard", "Standardrum".** Core answers with it while no category is
stored, and stores it with the first change. Since the last category cannot be deleted, nothing
stored only ever means a new hotel. Rooms 1, 2 and 3 in your `rooms.json` get "Standard" once, by
hand: no code handles a room stored without a category.

**Web gets a `roomCategories` folder**, with the "Kategorier" section above "Rum", and a query for
the categories beside those for rooms and bookings. A category change reloads the categories; a
room change reloads the rooms and the categories, since a category states whether a room has it.
Both dropdowns and the list sort by name the Swedish way, Å before Ä before Ö, so the sorting is
Web's: the backend compares names exactly and formats nothing for a person. The dev server forwards
the new route to Host.

**New texts:** one per refusal, and the labels, buttons and confirmation in `sv.json`. A refused
room add shows under the field it concerns: a missing category under "Kategori". Deleting the last
category shows its refusal above the categories table; "Ta bort" is not dimmed for it, since Web
would then repeat the API's rule.

**`GLOSSARY.md` gets `RoomCategory`.**
