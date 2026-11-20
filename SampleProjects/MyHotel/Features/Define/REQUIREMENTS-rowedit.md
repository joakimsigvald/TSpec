# Changing rooms and categories on their rows

A room's and a category's row is changed the way a booking's is: its values show as text until
"Ändra" is pressed.

In the UI:

- A room's row shows its Kategori and Sängar as text, a category's row its Beskrivning. "Spara"
  leaves the row, and "Ändra" comes last in its last column.
- "Ändra" turns those values into fields holding them: Kategori and Sängar dropdowns, Beskrivning a
  text field. The last column then holds only "Spara" and "Avbryt".
- "Spara" is dimmed until something is changed, and while Beskrivning is empty. A saved change
  shows on the row immediately, and so does its effect on the categories' "Ta bort". A refusal
  shows under its field, and the row stays open for editing.
- "Avbryt" shows the row as it was.
