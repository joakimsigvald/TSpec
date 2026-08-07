# Specification improvement

Standing log for the shipped generator (2.0.0+). Every observation about a real `SPECIFICATION.md`
lands here, gets classified, and either becomes work or is closed with a reason.

**Scope:** anything that improves the specification's value — what it says, how it reads, and what it
is *able* to say. API and pipeline changes count, since how a test can be written decides what gets
rendered. An item belongs here when the specification is the reason for wanting it, and in
`TODO.txt` when it is not. Laws and the stages past generation stay in
[TSpec-vision.md](TSpec-vision.md).

**State:** three observations open (§4), five done (§5), eleven carried in (§6, §7).

## 1. Where the feedback comes from

| Source | What it is good for |
|---|---|
| `SampleProjects/MyHotel/Core.Spec/SPECIFICATION.md` | domain rules, mocked collaborators, refusals |
| `SampleProjects/MyHotel/MyHotel.Spec/SPECIFICATION.md` | black-box HTTP, one return type throughout |
| Outside suites | the only source that can show a shape MyHotel cannot reach |

Record the source on every entry. **A wart seen in two suites outranks one seen in one** — the
standing risk in a dogfooded tool is tuning the renderer to one application's prose.

Intake is reading, not running: the diff is the release gate, but it never shows a sentence that has
been wrong since it was first generated.

## 2. Filing an entry

Six lines, in §4. Anything longer belongs in its own section.

```
### <short name>  — <class>, <source>, seen <date>
Rendered:  `<the line, verbatim from the document>`
From:      <the test expression that produced it>
Jarred:    <what a reader takes from it that the test does not say, or why it stalls the read>
Wanted:    <the line it should have been — or "unknown", which is a legitimate answer>
Status:    open | proposed | decided <date> | closed <reason>
```

Verbatim matters. A paraphrase loses the wrapping, the article and the backtick context, and those
are usually the whole finding.

## 3. Classes, in the order they get acted on

1. **Untrue** — the document states something the test does not. The one class that damages the
   thesis rather than the prose; fix at any version cost.
2. **Lost claim** — erasure dropped something that changes what is claimed. Judge semantically, per
   the `specification-erasure-principle`: `int?` stays because it admits values `int` does not.
3. **Noise** — text carrying no claim. Erasure candidate, and the class most likely to be a real
   improvement rather than a taste swap.
4. **Churn** — anything on the page that varies between runs, machines or checkouts. It voids
   diffability outright.
5. **Reads badly** — the claim is right and the sentence is poor. The PO decides, on a before and
   after plus the count of lines it moves.
6. **By design** — the shape follows from a rule. Still recorded, closed, with the rule named, so it
   is not rediscovered and re-argued.
7. **Surface** — the text is shaped by what the author could write, and the fix is in the API.

Classes 1–4 are fixed on one sighting; 5 and 6 wait for a second. Class 7 is not a severity — an item
can be both, and §4.2 is.

## 4. Open observations

### 4.2 `a X has Y = …` reads as Braavosi — reads badly, outside suite (`TokenIssuer`), seen 2026-08-07

```
Rendered:  `a ClientRecord has Status = "disabled"`
From:      Given().A<ClientRecord>(_ => _ with { Status = "disabled" })
Jarred:    indefinite article + type + `has` is the Braavosi construction ("a man has no name") —
           grammatical, right register for nobody. Reported as an annoyance, not a defect.
Wanted:    unknown. Reported alongside: there is no corresponding `The`-form to choose at this
           position, so an author who dislikes the sentence has no other way to say it.
Status:    open, held for a second sighting per §3
```

The `The`-form half is a surface gap, not taste, so the hold rule does not apply to it. Split the
entry if the PO wants that half moved.

### 4.4 Nested givens flatten, so a shared clause has nowhere to hoist — noise, outside suite (`TokenIssuer`), seen 2026-08-07

```
Rendered:  ### Given resource and condition, given a matching per type scope

           Token is Example.AccessToken(scope: "system/Condition.rs")
             and Permission is "r"
             and ResourceType is "Condition"
From:      a shared branch subclass ("given resource and condition") holding Permission and
           ResourceType, with a subclass per scope shape below it.
Jarred:    the author factored the shared clauses into an intermediate class so they would be
           stated once. The document flattens the branch path into one comma-joined heading,
           leaving no level for them to hoist to, so every sibling repeats them.
Wanted:    PO's proposal, recorded not adopted: nested headings — `### Given resource and
           condition` / `#### Given a matching per type scope` — as long as no heading reaches
           level 5.
