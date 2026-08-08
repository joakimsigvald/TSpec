# Specification improvement

Standing log for the shipped generator (2.0.0+). Every observation about a real `SPECIFICATION.md`
lands here, gets classified, and either becomes work or is closed with a reason.

**Scope:** anything that improves the specification's value — what it says, how it reads, and what it
is *able* to say. API and pipeline changes count, since how a test can be written decides what gets
rendered. An item belongs here when the specification is the reason for wanting it, and in
`TODO.txt` when it is not. Laws and the stages past generation stay in
[TSpec-vision.md](TSpec-vision.md).

**State:** one observation open (§4), nineteen done or closed (§5), four queued (§6), one of them
pinned.

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

## 5. Done

Kept as one line each so nothing here is filed again.

- **4.1** A cast of a collection expression was not read as a cast, losing the type and the value.
- **4.3** A cast renders as source — PO's ruling: correct as written, no change wanted.
- **4.4** A nested `Given` heads its own section where the document has depth for it.
- **4.5** A service-wide `Returns` answers any method whose return type the value can be assigned to.
- **4.6** Each assertion statement takes its own line instead of running into the one before it.
- **4.7** Dropped: not reproducible in any shape, and the recollection was of 4.6.
- **4.8** Tried and reverted: hoisting an assertion *clause* every requirement states. PO's ruling
  2026-08-08 — a requirement is the smallest thing that may rise, so one technical assertion made by
  two separate facts stays visible in both. Superseded by 5.3, which was what was wanted.
- **4.9** The return type has no ceiling: it rises as far as it holds, independently of the subject.
  PO's ruling — the argument that it belongs to the method each heading names lost to ten headings
  repeating `, returns HttpResponseMessage`. It stays a `Return type:` label beside the subject
  except where it joins an act, which only a heading naming that act has.
- **5.2** A second assertion no longer starts an orphaned sentence — closed by 4.6.
- **5.3** A requirement every branch repeats is listed once at the heading they share, which is
  where a `[Fact]` on the outer class was written. Decided by repetition, not by the declaring
  class, and no higher than the heading naming the act. `MyHotel.Spec` moves twice.
- **5.8b** An array creation reads as the list it is, keeping the element type where one was written.
- **5.10** A wrapped line never opens with the comma that joined it to the line above.
- **7.1** A test that asserts nothing reads `TODO: Assert behaviour` where its claim would be,
  rather than failing the build — outlining ahead of the requirements stays a legal move.
- **7.2** A raw string literal is described like any other string, holes and all.
- **7.3** A nested `new(…)` was already described; the `TODO.txt` entry was stale.
- **7.4** Closed, PO's ruling 2026-08-08: `Nothing` as an explicit type argument is low value and
  extra friction for the author. Nothing checks that a declared type argument is used, and nothing
  will.
- **5.8a** Closed, PO's ruling 2026-08-08: `one the Model` stays. No proposed form was truer to the
  test code than the one the source already reads as.
- **A target-typed `new(…)` keeps its source form.** Closed: TSpec describes source text and has no
  semantic model, so recovering `DateOnly` from `new(2026, 8, 10)` means reflecting on the enclosing
  call and matching by position — wrong under overloads, and inventing what the test does not say.
- **A `When…` class name is the heading.** Closed: whether `When book` should read `When book a
  room` is a naming choice in the suite, not a rendering rule.

## 6. Carried in from 2.0.0

Numbered as they were in the 2.0.0 working notes, which this section absorbed when that file was
retired.

| # | In one line | Class |
|---|---|---|
| 5.5 | nullable return type renders as non-nullable | lost claim, **candidate for the next release** |
| 4.2b | no `The` form when providing a value | surface, **queued, no work yet** |
| 5.4 | setup order is lost across a hoist boundary | lost claim, unreachable |
| 5.7 | a block opening with `Given` loses that word under a `Given` heading | lost claim, unreachable |
| 5.6 | one outlier costs its sibling group their hoist, no partial credit | reads badly, **pinned** |

**5.5** — a reference type carries no annotation at runtime, and `NullabilityInfoContext` has no
overload for a base type's generic argument, so it means decoding the compiler's `NullableAttribute`
on the spec class: a flag array indexed by position in a flattened type tree, which silently gets
the wrong answer for nested generics. `int?` already works, being `Nullable<int>`. Nothing in either
MyHotel suite returns a nullable type, so no document moves when it lands.

