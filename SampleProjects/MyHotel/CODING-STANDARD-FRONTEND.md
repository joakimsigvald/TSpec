# MyHotel - Coding standard, frontend

**Purpose:** the rules for Web, MyHotel's React frontend. Each rule carries the numeral of the
principle it serves; the principles, and how the rules are kept, are in
[CODING-STANDARD.md](CODING-STANDARD.md). Web is a client of the HTTP contract and repeats none of its
rules: it shows what the API answers.

## 1. Language and naming

a. Identifiers and comments are English and pure ASCII. Every text the user reads - a heading, a label, a
   button, a refusal (6a) - is Swedish, from `src/locales/sv.json` through `t`; a component writes no text
   of its own. Its key is English, `area.name`: the resource folder (`rooms.add`), `app` for the page
   itself, `general` for a word every resource uses (`general.save`), and `errors` with the refusal's
   code (`errors.RoomNotFound`). (I)
b. A domain concept has one term in code, the one `GLOSSARY.md` lists. (I)
c. A component and its file are PascalCase (`Rooms.tsx`); every other file is camelCase after its main
   export (`useAttempt.ts`, `roomsApi.ts`). (VI)
d. A hook is `useX`. A callback prop is `onX` (`onAdd`); its handler in the parent is named by what it
   does (`add`, `save`, `remove`). (III)

## 2. TypeScript

a. `strict` stays on. No `any`, no non-null `!`, no type assertion (`as T`; `as const` is not one): what
   comes from outside (a response body, `FormData`) is read as `unknown` or a `Partial`, and narrowed or
   converted where it enters. (II)
b. A data shape is a `type` alias, not an interface or a class. A component's props type is named
   `Props` and stays in its file. (VI)
c. A component or a hook is a `function` declaration. A helper whose body is one expression is a
   `const` arrow (CODING-STANDARD-BACKEND.md 4f); one that needs steps is a `function`. (VI)

## 3. Components and files

a. One folder per resource (`rooms/`, `bookings/`), holding its views, forms and its `*Api.ts`; what
   every resource uses (`http.ts`, `ErrorMessage`, the hooks) stays at the `src/` root. (I)
b. A file exports one component, named after it; the small helpers only it uses stand below it in the
   same file. (I)
c. Function components only. A component holds the state it owns, passes data down as props and changes
   up as callbacks; state lives in the lowest component that needs all of it. (IV)
d. An effect only synchronises with an outside the queries (4a) do not cover; work an event causes goes
   in that event's handler, never in an effect watching state. (III)

## 4. State

a. What the API holds is read through a query (TanStack Query), one per list, and never copied into
   component state or patched from a reply. A change, once it succeeds, reloads the queries it can
   make stale and no others: its own list, and any list that states something about it, as a room
   states whether a booking holds it. `queries.ts` lists them. A query also reloads when the
   window regains focus; the backend pushes nothing. (V)
b. An update that reads the current state uses the updater form
   (`setCount(current => current + 1)`), never the closed-over value. (II)
c. No state that can be computed from props or other state during render. (I)

## 5. The API

a. All HTTP goes through `http.ts`. A resource's calls live in its `*Api.ts`, one function per endpoint,
   returning the wire type; a component never calls `fetch`. (I)
b. The wire types mirror Contract by hand and change with it in the same PR
   (CODING-STANDARD-BACKEND.md 7c). (V)
c. A value put into a path is encoded (`encodeURIComponent`). (II)
d. A failed call throws `ApiError` carrying the body's error code, `'Unknown'` where there is none;
   nothing else crosses `http.ts`. A component never reads a status code. (V)

## 6. Errors shown to the user

a. Every refusal reaches the user as the Swedish text `sv.json` gives its code, or the `Unknown` text
   for a code it does not know. A new backend code gets its row in the same PR. (I)
b. A form shows a failure under the field its code concerns - the `fieldOf` map a form hands
   `useFormAttempt` - otherwise under its button, linked to it by `aria-describedby`. (V)
c. An error message renders as `role="alert"`, through `ErrorMessage`, nowhere else. (I)
d. Web repeats no backend rule: no validation that restates what the API refuses. An input states only
   its own shape (`required`, `type="number"`); whether a room may be added is the API's answer. (I)

## 7. Markup and styling

a. Semantic HTML first: a label wraps its input, an action is a `button`, tabular data is a `table`. An
   ARIA role or attribute is added only where no element carries it. (III)
b. Everything the user can see or do is reachable by role and accessible name - that is what the tests
   query, so an element they cannot find that way is a defect, not a test problem. (II)
c. Styles live in `styles.css`, on elements and small reusable classes (`field`, `actions`); no inline
   styles, no styling in components. (I)
