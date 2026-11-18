# MyHotel - Coding standard

**Purpose:** how MyHotel's code is written, for developers and AI agents alike: the philosophy every
rule serves, and how the rules are kept. The rules themselves live with the code they govern:

- [CODING-STANDARD-BACKEND.md](CODING-STANDARD-BACKEND.md) - the C# backend. Read it before editing
  backend code.
- [CODING-STANDARD-FRONTEND.md](CODING-STANDARD-FRONTEND.md) - the React frontend (`Web/`). Read it
  before editing frontend code.

## About this standard

- Rules are short and to the point.
- A rule keeps its label (`6b`) within its document, and a removed rule's label is never reused. A new
  rule takes the next letter of its section.
- Each rule carries the numeral of the principle it serves. A rule is a worked-out consequence, not an
  end in itself: where following a rule to the letter defeats the principle behind it, the principle
  wins and the rule is fixed in the same PR.

## Philosophy

### I. One home per thing

A term, a constant, a type, a decision exists in exactly one place and everything else points at it, so
changing it is one edit and a reader who has found it has found all of it.

### II. Make the wrong state unsayable

Prefer a shape that removes a mistake to a convention that avoids it: a record cannot be half-mutated, a
non-nullable type cannot hold null, a `@parameter` cannot be injected. A shape that permits the mistake is
wrong even where today's code does not make it.

### III. Say what, not how

A declarative form states the result; an imperative one states the steps and leaves the reader to infer the
result. State the result wherever there is one, otherwise the single purpose.

### IV. One unit at a time

A change should be decidable from one screen: one method, one file, one level of abstraction. Past that size
a unit is beyond understanding as a whole and is a magnet for more, which is why the size limits are triggers
and not suggestions. A split must leave parts that each have a name and do one thing.

### V. A boundary carries as little as possible

The other principles assume one edit fixes one thing. At a boundary - anything published, mirrored or
persisted, whose far side changes on its own schedule - it takes two, and nothing checks that they still
agree. So what crosses is the dullest payload both sides can accept, converted and validated at the crossing;
richness belongs inside.

### VI. Written for the reader

Code is optimised for reading: always for consistency and readability, for performance only where it matters,
never for debugging. The same thing is always written the same way, so its shape alone tells the reader what
it is.