**4.2b** — split from §4.2, whose phrasing half stays held for a second sighting. The arrange surface
offers `A`, `An`, `ASecond`… and no `The`, so an author who dislikes `a ClientRecord has Status =
"disabled"` has no other way to write it. A pure addition; nothing re-pins.

**5.4 and 5.7** are not reachable in any suite we have, which is the argument for intake from a
second application. 5.4: `Having` steps run last-declared-first and consecutive setups render joined
by "after" to say so — hoist one to the heading and leave the other in the item, and nothing relates
them in time. 5.7: the word-drop rule that turns `## When get room` + `When get` into `get` cannot
tell a family keyword from a class-name segment, so it eats the `Given` that marks a block as
arrangement.

**5.6** stays **pinned**. Exact match, no partial credit: one requirement saying something else
costs its whole sibling group the hoist. The open question, were it unpinned, is whether a dissenter
should restate the clause in its own block instead of blocking the hoist for everyone.

## 7. What a fix costs

- **Any change to rendered text** moves consumers' pinned `Specification.Is(…)` expectations and
  reflows every committed `SPECIFICATION.md`.
- **Section order and the ordering tiebreak** are a file format — `ComplexityNumber`,
  arrangement-based, ties on rendered length then name. Changing either reflows every committed
  `SPECIFICATION.md` at once: major version.
- **A pure addition** still moves pins, but a reader upgrading gains rather than re-pins.

**A change to the rendered text ships as a minor** — PO's ruling, 2026-08-07. Most of this list goes
into 2.1.0. Major is reserved for format-level reflows and for breaking the surface.

## 8. Working rules

- **Fix the smallest thing.** Adjust the existing condition before adding state or a pass; diff the
  real output of both before claiming the richer design is needed.
- **Erase mechanism, keep claims.** A change justified only by "reads better" needs the PO; one
  justified semantically does not.
- **One entry, one change**, so a fix the PO dislikes can be reverted alone.
- **Regenerate both documents and read the whole diff** — a change aimed at one line habitually moves
  twenty, and the twenty are the actual proposal.

## 9. What no test guards

Absorbed from the 2.0.0 working notes. Each of these cost a session to find, and none of them fails
a test when broken.

**Hoisting.** Two things rise, and an assertion is neither of them on its own.

*Arrangement*, clause by clause: what every requirement under a heading states is written once at
that heading, whole clauses only, rising as often as the least-frequent entry states it. The act
stops at the subject heading naming the method. Both declared labels rise as far as they hold, the
subject and the return type independently of each other (4.9).

*Requirements*, whole: one that every branch under a heading repeats is listed once at that heading
— the shape a `[Fact]` on the outer class makes, since it runs in every branch below. Capped so no
branch is emptied, and no higher than the subject heading, above which nothing names the act it is
about. A single branch that heads nothing is not a level, so what it holds is held by the node
above it.

Levels: document → area → (group, where an area holds more than one namespace below it) → subject →
branch. Both are decided by repetition, a proxy for placement, since TSpec does not record which
class declared what.

**Decisions.**

- Document and per-test specification render from one text — no second renderer, no unpinned
  requirement.
- Layout runs last, after every text edit (heading-word stripping, fence indent).
- Wrap width: source 80 / document 90, both with a 10-column tolerance before breaking a line.
  Continuation indent: source 3 steps / document 2, relative to the line it continues.
- Break points are recorded as unprintable markers in the text, stripped before any output
  (including failure messages) — a new sink of described text must strip them too.
- Areas come from `Type.Namespace`, not the folder — the run has no file paths.
- `Spec<T>`'s one type argument is inferred as subject/return-type/both from the `When` overload
  used. Nothing checks that a declared type argument is used in that capacity, and per §5 nothing
  will.
- Collection: recorded at `Dispose` keyed on `TestState.Result` (`TestStatus` alone can't tell pass
  from running); the assembly fixture disposes last; only a `Passed` result is recorded; the write
  gate compares "every participating method minus skips" against "what reported in".

**Traps.**

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
