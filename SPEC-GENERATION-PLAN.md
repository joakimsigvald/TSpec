# SPECIFICATION.md generation

Working notes for the `specification-generator` branch, shipping in **2.0.0**. Dies at merge, once
§6 is turned into release notes.

**State:** feature complete, dogfooded on `MyHotel.Spec` (52 requirements, black-box HTTP) and
`Core.Spec` (32, over `RoomService`, `BookingService`, `BookingNumberGenerator`, their stores
mocked). Remaining: §7 (release checklist) and §8 (MyHotel backlog + the PO's open readings).

**Out of scope:** laws, cross-assembly merging, CLI/MSBuild orchestration, the CI staleness gate.

## 1. How collection works

Built and stable; not self-tested beyond `MyHotel.Spec` passing (§7.2).

- A test's specification is recorded at `Dispose`, keyed on `TestState.Result` — `TestStatus` alone
  can't tell pass from running.
- The assembly fixture disposes last, after every test, so the document sees every outcome.
- Only a `Passed` result is recorded; a skip or a constructor-throw failure is silently excluded.
- The write gate compares "every participating method minus skips" against "what reported in" —
  covers filtered, failed, and thrown runs alike, with no "was anything red" flag needed.

## 2. Decisions a test would not catch

- Document and per-test specification render from one text — no second renderer, no unpinned
  requirement.
- Layout runs last, after every text edit (heading-word stripping, fence indent).
- Wrap width: source 80 / document 90, both with a 10-column tolerance before breaking a line.
  Continuation indent: source 3 steps / document 2, relative to the line it continues.
- Break points are recorded as unprintable markers in the text, stripped before any output
  (including failure messages) — a new sink of described text must strip them too.
- Erasure from a specification is semantic (e.g. `int?` kept, since it differs from `int` in what
  values can occur), never a taste call — see the `specification-erasure-principle` memory.
- Areas come from `Type.Namespace`, not the folder — the run has no file paths.
- Section order is a file format (`ComplexityNumber`, arrangement-based, ties on rendered length
  then name) — changing the tiebreak reflows every `SPECIFICATION.md` at once: major version.
- `Spec<T>`'s one type argument is inferred as subject/return-type/both from the `When` overload
  used. A checkable `Nothing` marker is in `TODO.txt`, and is breaking.

**Staleness is the consumer's gate:** `dotnet test && git diff --exit-code -- "**/SPECIFICATION.md"`,
needing a `.gitattributes` normalising line endings or the build id moves on every checkout.

## 3. Traps

Each cost a session to find; none is guarded by a test.

- The specification freezes at first observation — `Specification.Is(…)` reads and asserts in one
  recordable step, or a test would describe checking its own description.
- Compose phrases after describing, never before — prepending to raw source before parsing feeds
  non-C# to the grammar.
- Specifications contain `<`/`>` and two-space indents; markdown outside a code context eats or
  collapses them.
- Two hierarchy walks run opposite directions — the heading path walks nesting, declared types walk
  inheritance — and using the wrong one fails silently rather than erroring.
- Setups run last-declared-first, so generated numbering reads backwards unless setups are declared
  in reverse.

## 4. Hoisting

What every requirement under a heading states is written once at that heading, whole clauses only,
judged clause-by-clause, rising as often as the least-frequent entry states it. The act, the return
type and a claim about that act never rise above the subject whose heading names the method —
nothing higher says what the claim is a claim about; only arrangement rises further. A lone
requirement's claim stays in its own item: with no sibling saying it, there is nobody above it to
say it for. Levels: document → area → (group, where an area holds more than one namespace below it)
→ subject → branch.

**Deliberately not built:** a family-to-level ceiling for arrangement itself (e.g. `Having` no
higher than its branch) — placement is inferred from sharing today, a proxy, since TSpec does not
record which class declared a clause. **Trigger:** gap §5.3 wants the same input.

## 5. Open gaps

1. Opt-out attributes (`[Specification]`/`[ExcludeFromSpecification]`) — not being built for
   2.0.0; nothing has needed one yet. Both docs' WIP notes need rewording, not the feature (§7.1).
2. A second assertion in one requirement starts an orphaned sentence (no `Then`). Needs phase 2 to
   buffer a whole assertion before it can be told from other text.
3. A subject-wide assertion, declared above its branches, still repeats in every branch block
   instead of hoisting — needs the same "who declared this" input as §4. Untouched by assertions
   now hoisting: that lifts a clause every requirement states, and a `[Fact]` in the base class is a
   requirement of its own in each branch, not a clause its siblings repeat.
4. The binder is silent across a hoist boundary: `Having X` at the heading, `Having Y` in the item,
   nothing relating them in time. Not reachable in MyHotel today.
5. A nullable return type (`Room?`) renders as `Room` — needs `NullabilityInfoContext` over the
   generic argument. Not exhibited yet.
6. One outlier requirement costs its whole sibling group their shared hoisting (exact match, no
   partial credit). Open question: let dissenters restate instead of blocking the hoist? Unfixed;
   not currently exhibited in MyHotel.
7. A block opening with `Given` loses that word under a `Given` heading — the word-drop rule can't
   tell a family keyword from a class-name segment. Not reachable in `MyHotel.Spec` today.
8. Two cosmetic warts, neither blocking: `One(expr)` over an already-articled expression
   double-articles; a `new[] { … }` array argument keeps raw C# syntax.
9. An act that varies (e.g. a valid vs. a zero-night booking) can't become a branch — one `When`
   per class forces sibling sections instead of one act with two branches. Workaround: make the
   varying value a tag. By design, not a defect; every refused-input case will meet it.
10. A wrapped trailing phrase can still start its line with its binder's comma (e.g. `because …`
    after an assertion) — the fix belongs in composition, not layout: the binder needs to travel
    with its text. No longer exhibited for the return type specifically, which now only joins the
    act where it costs no line.

