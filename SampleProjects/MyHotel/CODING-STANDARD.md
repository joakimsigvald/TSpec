# MyHotel - Coding standard

How MyHotel's backend code is written, for developers and AI agents alike. Read it before editing code.
The frontend (`Web/`) is not covered.

## About this standard

- Rules are short and to the point.
- A rule keeps its label (`6b`), and a removed rule's label is never reused. A new rule takes the next letter
  of its section.

## Terms

- *Simple statement*: an expression statement (a call, an assignment, an `await`), `return`, `throw`,
  `break`, `continue` or `yield`.
- *Structured statement*: one that holds statements of its own - a selection (`if`, `switch`), an iteration
  (`for`, `foreach`, `while`, `do`), a `using`, a `lock` or a `try`.
- *Clause*: the statement an `if`, an `else`, a loop, a `using` or a `lock` holds.

## Philosophy

Every rule below serves one of these six principles, and carries its numeral. A rule is a worked-out
consequence, not an end in itself: where following a rule to the letter defeats the principle behind it, the
principle wins and the rule is fixed in the same PR.

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

## 0. General guidelines

a. Use the newest C# the target framework offers (.NET 10 today): when a newer feature does the job better
   than an older one, use it. (III)
b. Prefer high-level constructs (LINQ, switch expressions, pattern matching) over low-level ones (foreach,
   if-else). (III)
c. If you encounter code that does not live up to the standard, rewrite it if it is directly related to
   yours, otherwise leave it alone. (IV)
d. The standard is enforced in review. A rule a tool can check mechanically gets the tool when one
   exists. (II)
e. Code that mirrors a schema defined elsewhere - a database table, an XSD, an embedded query - keeps the
   original's shape. A mirrored type lives in a `Mirrors` folder (10a), with nothing of our own beside it,
   and follows the standard in everything its original does not dictate. (I)
f. Foreign code - anything from outside MyHotel, such as a package or TSpec itself - is not governed by
   this standard. (I)

## 1. Language and naming

a. Identifiers and comments are English and pure ASCII. (I)
b. A domain concept has one term in code, the one `GLOSSARY.md` lists. The PR that introduces, renames or
   retires a concept updates its row. (I)
c. A name says what the thing *is*: a method by what it returns or does (`BookingsFor`, `Book`), a
   variable by its role (`remaining`, `offered`), never by its type or history (`list2`, `newBooking`,
   `tempResult`). (III)
d. No magic constants. A literal that carries meaning - a configuration key such as `RoomStore:Path`, a
   route, a limit - is a named constant with one home, referenced from everywhere it is used. A spec's
   example is not one: what a fact is given and what it expects stand as literals, since they are the
   claim. (I)
e. Casing is .NET's: PascalCase for types, members, constants and static fields; camelCase for parameters
   and locals; `_camelCase` for private instance fields. In a test project, an underscore may separate the
   segments of a long method name: a test often has more to say than a production method. (VI)
f. The shorter a name's scope, the shorter the name (`ch` instead of `character` in a one-line lambda). (VI)
g. A message for the operator or the user is English. (V)
h. A using alias is a word that says what it stands for (`Wire`), never a single letter. (VI)

## 2. Types

a. A type whose main purpose is to carry data is a record, not a class, changed with `with` (a new copy); a
   small one compared by value (a key, a coordinate, a pair of bounds) is a record struct. A record holding
   a collection must implement its own equality if it is used as a key or compared, since its default
   equality compares the collection by reference. (II)
b. Avoid null. Where null is a legitimate value the type says so (`T?`; `Find` returns `T?` because nothing
   found is a normal outcome); a non-nullable type never holds null, and neither `!` nor `#nullable disable`
   silences a warning the shape should have removed. A check that a value is there hands it on (`is { } x`,
   `TryX(out x)`, a narrowed record); nothing after it needs `!`. (II)
