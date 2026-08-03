# SPECIFICATION.md generation

Working notes for the `specification-generator` branch, shipping in **2.0.0**. Temporary: this file
dies at the merge, once §6 has been turned into release notes.

The code and the ~1400 pinned `Specification.Is(…)` expectations already record everything built —
so what belongs here is only what neither can say: facts nothing tests, decisions a change could
reverse silently, and the open work.

**State:** feature complete, dogfooded on `MyHotel.Spec` (50 requirements, black-box HTTP) and
`Core.Spec` (28 over `RoomService` and `BookingService`, their stores mocked). MyHotel grew a
Bookings subdomain on 2026-08-03 — `POST`/`GET`/`GET {id}`/`DELETE` over `/bookings`, with a
half-open `[from, to)` overlap law — which is what more than doubled both documents. Still to
come there, and the readings awaiting the PO, are in
[`SampleProjects/MyHotel/BOOKINGS-PLAN.md`](SampleProjects/MyHotel/BOOKINGS-PLAN.md) — that work
outlives this file.

**Out of scope:** laws, cross-assembly merging, CLI/MSBuild orchestration, the CI staleness gate.

## 1. How collection works

A test's specification is recorded when its class disposes; the document is written when the assembly
fixture disposes. Four xunit facts make that work — probed 2026-07-26 against xunit.v3 3.2.2, still
the pinned version. Nothing self-tests them (§7.2); re-probe if it moves.

- **A test knows its own outcome at `Dispose`**, despite the XML doc saying otherwise — but
  `TestStatus` still reads `Running`, so the verdict must come from `TestState.Result`. This is the
  only moment TSpec holds a finished specification *and* knows it passed.
- **The assembly fixture disposes last**, after every test including failures, so by the time the
  document is written everything that was going to report has.
- **Recording only on `Passed` needs no skip handling.** `[Fact(Skip=…)]` never constructs the class;
  `Assert.Skip` constructs and disposes but reports `Skipped`. Neither can arrive as a pass.
- **A constructor that throws is invisible.** It never reaches `Dispose`, so the runner reports
  `Failed` while collection hears nothing — the run looks exactly as though that test did not exist.

That last one is why a "was anything red?" flag cannot gate the write. The gate instead reflects every
participating test method, subtracts those carrying `Skip`, and writes only when that set matches the
one that reported in — covering all three ways a run can be incomplete: filtered, failed, or thrown.

## 2. Decisions a test would not catch

- **Document and per-test specification render from one text.** The pinned expectations are
  simultaneously the document's regression tests; a second renderer would leave every requirement in
  the repo unpinned. The document may add structure *around* the text, never a second version of it.
- **Layout is last.** `TextBuilder` must never be handed text something else still intends to edit:
  the document strips its heading's word and accounts for the fence indent *first*, then each
  consumer wraps at its own width — source 80, document 90 — and its own continuation indent —
  source three steps, document two, decided 2026-08-03: the page needs a continuation to sit deeper
  than a phrase, not terminal-proof distance. Violating the ordering made a 76-character claim
  measure 81 and take a fence it never needed. The continuation indent is *relative* — the line's
  own step plus the wrap delta, never compounding across a line's several continuations — so the
  step delta is self-describing: one step down is a subordinate clause, more is the same statement
  wrapped, at any depth. Hanging indents were considered and rejected: no crisp lead word on every
  line kind, and per-statement columns leave the grid.
- **Where a line may break is structure, recorded while it still exists.** Describers write
  unprintable markers (`Wrap`) into the text — `Enter`/`Exit` rank by nesting; a `Point` after the
  opening paren and each argument comma, before a brace block, at each call-chain joint — and
  `TextBuilder` breaks at the last point of the shallowest rank that fits, then whitespace, then
  punctuation, then mid-word. Markers never reach output: layout strips them while measuring, and
  the one path splicing described text straight into a failure message
  (`Constraint.GetException`) strips them itself — any new sink of described text must do the
  same, or control characters leak into terminals.
- **Erasure is semantic, never taste** — see the `specification-erasure-principle` memory. The
  load-bearing case is `?` on a type, kept because `int?` and `int` differ in what values can occur.
- **Section order is a file format, not an implementation detail.** Sections sort by
  `ComplexityNumber`, which measures arrangement rather than size — assertions contribute nothing, so
  order moves only when arrangement does. Leaf ties break on rendered length, then alphabetically,
  and that last step is a placeholder for something semantic. Changing it reflows every
  `SPECIFICATION.md` at once: major.
- **Which type argument means what is inferred, not declared.** `Spec<T>`'s single argument may be
  the subject, the return type, or both, so each `When` overload reports from its own signature which
  it uses. A spec meaning "no subject" and an act that merely ignores one are indistinguishable. The
  checkable form, declaring `Nothing`, is in `TODO.txt` and is breaking — non-generic `Spec` would
  become `Spec<Nothing, Nothing>`.

**Staleness is the consumer's gate, not ours**, the document being deterministic:

