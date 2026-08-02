# SPECIFICATION.md generation

Stage 1 of [TSpec-vision.md](TSpec-vision.md) §4: one `SPECIFICATION.md` per Spec project, generated
from the specifications a green test run produces. Branch `specification-generator`, shipping in
**2.0.0**.

**State:** feature complete, dogfooded on `MyHotel.Spec` — full CRUD plus a restart, 22 requirements,
111 lines.

**Caveat:** every §3 decision was settled against one shape of test — a single `MyHotel/Program.cs`,
CRUD, HTTP end-to-end. Decided-for-this-shape, not proven general; §9 lists what would test it.

§§1–6 are authoritative and corrected in place. §7 is release-note material, §§8–10 the open work.

## 1. Scope

**In:** collect each passing test's specification, check the run was complete and green, write one
`SPECIFICATION.md` per Spec project.

**Out:** laws, cross-assembly merging, CLI/MSBuild orchestration, the CI staleness gate.

## 2. Verified xunit facts

Probed 2026-07-26 — xunit.v3 3.2.2, net10.0, default parallelism. Re-verify if the version moves.

| Fact | Result |
|---|---|
| `TestState` at test-class `Dispose` | **Available**, despite the XML doc. `TestStatus` is still `Running` — use `TestState.Result`. |
| `[assembly: AssemblyFixture(T)]` disposal | **Runs last**, after every test, including after failures. |
| `[Fact(Skip=…)]` | Never constructs the class. No `Dispose`, no trace. |
| `Assert.Skip` | Constructs and disposes, reports `Skipped`. |
| Constructor throws | Never reaches `Dispose`. `Failed` to the runner, **invisible** to per-test collection. |
| Reflecting `[Fact]` methods and their `Skip` | Works; gives the expected set. |

The constructor-throws row is why §4 is load-bearing: without it, one such test lets a document
publish from a red run.

## 3. Decisions

**One `SPECIFICATION.md` at the Spec project root.** Subjects are headings within it. Reviewed whole,
named like `README.md`, globbable as `**/SPECIFICATION.md` — hence no cross-assembly index.

**Opt-in by one line**, `[assembly: AssemblyFixture(typeof(SpecificationDocument))]`. No environment
variable, no MSBuild property, no run mode.