c. A tuple is private to its class: a public or internal member returns a named type. (III)
d. Never pass or return a mutable collection (`List<T>`, `HashSet<T>`, `Dictionary<K,V>`, `T[]`); use the
   read-only equivalent. Nothing passed to another context is mutated, by the sender or the receiver -
   except an object made to be written to (a writer, a builder, the framework's `HttpContext`). (II)
e. `IEnumerable<T>` is a sequence read once. A method may take one as a parameter and references that
   parameter exactly once. A member another class calls never returns one: it returns a materialised
   read-only collection (`IReadOnlyList<T>`, `IReadOnlySet<T>`, `IReadOnlyDictionary<K,V>`). (II)
f. A collection is a collection expression: a literal `[a, b]`, a materialised sequence `[.. xs]`; never
   `ToArray()`, `ToList()` or `ToHashSet()` - the type goes on the left when nothing else carries it (6e).
   `ToHashSet` stays only for a comparer a collection expression cannot take, never the default one
   (`StringComparer.Ordinal` for a string). (III)

## 3. Classes

a. Classes that are not intended to be overridden are sealed. (II)
b. Anything that can be internal is internal. A production project opens its internals to its own spec
   project (`InternalsVisibleTo`, `Core` to `Core.Spec`). A type is public only when another production
   project uses it, or when a public spec class names it in its signature. (II)
c. A class with dependencies takes them through a primary constructor and uses the parameters directly,
   without copying them into private fields. (I)
d. A constructor - including a record's positional parameters - takes at most 6 arguments, preferably 4 or
   fewer. (IV)
e. A singleton (a service, a store, a cache) serves every request at once: no field holds request data, and
   state shared across requests is guarded for concurrent access. What knows the current user is
   scoped. (II)
f. Code with dependencies is an instance that takes them through its constructor (3c), never a static class
   that takes them as arguments. Static is for a pure computation or transformation, a factory run at startup,
   and a handler the framework calls with its dependencies (a minimal-API endpoint). (II)
g. A computation whose steps share state - an input they read, or a value they build or update - is an
   instance holding it, even when it is pure. The instance serves one computation and is never shared. (IV)
h. A type and a member name their accessibility, `internal` and `private` too; an interface's members do
   not. (VI)

## 4. Methods

a. Methods are short, with few indentation levels. A local function or an embedded text is part of its
   method, and included in the method's size. A method that outgrows the guard's ceiling is split into
   methods each under half of it. (IV)
b. A method takes at most 4 arguments: 0 to 2 is the aim, 3 is ok, 4 a special case. (IV)
c. A method does one thing, and its name says what it does. A query is named by the noun it returns
   (`Bookings()`, `Version(assembly)`), a command by a verb (`Book`, `TryRemove`); no `Get` prefix.
   The noun takes a preposition only where the bare noun would misread: when the argument is a key to the
   thing rather than the thing itself (`BookingsFor(roomNumber)`, `NightsOf(booking)` - `Of` a part of
   the argument, `For` a lookup keyed by it), or when the bare noun is already taken in scope. (III)
d. A method works on one level of abstraction and does not mix levels: its body reads as steps, and the
   detail of any step is a method of its own. A name that needs an "and" is banned: name the method after
   what the parts together accomplish and let its body call them - `void C() { A(); B(); }`, never a
   `void AandB()` holding both sets of details. A method exists because its name says more than its body:
   a step used once whose name only restates it stays inline. (IV)
e. A method is a query or a command, never both. A query returns a value and changes nothing; a command
   changes state and may return the result of the change it made (`bool TryRemove(...)`,
   `Task<Booking> Book(...)`). A query with a side effect is the mistake this rule exists to prevent.
   Logging is neither: the orchestrator logs what it ran, the computation logs nothing. (III)
f. Prefer arrow syntax (`=>`) over a body: an expression states a result, a body states steps, and a member
   that needs steps is usually several members. (III)
g. A clause takes no braces when it is a simple statement, and takes them when it is a structured statement
   or a sequence of statements. Two take none whatever they hold: the `if` of an `else if`, which continues
   one selection, and a `using` that runs to the end of its block, which is a declaration
   (`using var reader = ...;`). (VI)
h. A named step inside a method is a local function, never a lambda held in a variable. (IV)

## 5. Async

a. Async methods have no `Async` suffix (an override keeps the name the framework gave it). (II)
b. An async method whose work can be cancelled takes a `CancellationToken` as its last parameter and passes
   it to every call that takes one. A method the framework calls takes it from the context it is handed. (II)
c. Never block on a task (`.Result`, `.Wait()`, `.GetAwaiter().GetResult()`): async all the way up. (II)

## 6. Expressions and control flow

a. Prefer LINQ over loops; never query syntax (`from x in xs select ...`). (III)
b. A LINQ chain reads as a sentence. Avoid complex lambdas. Avoid excessive chaining - as a rule of thumb, a
   LINQ expression has at most 7+-2 parts (anything that carries a meaning on its own). (III)
c. A lambda's parameter is a word of 2 to 12 letters, camelCase. Name shadowing is allowed, unless a unique
   name better fits its role (`cand` in a filter). `_` is an unused discard; only in a spec may a body start
   with it and use it nowhere else (`_ => _.Load(..)`). (III)
d. A null-forgiving `!` marks a shape that has not been fixed yet (2b); until it is, never more than one in
   one expression (not `request!.Room!.RoomNumber`). (II)
e. The type is written once. `var` wherever the right-hand side names it or carries it - `new Booking(..)`,
   `JsonSerializer.Deserialize<Book>(..)`, a cast, any call; the type on the left only where the right-hand
   side cannot hold one, a collection expression (`string[] xs = [..]`, `List<string> lines = [.. parts]`).
   Never on both sides. (I)
f. Prefer an expression over an `if` statement: a boolean expression (`return !a && b;`, not
   `if (a) return false; return b;`), a conditional (`a ? b : c`) or a switch expression. (III)
g. One expression per line: a clause on its own line below its head; a switch expression one arm per line;
   wherever code branches, the condition and each branch on a line of their own (a conditional whole on one
   line when it fits). (IV)
h. One line per expression, or a method of its own: a statement's body, a switch arm, a condition or a
   branch that needs more than one line becomes a method, called from where it stood. (IV)
i. Fail early: a guard - a refusal, a throw, a return of nothing - comes before the work it protects, as the
   first statement of a method and the first branch of a conditional. (III)
j. A text over several lines is one raw string literal, its values in `{}`, unless it must not hold a line
   break. (III)
k. A type argument the compiler infers is never written: `AddSingleton(seed)`, not
   `AddSingleton<BookingNumberSeed>(seed)`. (I)

## 7. Boundaries

A boundary is anything published, mirrored or persisted whose far side changes on its own schedule: a wire
format, a stored document, a linked library's contract. Each has one owner of its schema, one place where
conversion and validation happen, and a version pinned where the other side can see it.

a. What we send across a boundary is a value, never an exception. Inside the backend an exception may carry
   a failure up to the boundary, which turns it into a value - a status and a body where it is a REST
   response. (V)
b. Nothing crosses a boundary as an enum, a custom-defined value type, a time type other than `DateOnly`,
   `object` or `dynamic`: strings and primitives, converted at the crossing; a payload is a named record. A
   key is stable (a room number, a status code) and its display name is the far side's. A time with an
   offset carries too many quirks of format, offset and culture to be safe on the wire, where a `DateOnly`
   has one ISO form; how a value is represented is the developer's call, and the boundary refuses what does
   not parse. (V)
c. A wire type mirrored by hand on the far side (Web's `*Api.ts` files) changes with its mirror in the
   same PR. The wire shape is the JSON default: camelCase, polymorphism through `JsonDerivedType`, nulls
   omitted where marked. (V)
d. SQL lives in the project that owns the connection, as a raw string literal beside the method that runs
   it. Every value is a `@parameter`; nothing from a request or a user is ever concatenated into SQL. (II)
e. Parsing and formatting at a boundary name the culture: `CultureInfo.InvariantCulture` for anything code
   reads back - XML, JSON, a log, a key - and `StringComparison.Ordinal` for a comparison. (V)
f. An exception we expect from foreign code (0f) or over a boundary is caught where it enters, and handled
   or wrapped in one of our own. An unexpected one is a bug, and is left for the outer boundary (8b). (V)

## 8. Cross-cutting concerns

a. A catch names the types it handles. A catch of `Exception` only rethrows, `{ throw; }`, with any side
   effect in its `when`, unless it is in a construct that swallows all errors by design: a boundary (7), a
   dispatcher (8f) or a logger (8d). A catch that throws a different exception passes the original as
   `innerException`, unless the original holds a service's text (8d): then only its identifiers pass. (II)
b. An exception that reaches a boundary unhandled is a bug, not an outcome. The boundary logs it and answers
   with a generic message - a 500 where it is a REST response. (V)
c. `OperationCanceledException` is not an error: let it pass, or catch it only to stop quietly where nobody
   is waiting for an answer. (II)
d. `ILogger` is for the operator: what the process did and where it failed, as structured message templates,
   never interpolated strings. It holds only text we control, the messages of the exceptions it logs
   included: a service's answer is never quoted, only its identifiers, such as a status or an error code.
   It carries no personal data - a guest's name - and never fails the work it logs. (I)
e. The current time is read, and a wait is timed, only through an injected `TimeProvider`, so a spec can fake
   it; `DateTime.Now`, `DateTime.Today` and `DateTime.UtcNow` appear only where the host composes it. (II)
f. When a sender should not know its receivers, use publish-subscribe: the publisher publishes a message,
   and every subscriber to its type receives it. A subscriber's failure never reaches the publisher. (IV)
g. Our own exception types live in an `Exceptions` folder of their sub-domain and are named for what
   happened, without the `Exception` suffix: `RoomAlreadyBooked`. (VI)
h. A service we call that fails on its own side, a timeout included, becomes one of our own exceptions
   saying it is unavailable (7f) and is logged as a warning. If the service is vital, the failure reaches
   the user with status 503; otherwise it is handled. (V)

## 9. Comments

a. A comment says *why*, never what. If the name and the signature say it, there is no comment. (III)
b. Instead of repeating the reasoning behind a decision, a comment points to the document that records
   it. (I)
c. A `///` summary describes a type or a member - at most ten lines. (VI)
d. A type's summary states what the component is for and the one decision a reader would otherwise
   reverse. (IV)
e. A member's summary says what its name and signature leave out. (IV)
f. A `//` comment explains the code itself - at most three lines. There are no `/* */` comments. (VI)

## 10. Files

a. Folder = namespace = sub-domain. A family of independent types that belong together is a subfolder with a
   file per type. Within a sub-domain, a folder may instead group its types by kind, named for it:
   `Exceptions`, `Ports`, `Mirrors`. A kind folder never sits directly under a project, and it splits its
   own content however reads best. In a test project (`*.Spec`), a folder is a subject under test, not a
   sub-domain - the part of the tested project it mirrors, down to a single class. (I)
b. One public type per file, named after it. A nested record stays with its owner (discriminated union), and
   types that differ only in their type parameters share a file. (I)
c. In a test project, a file may hold several public classes, as long as they are nested in one top-level
   class. (I)
d. A file is ideally under 100 lines and never over 250; one that grows past 250 is split. (IV)
e. Member order: fields and constants first, then constructors, then by accessibility - public, protected,
   internal, private - and within one accessibility a caller above what it calls. Among public members:
   static before instance. (III)
f. A type is `partial` only when its other part is not ours to write - a generator's or a framework's; a
   partial type is never a split. (I)
g. A port - an interface another module implements - is declared together with the types it takes and
   returns, either in the module that uses it or in an abstract layer that holds only ports and their models
   (`Contract`). The implementer depends on the port, never the reverse. (I)

## 11. Layout

a. Lines are at most 110 characters, preferably under 90 - in code and in these documents; a table row and
   the lines of an embedded document (0e) are exempt. (IV)
b. A list of multiple arguments, parameters or initializer members is on one line, at or below the line that
   opens it, or each item starts on a line of its own. A tuple value opens with its first item and closes with
   its last. (IV)
c. Indentation is four spaces per level, never a tab, and no line is indented past the fifth level; the lines
   of an embedded document (0e) are exempt. (IV)
d. An expression-bodied member - method, constructor, property, accessor, local function or operator - and a
   switch arm are either whole on one line, or end the line with `=>` and start their body on the next, one
   level in. (VI)
e. A lambda keeps its arrow on the line of its parameters. An expression body starts on the arrow's line and,
   when it does not fit there, wraps inside itself like any other expression; a block body opens on the line
   after the arrow. When the parameters and the start of the body do not fit on one line, the parameters wrap
   (11b) or the lambda becomes a method. (VI)
f. An attribute shares its member's line only when the whole member fits on that line; otherwise it stands on
   the line above. (VI)
g. Exactly one empty line stands between two members, except between two single-line members of one kind.
   Comments and attributes are part of the member. (VI)
