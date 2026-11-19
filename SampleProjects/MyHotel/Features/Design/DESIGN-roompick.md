# Room dropdown when booking — design

Built in Web alone; the backend does not change.

**The page keeps the list of rooms.** Today the Rooms section loads and keeps its own list, which the
booking form cannot see. The list moves up to the page, which hands it to both sections. The
Rooms section still loads, shows, adds, saves and deletes rooms as it does now, and tells the page
what changed; the booking form reads the same list. Because there is one list, a change in the
Rooms section reaches the dropdown at once.

**The booking form's room field becomes a dropdown** made from that list, sorted alphanumerically,
with "Välj rum" as the empty first choice. The sorting is the dropdown's own: the Rooms table keeps
the order the rooms were added.

**"Immediately" covers changes made on this page.** A room added or deleted in another browser shows
after a reload, as in the Rooms table today. Showing it at once would need the backend to push
changes to the page, a new kind of interaction and a feature of its own.
