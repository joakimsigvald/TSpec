# Disabled add and save buttons

A button that adds or saves is disabled while pressing it would add nothing, or save nothing new.

- "Spara" on a room's or a category's row is disabled until a value on the row differs from the
  saved one. Changing the value back disables it again, and so does saving.
- An emptied category description counts as a change: "Spara" is enabled, and pressing it asks for
  a description instead of saving.
- "Lägg till rum", "Lägg till kategori" and "Boka" are disabled until every required field has a
  value, and again once the form is cleared after adding or booking. The required fields are:
  - "Lägg till rum": Rumsnummer and Kategori.
  - "Lägg till kategori": Namn and Beskrivning.
  - "Boka": Från, Till, Rumsnummer, Antal gäster and Gäst.

Not in this feature: checking a value against the hotel's rules before sending, such as at least
one guest or Till after Från; a tooltip saying why a button is disabled.