```bash
dotnet test && git diff --exit-code -- "**/SPECIFICATION.md"
```

It needs a `.gitattributes` normalising line endings (`* text=auto eol=lf`), or a Windows and a Linux
checkout compile different bytes, the build id moves, and the gate fails on every run for a reason
that looks nothing like its cause.

## 3. Traps

Each cost a session to find; none is guarded by a test.

- **The specification freezes at first observation.** `Specification.Is(…)` reads it from inside a
  test and then asserts — itself a recordable step. Without the freeze, a test describes checking its
  own description.
- **Compose phrases after describing, never before.** Prepending a word to *raw* source and parsing
  the splice feeds non-C# to the grammar: `"by (it, i) => it + i"` was read as a lambda return type
  and silently swallowed.
- **A code context is load-bearing.** Specifications contain `<` and `>`, which markdown outside a
  fence or span eats as a tag, and two-space indents that collapse. Also why tag names are emphasised
  by capitalization rather than `**` — the same text is failure output in a terminal.
- **Two hierarchy walks, opposite directions; the wrong one fails silently.** The heading path walks
  *nesting* — walking `BaseType` instead yields `ApiSpec<T>` → `WhenGetVersion` → `GivenNothing`, a
  shared black-box base never meant as a heading. Declared types walk the other way, up *inheritance*
  to the closed `Spec<,>`, recognising non-generic `Spec` first since that is `Spec<object, object>`.
- **Setups run last-declared-first, so generated numbering reads backwards.** A numbered mention
  takes its value when first *requested*. Exploitable — declare setups in reverse and `A<T>` is the
  first-created entity throughout, as `WhenListRooms.GivenTwoRooms` does. The limit: creation order
  and value order always coincide, so no requirement can distinguish "in creation order" from "sorted
  by the generated value".

## 4. Hoisting

What every requirement under a heading opens with is stated once under it: whole clauses only, shared
by every entry, each judged on its own, rising as often as the entry saying it fewest times says it.
Assertions never hoist. Levels are document → `##` subject → `###` branch.

**Deliberately not built.** Placement is inferred from sharing, a proxy — TSpec does not record which
class declared a clause, so a `Having` from a branch constructor and one from a `[Fact]` are
indistinguishable. The successor is a family-to-level ceiling (`When` no higher than its subject,
`Having` no higher than its branch, arrangement anywhere), needing a naming convention enforced on
test classes. **Trigger:** a clause above the heading that names it. Gap §5.4 wants the same input.

## 5. Open gaps

1. **Opt-out attributes.** `[Specification]` / `[ExcludeFromSpecification]`, nearest declaration
   wins, default include. Nothing has needed to opt out, and absence could never certify coverage
   anyway. Both docs carry a work-in-progress note about it (§7.1).
2. **Namespace as a grouping level.** Not collected. **Its trigger has fired** (2026-08-03): Core is
   now `Rooms/` and `Bookings/`, both spec projects mirror that, and each document holds two
   subjects' sections interleaved — `Core.Spec` sorts `When book`, `When cancel` and `When get`
   among the room sections, so the two services read as one list. What a namespace level would fix
   is exactly that; what it costs is a heading level above subjects that most documents do not
   need. Decide before release, since it moves every section: a level, or sorting subjects together.
3. **A second assertion starts an orphaned sentence.** Only the first gets `Then`, so
   `Second is new Room(…)` reads as a claim about nothing. Pinned deliberately in
   `WhenTwoItems.cs:24` and `HavingWhenUntil.cs:67`. Blocked with two neighbours — lowercase
   continuation, breaking after `that` — on one input: assertions carry no `StepFamily`, and phase 2
   streams steps one at a time. **Trigger:** phase 2 buffers a whole assertion.
4. **A subject-wide assertion repeats under every branch.** `ThenRespondOk` on `WhenListRooms` is
   inherited by both and stated twice; an assertion *declared above* branches is a claim about the
   subject, and the document cannot tell that from two branches agreeing. Wants §4's missing input.
5. **The binder is silent across a hoist boundary.** A branch sharing its first `Having` but not its
   second gets `Having X` in the heading and `Having Y` in the item, with nothing relating them in
   time. Not reachable in MyHotel today.
6. **The declared return type drops reference-type nullability.** `Spec<RoomService, Room?>` renders
   `Return type: Room` directly above a requirement reading `Result is null`. `Room?` is `Room` plus
   a `NullableAttribute` in IL, so reading it needs `NullabilityInfoContext` over the generic
   argument; `int?` survives, being a distinct type. Not exhibited — `RoomService.Get` throws instead.
7. **One outlier costs every sibling its hoisting.** Exact match with no partial credit, so a single
   differing section unhoists the whole level — a restart spec with its own subject once pushed four
   shared lines into all six HTTP sections, 105 lines becoming 141. Open question: should "shared by
   every entry" become "shared by all but the few that say otherwise", with the dissenters restating?
   Unfixed; MyHotel no longer exhibits it, `Hotel` now being the subject of every black-box spec.
