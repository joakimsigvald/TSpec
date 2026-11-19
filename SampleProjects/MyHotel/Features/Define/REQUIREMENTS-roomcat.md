# Room categories

The hotel keeps its own list of room categories, and every room has one.

- A category has a name and a description, both required. The name is its identity and never
  changes; the description can be changed.
- Adding a category whose name is taken is refused.
- A category can be deleted only while no room has it, and never the last one; otherwise deleting
  it is refused.
- A new hotel starts with one category, "Standard", described "Standardrum". The rooms that
  already exist get it. It is otherwise an ordinary category.
- A room is added and updated with a category the hotel has; any other is refused. A room's
  category can be changed.
- Categories show in alphabetical order by name.

In the UI:

- A new section, "Kategorier", above "Rum": each category with its description, which can be
  edited and saved on its row, and "Ta bort", confirmed in a dialog as for a room. While a room has
  the category, "Ta bort" is dimmed and says why on hover. Deleting the last category shows why it
  was refused. Under the list, a form adds a category.
- The rooms table gets a "Kategori" column after "Rum", a dropdown of the categories, saved with
  "Spara".
- The add-room form gets a "Kategori" dropdown after "Rumsnummer", showing "Välj kategori" until
  one is chosen, and
  adding is stopped by the form until then. After adding, it shows "Välj kategori" again.
- A category added or deleted appears in or leaves the dropdowns immediately.
