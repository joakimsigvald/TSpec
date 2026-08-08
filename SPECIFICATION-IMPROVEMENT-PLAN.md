# Specification improvement

Standing log for the shipped generator (2.0.0+). Every observation about a real `SPECIFICATION.md`
lands here, gets classified, and either becomes work or is closed with a reason.

**Scope:** anything that improves the specification's value — what it says, how it reads, and what it
is *able* to say. API and pipeline changes count, since how a test can be written decides what gets
rendered. An item belongs here when the specification is the reason for wanting it, and in
`TODO.txt` when it is not. Laws and the stages past generation stay in
[TSpec-vision.md](TSpec-vision.md).

**State:** one observation open (§4), twelve done (§5), six carried in (§6, §7), two of those
pinned or deprioritized.

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
- **5.2** A second assertion no longer starts an orphaned sentence — closed by 4.6.
- **5.8b** An array creation reads as the list it is, keeping the element type where one was written.
- **5.10** A wrapped line never opens with the comma that joined it to the line above.
- **7.1** A test that asserts nothing reads `TODO: Assert behaviour` where its claim would be,
  rather than failing the build — outlining ahead of the requirements stays a legal move.
- **7.2** A raw string literal is described like any other string, holes and all.
- **7.3** A nested `new(…)` was already described; the `TODO.txt` entry was stale.

## 6. Carried in from 2.0.0

By pointer rather than copy — [SPEC-GENERATION-PLAN.md](SPEC-GENERATION-PLAN.md) §5 describes each.

| # | In one line | Example | Class |
|---|---|---|---|
| 5.3 | a `[Fact]` declared above the branches repeats in every branch block | — | noise, **planned** |
| 5.4 | binder silent across a hoist boundary — two `Having`s, nothing relating them | — | lost claim, unreachable |
| 5.5 | nullable return type renders as non-nullable | `Spec<RoomService, Room?>` states `returns Room` | lost claim |
| 5.6 | one outlier costs its sibling group their hoist, no partial credit | — | reads badly, **pinned** |
| 5.7 | a block opening with `Given` loses that word under a `Given` heading | — | lost claim, unreachable |
| 5.8a | a count over an articled expression double-articles | `One(The(model))` → `one the Model` | reads badly, **deprioritized** |

5.5 is harder than it looks: a reference type carries no annotation at runtime, and
`NullabilityInfoContext` has no overload for a base type's generic argument, so it means decoding the
compiler's `NullableAttribute` on the spec class. `int?` already works, being `Nullable<int>`.

5.3 needs the declaring class of the `[Fact]`, not the sharing hoisting infers placement from —
"every branch claims this" is not the same fact as "this was claimed once above them". Planned at
the PO's request. 5.6 stays **pinned**.

No rendering has been agreed for 5.8a: neither `one MyModel` nor `[the Model]` convinced the PO, so
it sits last of the open items.

5.4 and 5.7 are not reachable in any suite we have, which is the argument for intake from a second
application.

Also carried in, unresolved and **the PO's to answer** — SPEC-GENERATION-PLAN §8:

- Should `Core.Spec`'s booking headings (`When book`) match the rooms' fuller style (`When add
  room`)?
- Should `new(2026, 8, 10)` say `new DateOnly(…)` so a reader sees the type?
- Should a refused input stay its own section, or become a branch by making the varying value a tag?
- Is `, returns HttpResponseMessage` on all ten `MyHotel.Spec` headings acceptable? Correct by the
  hoist ceiling, and the least informative case for it.

## 7. Carried in from `TODO.txt`

Moved rather than copied — wanted *for the specification*. The rest of `TODO.txt` stays there:
performance, fixture ergonomics and tooling, none with a path to the page.

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

**A change to the rendered text ships as a minor** — PO's ruling, 2026-08-07. Most of this list goes
into 2.1.0. Major is reserved for format-level reflows and for breaking the surface.

## 9. Working rules

- **Fix the smallest thing.** Adjust the existing condition before adding state or a pass; diff the
  real output of both before claiming the richer design is needed.
- **Erase mechanism, keep claims.** A change justified only by "reads better" needs the PO; one
  justified semantically does not.
- **One entry, one change**, so a fix the PO dislikes can be reverted alone.
- **Regenerate both documents and read the whole diff** — a change aimed at one line habitually moves
  twenty, and the twenty are the actual proposal.
