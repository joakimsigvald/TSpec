# Disabled add and save buttons — design

Built in Web alone.

**A row's "Spara" compares the row with what is saved.** Each room's and category's row already
keeps the values being edited; "Spara" is disabled while they equal the room or category as the list
last loaded it. A successful save reloads the list, so the two agree again and "Spara" is disabled by
itself.

**The add forms keep what is being entered.** Today a form reads its fields only when its button is
pressed. Instead, each of the three forms keeps its fields' values as they change, as the rows
already do, and its button is disabled while any of them is empty. Every field in the three forms is
required, so no list of required fields is needed. Emptying the form after adding or booking
disables the button again.

**The booking form takes the dates over from the booking period.** The booking period keeps Från and
Till to itself today, where Boka cannot see them. The form keeps them instead, and the booking period
shows them with their buttons as before.

No new texts.
