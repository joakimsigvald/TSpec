# Changing rooms and categories on their rows — build notes

Built. Web's tests and lint, `Core.Spec`, `Host.Spec` and `Architecture.Spec` are green, and the
25 new or changed Web tests were seen failing first, on the missing "Ändra" or on values shown as
fields. Web's existing tests that changed a room or a category on its row now press "Ändra" first.
The ones that saved twice, or clicked "Spara" with an empty description, are now a saved change
shown on the row and a "Spara" dimmed while Beskrivning is empty. `App.test.tsx` gained a test for
a saved change's effect on the categories' "Ta bort". The rows were seen in the browser: the
changing rows keep the table's layout.

Deviations from the design:

- **The add-room form and the room's row share which field a refusal shows under**, in
  `rooms/roomFieldOf.ts`. The form adds its room-number case to it, so the two cannot drift apart.