8. **`Given` loses its word under a `Given` heading.** A block opens `IRoomStore.Load() returns zero
   Room` bare, because a block's opening word is dropped where something above says it — but the
   heading's "Given" is part of a *name* and the clause's is a family keyword. Not reachable in
   `MyHotel.Spec`, where every branch block opens with `Having`.
9. **Two cosmetic warts**, neither exhibited nor blocking: `One(expr)` over an already-articled
   expression reads `one the Room with { … }`; an array argument keeps `new[] { … }`, the only place
   C# syntax survives into a claim.
10. **A trailing comma in an initializer emits a stray `}`.** `{ … To = new(2026, 8, 10), }` renders
    `… To = new(2026, 8, 10), } }]`: `ParseList` reads the comma as introducing another item and
    parses the closing brace as that item, which then prints alongside the real one. Exhibited twice
    in `Core.Spec`'s `When book` — deliberately, since the trailing comma is ordinary C# and
    removing it from the spec source only hides the defect. A parse bug, not a rendering one, so a
    pin belongs in `Core.Test` beside the other `ParseList` cases.
11. **An act that varies cannot be a branch, so it becomes a section.** One `When` per spec class
    means two spellings of the same call — booking a valid period and a zero-night one — are
    sibling `##` sections rather than `###` branches of one act, and the document cannot tell that
    they are the same endpoint. The workaround is to make what varies a tag and let branches supply
    it, which buys the branch structure at the cost of hiding the varying value from the heading.
    Not a defect so much as the shape the constraint forces; noted because every document with a
    refused-input case will meet it.
12. **A wrapped trailing phrase starts its line with the binder's comma.** `When Book(…)` followed by
    `, returns Booking` breaks as `…))` / `    , returns Booking`, because `AddWord` composes binder
    and word into one piece and layout moves the piece whole. The comma belongs to the line it binds
    to: a break has to fall *after* a binder, never before it. Exhibited twice in `Core.Spec`, and
    reachable by any long act with a return type. The fix is in composition, not layout — the binder
    wants to be its own unit, or to travel with the text it follows.

**Shapes not yet exercised**, where the next gaps will come from: a nullable return type (§5.6) and
branch trees three or more levels deep — where the two-level heading structure and §4 are pushed
hardest. Bookings exercised the rest: two subjects per document (§5.2), a second store to mock, a
`400` refusal, and acts that vary (§5.11).

## 6. Release notes material

Three lists. A consumer upgrading from 1.5.0 with pinned expectations cares only about the second.

**New in 2.0.0** — the generator: opt-in by
`[assembly: AssemblyFixture(typeof(SpecificationDocument))]`; subject resolved by convention and
verified against `deps.json`, failing before the first test; collection restricted to passing tests,
written only on a complete run; document layout (title, provenance, subject → branch headings,
requirements as a list, shared clauses hoisted); subject-under-test and return type declared where
they hold; sections ordered simplest first.

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

A dash means the count was never recorded, not that nothing moved. Re-count from the diff before the
notes quote a number.

**Document-only refinements**, worth one line between them: the return type is said on the act;
declared labels sit on the clauses and hoist independently; hoisting is decoupled from position;
dotted subject names title cleanly; layout is applied last; an item breaks where it no longer fits;
the document is 90 columns wide.

## 7. Before release

1. **Finish the docs.** `README.md` §7 and `TSpec-agent-reference.md` each carry a work-in-progress
   note about the opt-out attributes (§5.1), which are not being built for 2.0.0 — so the notes want
   rewording, not the feature. The reference also says "covers TSpec 1.5" while documenting post-1.5
   behaviour.
2. **Scratch project for the §1 xunit facts.** Fixture wiring, `TestState` at `Dispose` and
   end-of-assembly ordering cannot be self-tested from inside the same assembly; today only
   `MyHotel.Spec` passing verifies them, which will not catch a regression precisely.
3. **Set `PackageVersion`/`PackageReleaseNotes`**, untouched at 1.5.0.

**Everything ships as 2.0.0**, decided 2026-08-01: the generator is a breaking release in its own
right (§6, second list), so there is nothing to stage. Keep the rendering changes under their own
heading, separate from the removals below — they break different things, and a reader hitting
re-pinned expectations will not look under a heading about deleted types.

2.0.0 also drops the deprecated surface accumulated in 1.x, verified against the code 2026-08-03:

| Removed | Replacement |
|---|---|
| `Then<TService>()` — parameterless, on `Spec_Then`, `ITestPipeline` and `TestPipeline` | `Then<TService>(wasInvoked: Times)` |
| `And<TObject>()` — parameterless, on `IAndVerify` and `AndVerify` | `And<TObject>(wasInvoked: Times)` |
| `HasObject.Type<TObject>()` | `Is().A<T>()` / `Is().An<T>()` |

Removing those lets `IVerifyService`/`VerifyService` go — nothing else produces them. Also
`TODO.txt` line 1, failing a test whose pipeline never ran.