**Shapes not yet exercised**, where the next gaps will come from: a nullable return type (§5.5);
an area with three or more namespace segments below it (grouping merges everything past the first
into one key today, untested past two). Bookings exercised the rest — two subjects per document, a
second store to mock, a `400` refusal, an act that varies (§5.9), a nested branch, and two classes
under test sharing a folder.

## 6. Release notes material

Three lists. A consumer upgrading from 1.5.0 with pinned expectations cares only about the second.

**New in 2.0.0:**

- The generator — opt in with `[assembly: AssemblyFixture(typeof(SpecificationDocument))]`.
- Subject resolved by convention, verified against `deps.json`, failing before the first test.
- Collection restricted to passing tests; written only on a complete, unfiltered run.
- Document layout: title, provenance, area → group → subject → branch headings, requirements as a
  list, shared clauses hoisted, sections ordered simplest first.
- Subject-under-test and return type declared where they hold.
- Bug fix: a trailing comma in an initializer (`{ X = 1, }`) no longer emits a stray `}` into the
  rendered text.

**Changes text 1.5.0 already emits** — pinned expectations will fail on upgrade:

| Change | Pins moved |
|---|---|
| `Having` / `Until` replace `After` / `Before`; setups read `after`, teardowns `before` | 3 |
| Subject parameter elided — `When(_ => _.Api.Get("/x"))` renders `When Api.Get("/x")`; `When`/`Having`/`Until` only, mock setups and assertion predicates keep theirs | 291 |
| Collection mentions pluralize — `Two<Room>()` reads "two Rooms"; a plural drilldown takes the bare apostrophe | 57 |
| Wrapping keeps expressions whole — an over-long phrase moves to the next line entire | 19 |
| Wrapping breaks at structure — after the last argument comma that fits (the paren when none does), before a brace block that fits a continuation line, at call-chain joints; whitespace outranks punctuation | 3 |
| Arguments of a chained call are described — `obj.Foo(An<int>()).Bar()` reads `obj.Foo(an int).Bar()`, no longer quoting the source | — |
| Noise erased — `await`, `async`, `!`, `?.` no longer appear | — |
| Interpolation holes described as prose rather than source | — |
| `with` expressions name their target instead of rendering as their members alone | — |
| Tags name themselves via `[CallerMemberName]`, render normalized (`_roomNumber` → `RoomNumber`), drill down possessively, and must be unique within a test | — |

A dash means the count was never recorded, not that nothing moved. Re-count from the diff before
the notes quote a number.

**Document-only refinements:**

- The return type is said on the act where it costs no line; a long act keeps it as a `Return
  type:` label instead.
- Declared labels (subject, return type) hoist independently, no higher than the subject whose
  heading names the method.
- Hoisting is decoupled from position.
- A dotted subject name titles as one name; a dotted branch path reads as one comma-joined
  sentence — matching how an underscore already read within one name.
- Layout is applied last; an item breaks where it no longer fits.
- The document is 90 columns wide, tolerating 100 rather than break a statement.
- An area holding more than one namespace below it groups a second time, so a folder gaining a
  second class under test doesn't lose its subject's hoist.

## 7. Before release

1. **Finish the docs.** README §6.2 and `TSpec-agent-reference.md` carry a work-in-progress note
   about opt-out attributes (§5.1) that wants rewording, not removal — the feature just isn't
   landing in 2.0.0. The agent reference also still says "covers TSpec 1.5".
