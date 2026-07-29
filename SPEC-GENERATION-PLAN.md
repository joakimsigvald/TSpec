# SPECIFICATION.md generation

Realizes Stage 1 of [TSpec-vision.md](TSpec-vision.md) §4: one `SPECIFICATION.md` per Spec project,
generated from the specifications a green test run produces. Built on branch
`specification-generator`, targeting release **2.1.0**.

**Working state:** feature complete and dogfooded on `MyHotel.Spec`. What is left is in §8.

Sections 1–6 are the design and stay authoritative — when a decision changes they are corrected in
place, never contradicted from elsewhere. §7 is the history, kept short enough to be useful and long
enough to write release notes from.

## 1. Scope

**In:** collecting each passing test's specification, checking the run was complete and green, and
writing one `SPECIFICATION.md` per Spec project.

**Out:** laws; cross-assembly merging; a CLI tool or MSBuild orchestration; the CI pipeline that
runs the staleness gate.

## 2. Verified xunit facts

Probed 2026-07-26 against xunit.v3 3.2.2 on net10.0 under default parallelism. The design rests on
these — re-verify if the xunit version moves.

| Fact | Result |
|---|---|
| `TestContext.Current.TestState` at test-class `Dispose` | **Available**, despite the XML doc saying otherwise. `TestStatus` is still `Running` there — use `TestState.Result`. |
| `[assembly: AssemblyFixture(T)]` disposal | **Runs last**, after every test, including when tests failed. |
| Statically skipped test (`[Fact(Skip=…)]`) | Never constructs the class. No `Dispose`, no trace. |
| Dynamically skipped test (`Assert.Skip`) | Constructs and disposes, reports `Skipped`. |
| Test whose **constructor throws** | Never reaches `Dispose`. Counted `Failed` by the runner but **invisible** to per-test collection. |
| Reflecting `[Fact]` methods and their `Skip` at runtime | Works; gives the expected set. |

The constructor-throws row is why §4 is load-bearing rather than a convenience: without it, one such
test would let a document publish from a red run.

## 3. Decisions

**One `SPECIFICATION.md` at the Spec project root.** Subjects are headings within it, not separate
files. Reviewed as a whole, named like `README.md`, globbable as `**/SPECIFICATION.md` — which is
also why no cross-assembly index is needed.

**Generation is opt-in by one line**, `[assembly: AssemblyFixture(typeof(SpecificationDocument))]`.
No environment variable, no MSBuild property, no run mode. A project without that line behaves
exactly as before.