**Subject by convention, verified against the build graph.** The assembly name minus its last suffix
(`MyHotel.Spec` → `MyHotel`), which must appear among the direct project references in `deps.json` or
`SetupFailed` throws before the first test. The version is what the build resolved. Rejected:
`Spec<TSubject>` (a black-box API spec's subject is `HttpClient`) and `GetReferencedAssemblies()`
minus `System.*`/`Microsoft.*` (swaps the user's naming convention for Microsoft's).

**Write only on a complete, green run** (§4); otherwise leave the file untouched.

**Skipped tests do not exist.** Recording only on `Passed` covers static skips, dynamic skips and
failures with no special cases.

**Erasure is justified semantically, never by taste** — is the token a *claim about the subject* or a
*mechanism of the test*? Erased: `await`, `async` with an explicit lambda return type, `!`, and `?.`
(as a plain `.`). Kept: `?` on a type, since `int?` and `int` differ in what values can occur. That
case is load-bearing — it is where erasure would have weakened a real statement. Erasure is
cumulative and one-directional, and no test can report that too much has been erased.

**Document and per-test specification render from one text.** The hundreds of `Specification.Is(…)`
expectations are simultaneously the document's regression tests, so every line in `SPECIFICATION.md`
is text a passing test produced. A second renderer would leave every requirement in the repo
unpinned. The document may add *structure around* the text — headings, grouping, hoisting — never a
different version of it.

**Three phases.** Phase 1 (the phrase classes) describes each step into a `SpecificationStep`: what
it says, no layout. Phase 2 (`SpecificationRenderer`) holds everything positional — lead words,
mock-name elision, whether an assertion starts a sentence — and returns a `ComposedText`: every word
settled, no line broken. Phase 3 (`TextBuilder`) lays that out at the width it is given. Phase 1's
output is what lets the document arrange clauses without becoming a second renderer; phase 2's is
what lets it take text off before anything has been measured.

**Layout is last.** A line is broken against the text that reaches the page, so nothing may shorten
or lengthen it afterwards. The document removes the word its heading already said, and knows a
fenced item is indented two columns, *before* it calls phase 3 — each consumer wraps at its own
width. What phase 3 must never be handed is text something else still intends to edit. The composed
form keeps its pieces separate rather than concatenating them, because their boundaries are what
`FitsOnOwnLine` reads.

**A clause is the unit, not a line** — one line-starting step plus everything appending to it, so
`Given IMyService.GetValue() first returns 1 and next returns 2` is one clause from three steps. Line
breaks and a leading `and` are phase-2 artifacts and carry no meaning.

**Every family says its word once, then its binder** — `Using`, `Given`, `When`, `Having`, `Until`,
`Then`. Steps list in **declaration order** uniformly, pinned in `Core.Test/Pipeline/HavingWhenUntil.cs`.
The binder is "and", except for the two families that act in time: setups read **after** and
teardowns **before**, so the text states the order they ran in. `Given` keeps "and" — its reverse
order is precedence between values, not a sequence of acts. Chosen over reordering clauses into
execution order, which hides the inversion where the binder makes it legible.

**The document states subject-under-test and return type**, each at the highest heading where every
requirement below agrees on *it* — independently, so a subject can hold for a whole document while
every section returns something different:

```
Subject under test: HttpClient
Return type: HttpResponseMessage
```

**Where a heading states both a return type and the act, the type is said on the act** —
`When Add(a Room), returns Room` — since what a method returns is a fact about it, not about the
section. Added 2026-08-02. It takes the shape `because` already has after an assertion: a trailing
word-step with `", "` for a binder and no family, so it qualifies the act rather than continuing it.
The subject has no clause to belong to and stays a label. Where the two land at different headings —
as in `MyHotel.Spec`, whose return type holds for the whole document while each act sits at its
subject — the label stands alone as before. This does put apparatus inside a clause, which the
"never a step" rule below otherwise keeps out; the cost is one rendering rule, and the gain is that
every declared-type block in `Core.Spec` collapsed from a fence to one inline line.

Labels, not a sentence — no one phrasing survives `Spec<int>` over a static calculation. Inside the
fence because a return type can contain `<` and `>` (§6). `Spec<T>` states the type twice rather than
earn a special case; non-generic `Spec` omits both lines. Stated together they are padded into a
column, because two labels read as a pair; alone a label has nothing to line up with and is written
plainly. They sit directly above the clauses, with no blank line: apparatus and specification are
different kinds of statement, but a label already announces itself as one, and the gap cost a line of
height on every heading that states both. Corrected 2026-08-02, having set them apart at first.

**A type argument states something only where the act uses it in that capacity.** Corrected
2026-08-02, after a first attempt read `Spec<T>` as "subject only" — which is wrong, since
`Spec<TSUTorResult>` is named for the ambiguity and the one argument may be either or both. Nothing
in the declaration can settle it, but the act can: every `When` overload knows from its own signature
whether it is handed the subject and whether it yields a result, so it tells the pipeline instead of
being inferred from. An act taking no subject leaves a generated value nothing reads; one returning
`void`, `Task` or `ValueTask` has no return type whatever `TResult` says. `Spec<T>` then needs no
case of its own — it states T twice where both are used, once where one is.

This is inference, so it cannot be checked: a spec that means "no subject" and an act that merely
ignores one are indistinguishable. The explicit form — declaring `Nothing` and having `SetupFailed`
enforce it — is in `TODO.txt`, and is breaking because the non-generic `Spec` would become
`Spec<Nothing, Nothing>`.

**That pair is apparatus, not a step.** As a `SpecificationStep` it would enter every per-test
specification and move all 1400-odd pins. Excluded from `ComplexityNumber`, where it cancels anyway.
Reading it walks the *inheritance* chain of the closed `Spec<,>` — not the nesting walk behind the
headings (§6) — and must recognise non-generic `Spec` first, since that is `Spec<object, object>`.

**A requirement is a list item; only subjects and branches get headings.** A fourth heading level
renders at body size and cannot be told from the third; a heading plus a fence is two elements whose
four margins were most of the document's height; and a list item is a *different kind of thing* from
a heading, which is what lets two levels read as a hierarchy at all. Halved the MyHotel page. Answers
vision §11 Q1 — a requirement needs a name *and* a claim, but the name is a label.

Consequences, all from "nothing repeats what its position already states":

- One line of specification goes inline in a code span, several keep the fence — so a fence now
  *means* "more than one statement".
- A requirement whose specification exceeds its claim keeps the fence indented into its item.
- A block never opens with the word its heading just said; an item says no `Then`, because its place
  in the list is the `Then`.
- Only a block's *opening* word is dropped, and only where something above says that same word. A
  `Having` under a `Given` heading states something new; deeper in a block the lead words are the
  structure of the sequence.

**Sections order by `ComplexityNumber`, simplest first** — a node's own clauses in every phase but
Assert (`Using`, `Given`, `When`, `Having`, `Until`), plus its children's. Applies within a grouping,
never across one.

- **Own clauses are the hoisted ones**, not what the requirements carried before hoisting — otherwise
  a subject's number grows with how many requirements it has and the measure is size by another
  route. Hence `DocumentRenderer` builds `SubjectNode`/`BranchNode` and sorts those, not a flat list.
- **Assertions contribute nothing**, so adding a requirement leaves every key in the tree unchanged.
  The number moves only when arrangement appears or disappears — a structural change worth seeing in
  a diff. Summing size would reorder whole subjects over one added line.
- **Breadth and depth both raise it**, deliberately: at most one child can contribute zero, so a
  subject with n branches carries at least n−1. The `When` cancels among subjects.
- **At the leaf, ties break on rendered length**, then alphabetically — length puts the short status
  check before the one inspecting a whole value. Never overrides arrangement, decides nothing above
  the leaf. A good proxy only while the source is clean, so an odd sort is first a question about the
  test that produced it.
- **Alphabetical is a placeholder for something semantic.** Changing the rule reflows every
  `SPECIFICATION.md` at once — closer to a file format than an implementation detail, so: major.

## 4. Completeness check

At `SpecificationDocument.Dispose()`, before writing: reflect the assembly for every participating
test method, subtract those with `Skip`, compare against the set that reported in, write only on an
exact match, otherwise leave the file alone and report why.

One rule catches all three failure modes — a filtered run reports fewer, a failed test never reports,
a constructor-throws test never reports. Deliberately no separate "was anything red" flag.

**Staleness is the consumer's gate, not ours.** The document is deterministic, so the check exists:

```bash
dotnet test && git diff --exit-code -- "**/SPECIFICATION.md"
```

It requires a `.gitattributes` normalising line endings (`* text=auto eol=lf`). Without one a Windows
and a Linux checkout compile different bytes, the build id moves, and the gate fails on every run for
a reason that looks nothing like its cause.

## 5. Hoisting rules

What every requirement under a heading opens with is stated once, under that heading.

1. **Whole clauses only** — never a line, never a fragment.
2. **Shared by every entry, each clause decided on its own.** Exact match, no partial credit, and
   position is not a test. Corrected 2026-08-02: the rule used to take the shared *opening*, a common
   prefix. But the specification is written in the order the pipeline runs — arrangement before the
   act — so a branch arranging anything of its own sits in front of the `When` its whole subject
   shares, and the prefix stopped there. `MyHotel.Spec` was right only because nothing precedes its
   acts; `Core.Spec` broke the moment a `Given` did, leaving `When Delete(…)` restated in every
   branch of the heading named after it. The coupling was the fault, not the order.
3. **As many times as every entry says it.** How often a clause is stated is part of what it states —
   two identical `Having` steps are two invocations — so a clause rises the fewest number of times
   any entry says it, and the remainder stays below. Hoisting a repeat above an entry that says it
   once would put a claim on the page nothing made.
4. **No minimum number of entries.** The rule was *repetition*, the intent is *placement*; hoisting
   is not compression, and at one or two sharers it costs more lines in fences than it saves.
5. **Every level the document has**, today document → `##` subject → `###` branch. Not generalised to
   arbitrary depth; a new level would add one.
6. **Assertions never hoist** — two requirements agreeing on one is a coincidence worth seeing, and
   with no threshold this is the only thing guaranteeing a requirement block still says something.
   Qualified by §8.5.

A leading `and …` is never left behind: lead words are assigned while rendering.

**Deliberately not built.** Placement is inferred from sharing, a proxy — TSpec does not record which
class declared a clause, so a `Having` from a branch constructor and one from a `[Fact]` are
indistinguishable. The successor is a family-to-level ceiling (`When` no higher than its subject,
`Having` no higher than its branch, arrangement anywhere), which needs a naming convention enforced
on test classes. **Trigger:** a clause above the heading that names it — a `When` at document level,
possible today in a single-subject document.

## 6. Facts that were expensive to establish

**The specification freezes at first observation.** `Specification.Is(…)` reads it from inside a test
and then asserts — itself a recordable step. Without the freeze a test describes checking its own
description. Invisible to the suite, visible only in the document.

**Compose phrases after describing, never before.** Assertion phrases used to prepend a word to the
*raw* expression and parse the splice; `"by (it, i) => it + i"` is not C#, and once the grammar
learned lambda return types it read `by` as one and silently swallowed it. Parsing now only ever sees
real C#, which retires the collision class instead of enumerating it.

**A code context is load-bearing.** Specifications contain `<` and `>` (`Result.Read<Room>()`), which
markdown outside one eats as a tag, and two-space continuation indents that collapse. A fence and an
inline code span both provide it, which is what makes the one-line form safe. Also why tag names are
emphasised by capitalization rather than `**`: the same text is failure output in a terminal.

**`Tag.Name` never reaches the specification text** — every tag word comes from the captured source
expression; `Name` labels a value in a failure report. A tag is named after its variable only in a
*field* initializer, since `[CallerMemberName]` reports the enclosing member. Names must be unique
within a test; a clash throws `SetupFailed`, which catches two locals sharing their method's name.

**A drilldown after a tag reads possessively**, as one after a mention does — `The(_updatedRoom).RoomNumber`
is "the UpdatedRoom's RoomNumber". Added 2026-08-02: the possessive rule lived only in the mention
describer, so a member access on a tag fell through to raw source and put C# on the page.

**A tag says what a constant cannot.** `const RoomNumber = "101"` renders as its name and hides the
value; a tag renders as `the RoomNumber` — the *identity*, which was the actual claim. Where the
value matters, write it literally and it renders.

**The class chain follows nesting, not inheritance.** A `BaseType` walk yields `ApiSpec\`1 →
WhenGetVersion → GivenNothing`, because a shared black-box base sits between the test and `Spec`.
Nesting gives what the recommended structure expresses; shared bases are scaffolding.

**Mentions built inside `Having` steps come out in creation order.** A numbered mention takes its
value when *first requested*, and setups run last-declared-first, so the first-running setup creates
first and gets the lowest value. Useful: declare setups in reverse and `A<T>` is the first-created
entity throughout — `WhenListRooms.GivenTwoRooms` declares its second room first. Limit: the two
orders always coincide, so such a requirement cannot distinguish "in creation order" from "sorted by
the generated value"; breaking the tie costs the readable setup lines, which MyHotel declines since
no implementation there sorts.

**The document normalises line endings at its own boundary.** `Specification.ToString()` documents
platform-native endings, so that is not a bug to fix at the source.

## 7. History

User-facing, in order — the material for release notes:

| Change | Effect |
|---|---|
| `SpecificationDocument` assembly fixture | Opt-in generation; misconfiguration fails before the first test. |
| Subject resolution | By convention, verified against `deps.json`; failures state both expectations. |
| Collection + completeness | Only passing tests recorded; written only when reported set matches expected. |
| `Having` / `Until` keywords | Setup and teardown render under the pipeline method that produced them (was `After` / `Before`). |
| Noise erased | `await`, `async`, `!`, `?.` no longer appear. |
| Interpolation holes described | `$"/rooms/{The(x)}"` renders its hole as prose, not source. |
| Tags name themselves | `[CallerMemberName]`; `nameof(…)` no longer needed for a tag field. Names unique within a test. |
| Tag names normalized | `_roomNumber` renders as `RoomNumber`. |
| Document layout | Title, provenance comment, subject → branch headings, shared clauses hoisted. |
| Subject-under-test and return type | Stated at the highest heading where they hold. Document-only. |
| Ordered by `ComplexityNumber` | Every level reads simplest first, not alphabetically. Ties break on claim length, then name. |
| Wrapping keeps expressions whole | An over-long phrase moves to the next line entire instead of splitting at the last break cue. 19 pins moved. |
| Requirements are a list | Two heading levels, not four; one line inline, several fenced. Halved the MyHotel document. |
| `Having` / `Until` binders | Setups read `after`, teardowns `before`. **Changes per-test text**, not only the document — 3 pins moved. |
| `with` expressions name their target | `The<Room>() with { … }` rendered as its members alone, which is not a room. Erasure narrowed to `p => p with { … }`. Also fixes `_ => _.Inner with { … }` dropping `_.Inner`. |
| Tag drilldown reads possessively | `The(_room).RoomNumber` renders "the Room's RoomNumber" where it showed the raw expression. Mentions already did this; tags did not. |
| Return type said on the act | Where a heading states both, `Return type: Room` joins the act as `When Add(a Room), returns Room` instead of standing as a label. Document-only. |
| Declared types sit on the clauses | The blank line between `Return type:` and the clauses below it is gone — it cost a line of height on every heading stating both, and a label sets itself apart by being one. Document-only. |
| Hoisting decoupled from position | A clause shared by every requirement is stated at the heading wherever it sits, so a branch's own `Given` no longer keeps the subject's `When` out of the heading named after it. Multiplicity respected: a clause rises as often as the requirement saying it fewest times says it. Document-only. |
| Declared types follow the act | `Subject under test:` is stated only where the `When` takes the subject, `Return type:` only where it yields a result — so a `Task`-returning act names no return type and `When(() => …)` names no subject. Makes `Spec<T>` work for either role. Document-only. |
| Dotted subject names title cleanly | `MyHotel.Core.Spec` titles its document `# My Hotel Core`, where it read `# My Hotel. Core`. Document-only. |
| Collection mentions pluralize | `Two<Room>()` renders "two Rooms" where it read "two Room"; `Many<Query>()` gives "many Queries". Everything but `One`. A plural drilldown takes the bare apostrophe — `three MyModels' Last()`. **Changes per-test text** — 57 expectations re-pinned. |
| Declared labels hoist independently | `Subject under test:` and `Return type:` each rise to the highest heading where every requirement agrees on that label, instead of both falling when either disagrees. A lone label is written without the column. |
| Layout applied last | Wrapping counted the `Then` the document then stripped, so a 76-character claim measured 81, broke, and took a fence it never needed. Compose and lay out are separate phases now, each consumer wrapping at its own width. Document-only; per-test text byte-identical. |
| An item breaks where it no longer fits | A claim was measured for its fence but written beside its label, uncounted — 14 item lines ran past 80, the longest 113. A claim that no longer fits beside its label now takes the line under it, the break saying what the dash said. No line of either document exceeds 80. Document-only. |
| Subject parameter elided | `When(_ => _.Api.Get("/x"))` renders `When Api.Get("/x")`; `++_.Counter` renders `++Counter`. `When`/`Having`/`Until` only, wherever the parameter heads a chain. Mock setups, `Given` setups and assertion predicates keep theirs. **Changes per-test text** — 291 expectations re-pinned. |

Notable internals: the two-phase engine (§3) was the largest change and the enabler for the rest;
`Expr.ToSource()` rebuilds an expression from the tree so erased keywords cannot return through a
parent's raw text; `ExpressionParser` → `ExpressionDescriber`, and
`ParseValue`/`ParseCall`/`ParseActual` → `Describe`/`DescribeCall`/`DescribeActual`.

## 8. Known gaps

1. ~~**A weak assertion cannot tell "not implemented" from "correctly absent."**~~ **Fixed
   2026-08-02.** `GivenNoSuchRoom.ThenRespondNotFound` passed before any endpoint existed — an
   unmatched route also returns 404, so the claim held whether or not anything implemented it. Each
   of the four 404 branches now also states `say which room` — the refusal names the room it refused
   — which only a handler can produce, since an unmatched path answers with an empty body. Verified
   by deleting every route on `/rooms/{roomNumber}`: `respond not found` still passes, `say which
   room` fails. That the body existed to assert on is recent; before the global exception handler
   there was nothing there to name.

   The general form is worth keeping in view: **an assertion that only checks an absence cannot
   distinguish "not implemented" from "correctly absent."** Assert something only the implementation
   can produce. A 409 does not have the problem — an unmatched route answers 404, not 409 — so the
   rule bites on whatever status the framework itself would give.
2. **Opt-out attributes.** `[Specification]` / `[ExcludeFromSpecification]`, nearest declaration
   wins, default include. Nothing has needed to opt out. Polarity: the framework cannot detect tests
   that were never written, so absence never certified coverage.
3. **Namespace as a grouping level.** Not collected — with few subjects it adds a heading level for
   nothing. Only about headings; ordering does not need it. **Trigger:** several subjects (§9).
4. **A second assertion starts an orphaned sentence.** Only the first gets `Then`; the next is
   capitalized on a new line with no connective, so `Second is new Room(…)` reads as a claim about
   nothing. Pinned deliberately in `WhenTwoItems.cs:24` and `HavingWhenUntil.cs:67`. Deferred with
   two neighbours — lowercase continuation, and breaking after `that`. All three need the same thing:
   assertions carry no `StepFamily`, and synthesising the group means knowing where it ends while
   phase 2 streams steps one at a time. **Trigger:** phase 2 buffers a whole assertion.
5. **A subject-wide assertion repeats under every branch.** `ThenRespondOk` on `WhenListRooms` is
   inherited by both branches and stated twice. §5 rule 6 holds for branches that merely *agree*, but
   an assertion *declared above* them is a claim about the subject and the document cannot tell the
   two apart. Same missing input as §5's "deliberately not built".
6. **A `with` block wraps badly.** `{` is a break-after cue in `TextBuilder` and the greedy fit takes
   the last cue that fits, so a long `with` breaks inside itself. Visible in `When update room`,
   which reads `… the Room with { BedCount = a` / `second int })`. Worse since layout moved last
   (§7): the heading no longer measures the `When` it drops, so five more characters fit and the
   last cue that fits is a worse one. Fix: stop treating `{` as a cue when the block would fit on a
   continuation line, as `FitsOnOwnLine` already does for phrases. Touches every wrapped
   specification in the suite, so it wants its own session.
7. **The binder is silent across a hoist boundary.** A branch sharing its first `Having` but not its
   second gets `Having X` in the heading and `Having Y` in the item, with nothing relating them in
   time — the family restarts when a block starts. Not a regression ("and" was equally silent), not
   reachable in MyHotel today; the one gap §3's binder rule leaves open.

Found 2026-08-01 in a `Core.Spec` run since reverted (§9). All three are facts about the renderer,
not about that suite, and reappear as soon as any spec has a non-`HttpClient` subject. All three are
invisible in a single-subject HTTP document, which is why nothing before now could catch them.

8. **The declared return type drops reference-type nullability.** `Spec<RoomService, Room?>` renders
   `Return type: Room`, directly above a requirement reading `return no room — Result is null`. The
   document contradicts itself on the page. `Room?` is `Room` plus a `NullableAttribute` in IL, and
   the §3 walk up the closed `Spec<,>` reads `Type` only, so the `?` is gone before rendering.
   `int?` survives, being a distinct type — which is why §3's load-bearing erasure case never caught
   this. Reading it needs `NullabilityInfoContext` over the generic argument.
9. ~~**`AsTitle` does not split on `.`.**~~ **Fixed 2026-08-02.** `MyHotel.Core` titled its document
   `# My Hotel. Core`, which reads as two sentences. It now splits on `.` before splitting words and
   joins with a space — `# My Hotel Core`. Deliberately unlike `AsHeading`, which splits on `.` and
   joins with `". "`: a branch path is a sequence of sentences where a title is one name. Untestable
   before a subject name contained a dot, which needed a second production project.
10. ~~**The declared pair does not hoist independently.**~~ **Fixed 2026-08-02.** The two labels were
    hoisted together, so `Core.Spec` — where the subject agrees everywhere but the return type does
    not — stated `Subject under test: RoomService` five times. Each label now hoists to the highest
    heading where *it* holds. The column was the open question: a lone label has nothing to line up
    with, so it is written plainly, and being one line it renders inline rather than fenced, which
    the existing one-line rule already decided. `Core.Spec` went from 123 lines to 108;
    `MyHotel.Spec`, where both still agree, is unchanged.

Found 2026-08-01 in `MyHotel.Spec`, and since designed out of it rather than fixed.

11. **One outlier costs every sibling its hoisting.** §5 rule 2 was exact-match on a shared opening with no partial
    credit, so one section that differs unhoists the whole level. A restart spec with subject
    `Hotel` rather than `HttpClient` pushed the four shared lines — the declared pair plus two
    `Using` clauses — out of the document header and into all six HTTP sections: 105 lines became
    141 for one added requirement. The rule behaves as specified; the question is whether "shared by
    every entry" should become "shared by all but the few that say otherwise", with the dissenters
    restating. Interacts with §8.10 — per-line hoisting would have kept `Return type` at the top,
    since every section still agreed on it.

    Resolved for MyHotel by making `Hotel` the subject of *every* black-box spec, which was the
    better statement anyway: the thing under test is the application, and `HttpClient` is how it is
    reached. That also deleted `Using owned api / and owned api.CreateClient` from the page — pure
    mechanism, present only because a client needs disposing, where `Hotel` is constructed and
    disposed by the pipeline. 22 requirements now render in 111 lines against the old 20 in 105.
    **The gap is unfixed**; MyHotel simply no longer exhibits it.
Found 2026-08-02 in `Core.Spec`, the first document built on mocked collaborators.

13. ~~**No way to declare a subject with no result.**~~ **Fixed 2026-08-02.** A `void`/`Task` method
    on a subject had nowhere honest to go: `WhenDeleteRoom : Spec<RoomService>` printed
    `Return type: RoomService`, which is false, and `Spec<RoomService, Task>` declared the wrapper
    rather than the absence. Each label is now stated only where the act uses that capacity (§3), so
    the same rule also covers the opposite case — a static method under `Spec<int>`, which claimed a
    subject that is only an unread generated value. `Core.Test`'s own
    `WhenSplitIdentifierIntoWords : Spec<string>` was one.

    My first attempt read `Spec<T>` as "subject only", which the PO rejected: `Spec<TSUTorResult>`
    is named for the ambiguity and the argument may be either or both. The declaration cannot settle
    it; the act can.
14. **`Given` loses its word under a `Given` heading.** A branch block opens
    `IRoomStore.Load() returns zero Room` with no lead word, because §3 drops a block's opening word
    where something above says it, and the heading is `### Given no such room`. But the heading's
    "Given" is part of a *name* and the clause's is a family keyword; dropping one because the other
    is spelled the same leaves a bare sentence with no grammatical subject. Not reachable in
    `MyHotel.Spec`, where every branch block opens with `Having`.
15. **A collection mention wrapping an expression reads badly.** `One(The<Room>() with { … })`
    renders `one the Room with { BedCount = any int }` — "one the Room" is not English. `One<T>()`
    alone is fine (`one Room`); the fault is `one` in front of an already-articled mention.
    **No longer exhibited**: the spec that raised it was wrapping a value that was already a
    mention, so `One(…)` bought nothing and a collection expression says it plainly —
    `returns [the Room with { BedCount = any int }]`. The gap is unfixed; `One(expr)` over an
    articled expression still reads this way.

    Unrelated to the plural spelling, fixed 2026-08-02: `Two<Room>()` now reads "two Rooms". The
    orthography is shared with `PresentSingularS` rather than copied, since English spells noun
    plurals and third-person verbs alike — which also fixed a latent bug there, `-y` becoming
    `-ies` after a vowel. Three call sites needed it (mention, `Given` count, `Given` count with a
    setup), so the factory list lives once in `StringExtensions.CountedBy`.
16. **An array argument renders as source.** `Save(new[] { the third Room, the second Room })` keeps
    `new[] { … }`, where a collection mention would have said `two Room`. Honest but noisy, and it is
    the only place in either document where C# syntax survives into a claim.

17. ~~**A shared clause behind a differing one never rises.**~~ **Fixed 2026-08-02.** Hoisting took the
    shared *opening* — a common prefix — but the specification is written in pipeline order,
    arrangement before act, so a branch's own `Given` sat in front of the `When` its whole subject
    shares and stopped the prefix dead. `Core.Spec` restated `When Delete(…)` under both branches of
    the heading named after it, and ran `IRoomStore.Load() returns zero Rooms` straight into
    `When Delete(…)` as though one sentence. `MyHotel.Spec` had been right only because nothing
    precedes its acts. The order was never the problem — Given–When–Then is correct, arrangement does
    run first — the coupling was: each clause is now decided on its own (§5 rules 2–3), including how
    many times it is stated. `Core.Spec` went from 108 lines to 91.

**Fixed 2026-08-01**, unlike 8–11.

12. ~~**The subject-under-test receiver is never elided.**~~ Every clause used to name it —
    `_.Restart()`, `_.Api.GetAsync(…)` — under a heading block already saying `Subject under test:
    Hotel`. That was the one repetition §3 did not erase, and it also forced a naming decision that
    should never have been the author's: `_` reads fine in code but rendered verbatim, so the page
    said `_.Api.PostAsJsonAsync(…)`, a name identifying nothing; calling the parameter `hotel`
    only traded one repetition for another. Eliding made the choice invisible, which is what showed
    it belonged to rendering rather than to naming.

    `SubjectElision` rewrites the parameter out of the lambda body before description, wherever it
    heads a chain — arguments included, since `new(_.GetNextId(), _.GetConnectionString())` is the
    subject doing two things and eliding one but not the other would read as two different sources.
    A bare `_` passed as a value stays: it names the subject rather than something it did, and there
    would be nothing left to render. `Raw` is recomputed per rewritten node, since describers fall
    back to it.

    Two things the change turned up. `Lambda.IsParamRef` treats `_` as a wildcard matching *any*
    receiver, which is right for recognising the shape but wrong for eliding — `_ => MyService.Echo(…)`
    calls a static class, and elision has to check the receiver really is the parameter. And `_`
    outside `When`/`Having`/`Until` is not the subject: mock setups name their service, `Given` value
    setups and `throws … where _.Message` keep theirs. 291 pinned expectations moved.

## 9. Widening MyHotel

**In progress 2026-08-01: MyHotel moves to Neat**, the PO's architecture. Everything now sits under
`SampleProjects/MyHotel/`. `MyHotel` keeps its name and plays Host, because the deployable is the
application; `Entry` (endpoints), `Contract` (the public shape, referencing nothing) and `Core`
(logic, vertical subdomains) exist as empty projects, plus `Core.Spec`. Entry and Core never
reference each other; the compiler is the boundary. Infra skipped — storage is a list, so the layer
would be empty. Placement rules are in `SampleProjects/MyHotel/CLAUDE.md`.

**Open: how `Program.cs` decomposes into the layers.** The PO leads this; a first attempt was made
and reverted unasked-for. What the decomposition has to settle: which models and interfaces belong
in Contract, whether Core exposes one service per subdomain or finer types, and how a Core operation
reports an outcome that Entry turns into a status code without Core knowing HTTP exists.

One constraint on that, found while attempting it: **assertions only see `Result`**. TSpec exposes no
subject to the test, so a spec claims only what its `When` returns. An operation reporting a bare
`bool` can therefore only be stated as that flag; its *effect* must be stated under the read that
observes it. This is §8.1's lesson arriving as a structural constraint, and it makes the Contract's
shape a question about what the document can say, not only about layering.

**`Core.Spec` written 2026-08-02** — 12 requirements over `RoomService` with `IRoomStore` mocked, in
a `RoomService/` folder per the PO's convention. Four shapes at once: a subject that is not
`HttpClient`; return types `Room`, `IReadOnlyList<Room>` and `Task`; arrangement carried by `Given`
mock setups instead of `Having`; and exception claims (`throws RoomNotFound`). It is also the second
`SPECIFICATION.md`, so one-file-per-project is exercised for the first time. It produced §8.13–16 and
put §8.9 and §8.10 back in the repo as visible evidence.

Shapes still unexercised:

- **Namespace grouping (§8.3), which does not follow from layering.** Each layer gets its own Spec
  project and therefore its own document, so layers produce *more documents*, not more namespaces
  within one. The `##` heading is the operation (`When add room`); grouping becomes visible only when
  one Spec project holds operations in different namespaces — under Neat, a **second vertical
  subdomain** in Core. Neither rooms alone nor the `RoomService/` folder will raise it: one folder is
  one namespace.
- **A nullable return type (§8.8).** `RoomService.Get` throws rather than returning `Room?`, so the
  dropped `?` is still invisible in both documents. It needs a Core method that answers "no" with a
  value instead of an exception.
- **Branch trees three or more levels deep** — where the two-level heading structure and §5's
  hoisting are pushed hardest.

## 10. Before release

1. **Finish the docs.** `README.md` §7 and `TSpec-agent-reference.md` are marked work-in-progress,
   and the agent reference says "covers TSpec 1.5" while documenting post-1.5 behaviour.
2. **Scratch project for the §2 xunit facts.** Fixture wiring, `TestState` at `Dispose` and
   end-of-assembly ordering cannot be self-tested from inside the same assembly; today they are
   verified only by `MyHotel.Spec` passing, which will not catch a regression precisely.
3. **Set `PackageVersion`/`PackageReleaseNotes`**, untouched at 1.5.0.

**Everything ships as 2.0.0.** Decided by the PO 2026-08-01: no 1.6.0 in between, since this project
has their full attention until it is finished and nothing needs to reach users before it does. The
earlier plan staged 1.6.0 → 2.0.0 → 2.1.0 to keep the generator from holding the deprecation cleanup
hostage; with no gap between them there is nothing to decouple, and the generator is a breaking
release in its own right anyway (the `Having`/`Until` binders and the `with` fix both change text
1.5.0 already emits, so a project with pinned `Specification.Is(…)` expectations sees failures on
upgrade).

2.0.0 therefore carries: the specification generator; the removals — three `[Obsolete]` members plus
`IVerifyService`/`VerifyService`, per IMPROVEMENT-PLAN.md; and `TODO.txt` line 1, failing a test
whose pipeline never ran. The release notes must state the rendering changes separately from the API
removals — they break different things and a reader hitting re-pinned expectations will not look
under a heading about deleted types.