Status:    open
```

Not a defect but a request to revisit a recorded decision (SPEC-GENERATION-PLAN §6, commit 6b7fe25).
Heading structure is format-level per §8: every committed document reflows at once.

### 4.7 A default `Throws` and a method setup merged into one line — not reproduced, outside suite (`TokenIssuer`), reported 2026-08-07

```
Rendered:  "Given IMyService throws some exception that some method returns some value"
           (PO's recollection — the real line is still wanted)
From:      Given<IMyService>().Throws(someException);
           Given<IMyService>().That(_ => _.SomeMethod).Returns(someValue);
Jarred:    two arrangement clauses with no boundary, joined by a stray `that`.
Wanted:    the two clauses on their own lines, as the per-test text already renders them.
Status:    open, cannot reproduce
```

Tried and correct: `Throws<TEx>()` or `Throws(A<TEx>)` before `That`, `That` before either, a
property setup rather than a method, and chained `.And<T>()` versus two statements. Two threads left:
it was seen **in `SPECIFICATION.md`**, and all of the above was checked in the per-test text; and a
stray `that` is what an exception-property continuation produces, which a `Given` never does — so the
recalled source may not be the source of the recalled line.

## 5. Done

Kept as one line each so nothing here is filed again.

- **4.1** Interface name chopped into words — a cast of a collection expression was not read as a
  cast, so the type became words and the operand was dropped. Fixed 2026-08-07.
- **4.3** A cast expression renders as source — PO's ruling: correct as rendered, no change wanted.
- **4.5** Mock return values matched by exact type — a service-wide `Returns` now answers any method
  whose return type the value is assignable to. Built 2026-08-07.
- **4.6** A second assertion ran into the first — the recording files each step under the statement
  it belongs to, so every claim takes its own line. Fixed 2026-08-07.
- **5.2** (from 2.0.0) A second assertion started an orphaned sentence — closed by 4.6's fix.

## 6. Carried in from 2.0.0

By pointer rather than copy — [SPEC-GENERATION-PLAN.md](SPEC-GENERATION-PLAN.md) §5 describes each.

| # | In one line | Class | Blocked on |
|---|---|---|---|
| 5.3 | subject-wide assertion repeats in every branch instead of hoisting | noise | "who declared this" input |
| 5.4 | binder silent across a hoist boundary — two `Having`s, nothing relating them | lost claim | not reachable in MyHotel |
| 5.5 | nullable return type renders as non-nullable (`Room?` → `Room`) | lost claim | `NullabilityInfoContext`, not exhibited |
| 5.6 | one outlier costs its sibling group their hoist, no partial credit | reads badly | a real sighting |
| 5.7 | a block opening with `Given` loses that word under a `Given` heading | lost claim | not reachable |
| 5.8 | `One(expr)` double-articles; `new[] { … }` keeps raw C# | reads badly | — |
| 5.10 | a wrapped trailing phrase can start its line with its binder's comma | reads badly | fix belongs in composition |

Half are waiting on a shape MyHotel does not have, which is the argument for intake from a second
application.

Also carried in, unresolved and **the PO's to answer** — SPEC-GENERATION-PLAN §8:

- Should `Core.Spec`'s booking headings (`When book`) match the rooms' fuller style (`When add
  room`)?
- Should `new(2026, 8, 10)` say `new DateOnly(…)` so a reader sees the type?
- Should a refused input stay its own section, or become a branch by making the varying value a tag?
- Is `, returns HttpResponseMessage` on all ten `MyHotel.Spec` headings acceptable? Correct by the
  hoist ceiling, and the least informative case for it.

## 7. Carried in from `TODO.txt`

Moved rather than copied — these four are wanted *for the specification*. The rest of `TODO.txt`
stays there: performance, fixture ergonomics and tooling, none with a path to the page.

### 7.1 A test that asserted nothing renders as a requirement that claims nothing

Deferred execution means `[Fact] void T() => When(_ => Act());` passes without running the act, and
is then collected like any passing test. **A false entry is worse than a missing one**, and TSpec is
the only framework that can catch it, because it knows what an assertion is.

- Runtime: fail in `SpecFixture` teardown when no assertion was recorded. ~20 lines. Check the suite
  does not trip it — `Specification.Is` and `Then<TService>(…)` must count as assertions.
- Compile time: a Roslyn analyzer warning, only if the runtime check earns its keep first.

### 7.2 Raw string literals render as source, delimiters included

The tokenizer does not handle quote runs, so `$"""{The(y)}"""` falls out as Unknown and prints
verbatim — **the last place a specification shows source code inside quotes**. Needs quote-run
delimiters, then the hole rule keyed on the `$`-run: `$$` means two braces open a hole and a single
`{` is literal. Low priority: rare, and a multi-line one is collapsed before parsing anyway.

### 7.3 A nested `new(…)` in an argument list is not described

`_.AddNewItem(A<CartId>(), new(A<Sku>(), A<Price>()), A<string>())` — the inner target-typed `new` is
not described. Same family as §6's 5.8, and the two should be judged together.

### 7.4 `Nothing` as an explicit type argument

A type argument reaches the document only where the act uses it in that capacity, which covers the
common cases without ceremony but **nothing checks it**. Explicit form: `Spec<RoomService, Nothing>`,
`Spec<Nothing, string>`, `Spec<Nothing>`; declaring `Nothing` and then using that capacity throws
`SetupFailed`. Breaking — the non-generic `Spec` becomes `Spec<Nothing, Nothing>` — so 3.0.0.

## 8. What a fix costs

- **Any change to rendered text** moves consumers' pinned `Specification.Is(…)` expectations and
  reflows every committed `SPECIFICATION.md`.
- **Section order and the ordering tiebreak** are a file format: changing either is a major version,
  per SPEC-GENERATION-PLAN §2.
- **A pure addition** still moves pins, but a reader upgrading gains rather than re-pins.

Open, and wanted before the first text change ships: **does a text change alone justify a minor, or
does it need a major?** `CLAUDE.md` prices docs-only as patch and new functionality as minor, and
says nothing about rendered text — the one thing this file will keep changing. The lean is minor with
the moved pins tabled in the release notes, major reserved for format-level reflows.

## 9. Working rules

- **Fix the smallest thing.** Adjust the existing condition before adding state or a pass; diff the
  real output of both before claiming the richer design is needed.
- **Erase mechanism, keep claims.** A change justified only by "reads better" needs the PO; one
  justified semantically does not.
- **One entry, one change**, so a fix the PO dislikes can be reverted alone.
- **Regenerate both documents and read the whole diff** — a change aimed at one line habitually moves
  twenty, and the twenty are the actual proposal.