2. **Scratch project for the §1 xunit facts.** Fixture wiring, `TestState` at `Dispose`, and
   end-of-assembly ordering can't be self-tested from inside the same assembly; today only
   `MyHotel.Spec` passing verifies them, which won't catch a regression precisely.
3. **Set `PackageVersion`/`PackageReleaseNotes`** in `Core/Core.csproj`, still at 1.5.0.

**Everything ships as 2.0.0** — the generator is a breaking release in its own right (§6, second
list), so there is nothing to stage separately. Keep the rendering changes and the removals below
under their own headings: they break different things, and a reader hitting re-pinned expectations
won't look under a heading about deleted types.

2.0.0 also drops the deprecated surface accumulated in 1.x:

| Removed | Replacement |
|---|---|
| `Then<TService>()` — parameterless, on `Spec_Then`, `ITestPipeline` and `TestPipeline` | `Then<TService>(wasInvoked: Times)` |
| `And<TObject>()` — parameterless, on `IAndVerify` and `AndVerify` | `And<TObject>(wasInvoked: Times)` |
| `HasObject.Type<TObject>()` | `Is().A<T>()` / `Is().An<T>()` |

Removing those lets `IVerifyService`/`VerifyService` go — nothing else produces them. Also
`TODO.txt` line 1, failing a test whose pipeline never ran.

## 8. MyHotel bookings

The feature the documents are grown against. Here because the next session continues it; the one
part of this file that outlives the merge, moving to MyHotel when it does.

**Built: the bookings resource.**

- `POST /bookings` (`201` + `Location`), `GET /bookings`, `GET /bookings/{bookingNumber}`,
  `DELETE /bookings/{bookingNumber}`.
- Refusals: `400` (period under one night), `404` (unknown room or booking), `409` (room already
  taken for those nights).
- Nights are half-open, `[from, to)` — adjacent stays don't collide; the boundary has its own
  branch in both suites.
- `Booking.Id` renamed `BookingNumber` throughout (Contract, Core, Entry, both spec projects).
- Numbers come from `IBookingNumberGenerator`, seeded via `BookingNumbers:Seed` (10000 shipped, 0
  default). `BookingNumberSeed.LastUsed` is the number already counted as used, so `Next()` is
  `(LoadLastUsed() ?? seed.LastUsed) + 1`. Never re-seeds or decrements — a cancelled booking's
  number is not returned, since nothing here reads the bookings at all.
- `BookingStore` persists both bookings and the counter in one JSON file (`{ Bookings,
  LastUsedNumber }`), so numbering survives a restart with the bookings it counts.
- `MyHotel.Spec` asserts against the shipped seed rather than mocking — that suite states the HTTP
  surface, not every wrinkle of numbering. `Core.Spec` covers the generator directly, plus a
  two-call mock sequence proving the second booking gets the second number.
- Cancelling frees its nights, stated under booking rather than cancelling — the act that observes
  it is booking.
- Core went vertical: `Core/Rooms/`, `Core/Bookings/`.

**Next, in the order the PO set:**

1. **`GET /bookings?roomNumber=`** — one room's bookings, as a filter on the list endpoint rather
   than a route of its own.
2. **`PUT /bookings/{bookingNumber}`** — amending a booking, re-stating the refusals booking makes.
   Left out of the resource because cancel-and-rebook covered it; asked for explicitly.
3. **Refuse `DELETE /rooms/{roomNumber}` while the room has bookings** (`409`). The first rule
   crossing subdomains — watch what it does to Core's layering, since rooms must consult bookings.
4. **Out of service.** `PUT /rooms/{n}/out-of-service` sets it, `DELETE` on the same path reverses
   it, and the state shows in `GET`. The term is hotel usage — *out of order* is maintenance, *out
   of service* is temporarily off inventory — and is the PO's to rename once it can be read in the
   document. A room out of service takes no new bookings; what becomes of the bookings it already
   has is **undecided and needs a ruling before this is built**.

**Open questions for the PO:**

- Whether `Core.Spec`'s booking headings (`When book`, `When get`) should match the rooms' fuller
  style (`When add room`).
- Whether `new(2026, 8, 10)` should say `new DateOnly(…)` so a reader can see the type.
- Whether a refused input should stay its own section or become a branch, by making the varying
  value a tag (§5.9).
- Whether `MyHotel.Spec` repeating `, returns HttpResponseMessage` on all ten `When` headings is
  acceptable — correct by the new hoist ceiling (§4), but the least informative case for it, since
  that document has one return type throughout.