**The subject is derived by convention, then verified against the build graph.** The subject name is
the spec assembly name with its last suffix stripped — `MyHotel.Spec` describes `MyHotel`, and *any*
suffix works. That name must appear among the direct project references recorded in the spec
assembly's `deps.json`, or `SetupFailed` is thrown before the first test. The version is the one the
build resolved, i.e. `<Version>` in the production project file. Rejected: `Spec<TSubject>` type
arguments (a black-box API spec's subject is `HttpClient`, so this collapses) and
`GetReferencedAssemblies()` filtered by `System.*`/`Microsoft.*` (swaps the user's naming convention
for Microsoft's).

**Write only on a complete, green run; otherwise leave the file untouched.** A filtered run must not
truncate the document and a red run must not publish one — the same check either way (§4).

**Skipped tests do not exist.** They contribute nothing and are excluded from the expected set.
Recording only on `Passed` handles static skips, dynamic skips and failures with no special cases.

**Erasure is justified semantically, never by taste.** The recurring question is whether a token is
a *claim about the subject* or a *mechanism of the test*; mechanism is erased. Erased: `await`,
`async` plus an explicit lambda return type, `!`, and `?.` (rendered as a plain `.`). Kept: `?` on a
type, because `int?` and `int` differ in what values can occur — that is a claim. The `?` case is
the rule's load-bearing one: it is where erasure would have weakened a real statement, and that is
what stopped it. Erasure is cumulative and one-directional, and no test can report that too much has
been erased.

**The document and the per-test specification render from one and the same text.** Not because it is
simpler, but because the hundreds of `Specification.Is(…)` expectations are what keep the document
honest: with one renderer they are simultaneously the document's regression tests, and every line in
`SPECIFICATION.md` is text a passing test actually produced. A second renderer would leave every
requirement in the repo unpinned. The document may add *structure around* the text — headings,
grouping, hoisting — but never a different version of it.

**The engine has two phases.** Phase 1 (the phrase classes) describes each pipeline step into a
`SpecificationStep`: what it says, with no layout. Phase 2 (`SpecificationRenderer`) lays steps out,
holding everything positional — lead words, mock-name elision, whether an assertion starts a
sentence. Phase 1's output is the structure served to both consumers, which is what lets the
document arrange clauses differently without becoming a second renderer.

**A clause is the unit, not a line.** One line-starting step plus every step that appends to it, so
`Given IMyService.GetValue() first returns 1 and next returns 2` is one clause built from three
steps. Line breaks and a leading `and` are phase-2 artifacts and carry no meaning.

**Every family says its word once and reads "and" after that** — `Using`, `Given`, `When`, `Having`,
`Until`, `Then`. Consecutive steps of a family are listed in **declaration order**, uniformly, and
pinned that way in `Core.Test/Pipeline/HavingWhenUntil.cs`. For `Until` that is also execution order;
for `Having` it is the reverse, and that is correct: `README.md` states the rule (`When(A).Having(B)
.Having(C)` executes C → B → A), so a reader who needs the order has it, and the document should show
the declarations rather than restate a rule the package already documents.

**Sections are ordered by `ComplexityNumber`, simplest first** — don't go for the gold too early. A
node's `ComplexityNumber` is the count of its own clauses in every phase but Assert — `Using`,
`Given`, `When`, `Having`, `Until` — plus the sum of its children's. Ordering applies *within* a
grouping and never across one: requirements within a branch, branches within a subject, subjects
within the document, later folders and projects.

**At the leaf, a tie breaks on the length of the rendered requirement**, then alphabetically. Most
requirements of a branch arrange identically, so without it the leaf would be sorted by name, which
says nothing; length at least puts the short status check before the one that inspects a whole value.
It is a proxy for how much a requirement claims and it is only a tiebreak — it never overrides
arrangement, and it decides nothing above the leaf, where sections are large enough that ordering by
their size would be the churn the measure exists to avoid. Expect to replace it: length tracks
spelling, not weight. MyHotel demonstrated both directions within an hour — `Then return no rooms`
outranked `Then respond ok` while the status read `is HttpStatusCode.OK`, and they swapped back when
a `using static` shortened it to `is OK`. Neither claim had changed. Elsewhere ties break
alphabetically.

The measure is arrangement, not size, and that is what makes it safe to sum upward. Assertions
contribute nothing, so adding a requirement to an existing branch leaves every number in the tree
unchanged; the number moves only when arrangement appears or disappears, which is a structural change
worth seeing in a diff anyway. Summing size instead would move keys on every edit and reorder whole
subjects because one line was added — spending the reviewable diff the document exists for.

Breadth and depth both raise it, deliberately: siblings are distinguished *by* their arrangement, so
at most one child of a node can contribute zero and a subject with n branches carries at least n−1.
The `When` is constant among subjects and cancels, so subject order is decided by the branches.

**A node's own clauses are the hoisted ones** — what is stated at its heading, not what its
requirements carry before hoisting. This is not a free choice: counting pre-hoist would charge every
requirement for the arrangement it inherits, so a subject's number would grow with how many
requirements it has, and the measure would be size again by another route. The tree must therefore be
decomposed before it is ordered, which is why `DocumentRenderer` builds `SubjectNode`/`BranchNode`
and sorts those rather than sorting a flat list of entries.

Alphabetical is a placeholder for something semantic. Any change to the rule reflows every
`SPECIFICATION.md` at once, which makes the ordering strategy closer to a file format than an
implementation detail: it belongs in a major release, not a minor.

## 4. Completeness check

At `SpecificationDocument.Dispose()`, before writing anything:

1. Reflect over the assembly for every participating test method.
2. Subtract those with `Skip` set.
3. Compare that expected set against the set that reported in at `Dispose`.
4. Write only if they match exactly; otherwise leave the file alone and report why.

Equality catches all three failure modes with one rule: a filtered run reports fewer, a failed test
never reports, a constructor-throws test never reports. There is deliberately no separate "was
anything red" flag.

**Staleness is the consumer's gate, not ours.** The document is deterministic, so the check is exact
and already exists:

```bash
dotnet test && git diff --exit-code -- "**/SPECIFICATION.md"
```

This requires a `.gitattributes` normalising line endings (`* text=auto eol=lf`). Without one, a
Windows checkout and a Linux checkout compile different bytes, the build id moves, and the gate fails
on every run for a reason that looks nothing like its cause.

## 5. Hoisting rules

What every requirement under a heading opens with is stated once, under that heading.

1. **Whole clauses only** — never a line, never a fragment.
2. **Shared by every entry under the heading.** Exact match, no partial credit.
3. **No minimum number of entries.** A lone requirement's context still belongs under the heading
   that names it. The rule was *repetition*, the intent is *placement*, and those coincide only when
   groups are large — hoisting is not compression, and at one or two sharers it costs more lines in
   fences than it saves.
4. **Every level the document has**, today document → `##` subject → `###` branch. Not generalised
   to arbitrary depth: it applies to the levels that exist, and a new level would add one.
5. **Assertions never hoist.** They are the claim each requirement exists to make, so two
   requirements agreeing on one is a coincidence worth seeing. With no threshold, this is the only
   thing guaranteeing a requirement block still says something.

A leading `and …` can never be left behind, because lead words are assigned while rendering:
whatever clause a block now opens with is given its family's word.

**Deliberately not built.** Placement is inferred from sharing, which is a proxy — TSpec does not
record which class declared a clause, so a `Having` from a branch constructor and one from a `[Fact]`
are indistinguishable. The successor is a family-to-level ceiling (`When` no higher than its subject,
`Having` no higher than its branch, arrangement anywhere), which encodes the intent instead of
inferring it. It needs a naming convention enforced on test classes. **Trigger to build it:** a
clause appearing above the heading that names it — a `When` at document level, possible today in a
single-subject document.

## 6. Facts that were expensive to establish

**The specification freezes at first observation.** `Specification.Is(…)` reads it from inside a
test and then asserts — and that assertion is itself a recordable step. Without the freeze a test
describes the act of checking its own description. Invisible to the suite; visible only in the
document.

**Compose phrases after describing, never before.** Some assertion phrases used to prepend a word to
the *raw* expression and parse the splice — `"by (it, i) => it + i"`. That is not C#, and the moment
the grammar learned about lambda return types it read `by` as one and silently swallowed it. Parsing
now only ever sees real C#, which retires the whole collision class instead of enumerating it.

**Fences are load-bearing.** The specification contains `<` and `>` (`Result.Read<Room>()`), which a
markdown renderer outside a code fence eats as a tag, and two-space continuation indents that
collapse. This is also why tag names are emphasised by capitalization rather than `**`: markdown
inside a fence is verbatim, and the same text is failure output in a terminal.

**`Tag.Name` never reaches the specification text.** Every tag word in a specification comes from the
captured source expression; `Name` labels a value in a failure report and nothing else. A tag is
named after its variable only in a *field* initializer — `[CallerMemberName]` reports the enclosing
member, which is the method for a local. Names must be unique within a test; a clash throws
`SetupFailed`, which is what catches two locals sharing their method's name.

**A tag says what a constant cannot.** `const RoomNumber = "101"` renders as its name and hides the
value; the value itself would be noise. A tag renders as `the RoomNumber` — the *identity*, which was
the actual claim: the requirement never cared what the room number was, only that it is the same one
throughout. Where a value genuinely matters, write it literally and it renders.

**The class chain follows nesting, not inheritance.** A `BaseType` walk yields `ApiSpec\`1 →
WhenGetVersion → GivenNothing`, because a shared black-box base sits between the test and `Spec`.
Nesting gives what the recommended structure actually expresses; shared bases are scaffolding.

**Mentions built inside `Having` steps come out in creation order.** A numbered mention takes its
value from a shared counter when it is *first requested*, and setups run last-declared-first, so the
setup that runs first both creates its entity first and gets the lowest generated value. Two
consequences. The useful one: declare the setups in reverse and `A<T>` reads as the first-created
entity throughout, which is why `WhenListRooms.GivenTwoRooms` declares its second room first. The
limit: creation order and generated-value order always coincide, so such a requirement cannot
distinguish "in creation order" from "sorted by the generated value". Breaking the tie means building
the entities before the setups, which costs the document its readable setup lines — MyHotel accepts
the coincidence rather than pay that, since no implementation there sorts.

**The document normalises line endings at its own boundary.** `Specification.ToString()` documents
that it returns platform-native endings, so that is not a bug to fix at the source.

## 7. History

**User-facing, in order** — the material for release notes:

| Change | Effect |
|---|---|
| `SpecificationDocument` assembly fixture | Opt-in generation; misconfiguration fails before the first test. |
| Subject resolution | Name by convention, verified against `deps.json`; failure messages state both expectations. |
| Collection + completeness | Only passing tests recorded; document written only when the reported set matches the expected one. |
| `Having` / `Until` keywords | Setup and tear-down render under the name of the pipeline method that produced them (was `After` / `Before`). |
| Noise erased | `await`, `async`, `!`, `?.` no longer appear in specifications. |
| Interpolation holes described | `$"/rooms/{The(x)}"` renders its hole as prose, not as source. |
| Tags name themselves | `[CallerMemberName]` — `nameof(…)` is no longer needed for a tag declared as a field. Names must be unique within a test. |
| Tag names normalized | `_roomNumber` renders as `RoomNumber`. |
| Document layout | Title, provenance in a comment, subject → branch → requirement headings read as prose, shared clauses hoisted to the level that names them. |
| Sections ordered by `ComplexityNumber` | Every level now reads simplest first — subjects, branches and requirements — instead of alphabetically. On MyHotel: version 1, add room 2, get room 2, list rooms 3. Requirements that arrange alike are then ordered by the length of what they claim; anything still tied falls back to the name. |
| Line wrapping keeps expressions whole | A phrase too long for the rest of the line moves to the next line entire, where before it was split at the last break cue that fitted — `tap(…) returns` + `RetVal` is now `tap(…)` + `returns RetVal`. Nineteen expectations re-pinned. |

**Notable internals:** the two-phase engine (§3) was the largest single change and the enabler for
everything after it; `Expr.ToSource()` rebuilds an expression from the tree so erased keywords cannot
return through a parent's raw text; `ExpressionParser` became `ExpressionDescriber` and
`ParseValue`/`ParseCall`/`ParseActual` became `Describe`/`DescribeCall`/`DescribeActual`, since they
return finished prose rather than a tree.

**Two gates earned their keep.** The phase split had to reproduce every pinned expectation
byte-for-byte, and did on the first run. The document rebuild had to produce a diff containing only
the build id — it did not, and that caught the freeze bug above.

## 8. Remaining

1. **Grow MyHotel** — ~~list~~ (done, 13 requirements), then delete, then update (delete is simpler,
   and delete+add covers what update does). Format questions open, all PO calls against a bigger
   document: the scaffolding-to-content ratio, whose remedy is the fences; whether a requirement
   needs both a heading and a claim (vision §11 Q1); and whether an assertion shared by every branch
   of a subject should repeat (§8.8).
2. **404 with a body.** `GivenNoSuchRoom.ThenRespondNotFound` passed before any endpoint existed — an
   unmatched route also returns 404. **An assertion that checks only an absence cannot distinguish
   "not implemented" from "correctly absent."** Assert on something only the implementation can
   produce. One line per handler; do it with step 1.
3. **Opt-out attributes.** `[Specification]` / `[ExcludeFromSpecification]`, nearest declaration
   wins, default include. Deferred because nothing has needed to opt out. Rationale for the polarity:
   the framework cannot detect tests that were never written, so absence never certified coverage —
   an untagged class is just another way to have no test.
4. **Scratch project for the §2 xunit facts.** Fixture wiring, `TestState` at `Dispose` and
   end-of-assembly ordering cannot be self-tested from inside the same assembly. Currently verified
   only by `MyHotel.Spec` passing, which will not catch a regression precisely.
5. **Namespace as a grouping level.** Not collected — with few subjects it would add a heading level
   for nothing. Revisit when MyHotel has several. Now only about *headings*: ordering no longer needs
   it, since §3's rule does not depend on where a class sits.
6. **Finish the docs.** `README.md` §7 and `TSpec-agent-reference.md` are marked work-in-progress,
   and the agent reference's "covers TSpec x.y" line says 1.5 while documenting post-1.5 behaviour.
7. **Version and release decision.** `PackageVersion` and `PackageReleaseNotes` are untouched at
   1.5.0. This branch aims at 2.1.0, but 2.0.0 has not happened. Needs the PO.
8. **A subject-wide assertion repeats under every branch.** `ThenRespondOk` is declared on
   `WhenListRooms` and inherited by both branches, so the document states it twice. §5 rule 5 says
   assertions never hoist, and the reason still holds — two branches *agreeing* on an assertion is a
   coincidence worth seeing. But an assertion *declared above the branches* is not a coincidence; it
   is a claim about the subject, and the document cannot currently tell the two apart. Same missing
   input as §5's "deliberately not built": TSpec does not record which class declared a step.
9. **A second assertion in a test method starts an orphaned sentence.** Only the first gets `Then`;
   the next is capitalized on a new line with no connective, so `Second is new Room(…)` reads as a
   claim about nothing. Pinned deliberately in `WhenTwoItems.cs:24` and `HavingWhenUntil.cs:67`.
   **Examined 2026-07-28 and deferred**, along with two neighbours — rendering the continuation
   lowercase, and breaking the line after `that`. All three need the same thing and none is cheap.
   Assertions carry no `StepFamily`, so the family "and" rule does not reach them; every `and` in an
   assertion today comes from an author-written `.and.` combinator, which cannot be written here
   because destructuring the tuple breaks the chain. Synthesising one means knowing where the group
   ends, and phase 2 streams steps one at a time. Breaking after `that` is eight lines if
   unconditional, but that spoils the sixteen short pins where `… that is the MyModel` fits on one
   line; conditional needs the same lookahead, since `AddAssert` emits head, verb and expected
   separately and `first` alone always fits. Reconsider when phase 2 buffers a whole assertion.

## 9. Release train

| Version | Content |
|---|---|
| **1.6.0** | `TODO.txt` line 1 — fail a test whose pipeline never ran. Minor, not a patch: a green suite going red must not arrive in a patch upgrade. |
| **2.0.0** | Removals only — the three `[Obsolete]` members plus `IVerifyService`/`VerifyService`, per IMPROVEMENT-PLAN.md. |
| **2.1.0** | This work. |

Decoupled deliberately: the generator is purely additive and does not justify a major, and bundling
it would hold the deprecation cleanup hostage.
