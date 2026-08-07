# Specification improvement

Standing log for the **shipped** generator (2.0.0+). Every observation about a real
`SPECIFICATION.md` lands here, gets classified, and either becomes work or is closed with a reason.
Unlike [SPEC-GENERATION-PLAN.md](SPEC-GENERATION-PLAN.md), this file does not die — it is how the
document's shape keeps improving after the feature stopped being a feature.

**Scope:** anything that improves the specification's value — what it says, how it reads, and what
it is *able* to say. API and pipeline changes count, because how a test can be written decides what
gets rendered. The test is why an item is wanted, not which layer it lands in: it belongs here when
the specification is the reason, and in `TODO.txt` when it is not. Laws and the stages past
generation stay in [TSpec-vision.md](TSpec-vision.md).

**State:** open for intake. Six observations (§4) — §4.5 shipped in 2.1.0, §4.3 closed, §4.6 next.
Nine backlog items carried in at §5 and §6.

## 1. Where the feedback comes from

| Source | What it is good for |
|---|---|
| `SampleProjects/MyHotel/Core.Spec/SPECIFICATION.md` | domain rules, mocked collaborators, refusals |
| `SampleProjects/MyHotel/MyHotel.Spec/SPECIFICATION.md` | black-box HTTP, one return type throughout |
| Outside suites | the only source that can show a shape MyHotel cannot reach |

Record the source on every entry. **A wart seen in two suites outranks one seen in one** — a single
sighting inside MyHotel may be saying more about MyHotel than about TSpec, and the standing risk in
a dogfooded tool is tuning the renderer to one application's prose.

Intake is reading, not running. The document is read whole, as a reader who did not write the tests
would read it — the diff is the release gate, but the diff never shows a sentence that has been
wrong since it was first generated.

## 2. Filing an entry

Six lines, in §4. Anything longer is a design note and belongs in its own section.

```
### <short name>  — <class>, <source>, seen <date>
Rendered:  `<the line, verbatim from the document>`
From:      <the test expression that produced it>
Jarred:    <what a reader takes from it that the test does not say, or why it stalls the read>
Wanted:    <the line it should have been — or "unknown", which is a legitimate answer>
Status:    open | proposed | decided <date> | closed <reason>
```

Verbatim matters. A paraphrase loses the wrapping, the article, and the backtick context, and those
are usually the whole finding.

## 3. Classes, in the order they get acted on

1. **Untrue** — the document states something the test does not. Fix immediately, at any version
   cost; this is the one class that damages the thesis rather than the prose.
2. **Lost claim** — erasure dropped something that changes *what is claimed*, not merely how it is
   said. Judge semantically, per the `specification-erasure-principle`: `int?` stays because it
   admits values `int` does not.
3. **Noise** — text carrying no claim (mechanism, ceremony, framework vocabulary). Erasure
   candidate. Cheap to fix, and the class most likely to be a real improvement rather than a taste
   swap.
4. **Churn** — anything reaching the page that varies between runs, machines, or checkouts. Rare
   now, but it voids diffability outright, so it outranks everything below it.
5. **Reads badly** — the claim is right and the sentence is poor. The PO decides; a proposal needs
   the rendered before and after, and the count of lines the change moves across both documents.
6. **By design** — the shape follows from a rule (one `When` per class forcing sibling sections;
   assertions never hoisting). **Still record it**, closed, with the rule named. An unrecorded
   by-design finding gets rediscovered every few months and re-argued from scratch.

7. **Surface** — the text is shaped by what the author was able to write, and the fix is in TSpec's
   API rather than in the renderer. In scope per §1, priced differently: it moves the surface as
   well as the output.

Class 5 and 6 findings are not fixed on one sighting. Classes 1–4 are. Class 7 is not a severity —
an item can be both, and §4.2 is.

## 4. Observations

### 4.1 Interface name chopped into words — lost claim, outside suite (`TokenIssuer`), seen 2026-08-07

```
Rendered:  `and ISigningKeyStore returns i read only list SigningKeyRecord`
From:      .And<ISigningKeyStore>().Returns(() => (IReadOnlyList<SigningKeyRecord>)[Example.ServerKey])
Jarred:    `IReadOnlyList` is normalized as an identifier — leading `I` split off as its own word,
           PascalCase lowercased into "i read only list". It reads as prose but names no type, and
           the stray "i" reads as a word the type does not contain. Sibling rows in the same block
           keep their names (`ISigningKeyStore`, `IClientRegistry`, `ClientRecord?`), and the
           generic argument `SigningKeyRecord` survives intact — only the outer name is split.
Wanted:    unknown; at minimum the type recoverable from the text.
Status:    open
```

Filed as lost claim rather than reads-badly: a reader cannot map the sentence back to a type, so
what the store returns is no longer stated. Provisional — reclassify if the PO reads it as taste.
Whole rendered block and source: this session's transcript. Sharpened by §4.3's ruling: a cast
expression rendering verbatim is correct, so this cast being split into words is the odd one out
rather than one of two competing treatments. **Do not fix the renderer before re-reading the
suite:** §4.5 is built, and `Returns(One(Example.ServerKey))` now removes the cast this entry is
about, most likely taking the mangled sentence with it.

### 4.2 `a X has Y = …` reads as Braavosi — reads badly, outside suite (`TokenIssuer`), seen 2026-08-07

```
Rendered:  `a ClientRecord has Status = "disabled"`
From:      Given().A<ClientRecord>(_ => _ with { Status = "disabled" })
Jarred:    indefinite article + type + `has` is the Braavosi construction ("a man has no name") —
           grammatical, right register for nobody. The PO reports it as an annoyance, not a defect:
           the claim itself is correct and complete.
Wanted:    unknown. Reported alongside: there is no corresponding `The`-form to choose at this
           position, so an author who dislikes the sentence has no other way to say it.
Status:    open, held for a second sighting per §3
```

The second half straddles the scope line: the reading is a rendering matter, but "no `The`-form to
choose" is a surface gap, and §3's hold-for-a-second-sighting rule was written for taste, not for a
form that does not exist. Both halves kept in one entry until the PO splits them — recorded as
reported, unverified against the current API.

### 4.3 A cast expression renders as source — closed 2026-08-07, correct as rendered

`Using (ClientRecord?)null`, from `Using((ClientRecord?)null)`. Filed as reads-badly, withdrawn the
same day: the PO's ruling is that the rendering is right and no change is wanted. One line kept
rather than deleted, per §3's last class — so it is not re-filed from a fresh reading in six months.

### 4.4 Nested givens flatten, so a shared clause has nowhere to hoist — noise, outside suite (`TokenIssuer`), seen 2026-08-07

```
Rendered:  ### Given resource and condition, given a matching per type scope

           Token is Example.AccessToken(scope: "system/Condition.rs")
             and Permission is "r"
             and ResourceType is "Condition"
From:      a shared branch subclass ("given resource and condition") holding Permission and
           ResourceType, with a subclass per scope shape below it.
Jarred:    the author factored the shared clauses into an intermediate class so they would be
           stated once. The document flattens the whole branch path into one comma-joined
           heading, which leaves no level for them to hoist to — so every sibling repeats them,
           and the structure the author built is invisible in the output.
Wanted:    PO's proposal, recorded not adopted: render the nesting as nested headings —
           `### Given resource and condition` / `#### Given a matching per type scope` — and
           allow it as long as it does not push headings to level 5.
Status:    open
```

Not a defect: this is the recorded behaviour. A branch path reads as one comma-joined sentence
(SPEC-GENERATION-PLAN §6, commit 6b7fe25 "merge nested givens into one sentence"), and §4 there
lists exactly one branch level. So the entry is a request to revisit that decision, and the case for
revisiting is that the flattening also costs the hoist — which is what §4.1 of that plan says
hoisting is for. Related but distinct from its §5.3, where the assertion has a level to rise to and
does not.

Priced per §7: heading structure is format-level. Every committed document reflows at once.

### 4.5 Mock return values match by exact type, forcing casts into the text — surface, outside suite (`TokenIssuer`), seen 2026-08-07

```
Rendered:  `and ISigningKeyStore returns i read only list SigningKeyRecord`  (§4.1's line)
From:      .And<ISigningKeyStore>().Returns(() => (IReadOnlyList<SigningKeyRecord>)[Example.ServerKey])
Jarred:    the cast is ceremony the author would not write if the setup took an assignable value,
           and ceremony in the source becomes text in the document.
Wanted:    PO's rule — where an interface method returns `ICollection<int>` and the setup supplies
           an `int[]`, the array is what the mock returns when the method is invoked. A value
           supplied for the more specific type still wins where one was also provided.
Status:    built 2026-08-07, unreleased
```

**What shipped.** A service-level `Given<TService>().Returns(value)` now also answers methods whose
return type the value is assignable to. Resolution lives in `FluentDefaultProvider`, which Moq
consults only after its own exact-type lookup misses — so "a more specific value wins" needs no code
of its own, since an exactly-typed value never reaches the assignable path. Among two assignable
candidates the more specific is used; a genuine tie throws `SetupFailed` naming both, rather than
picking one. Matching is on the *declared* type, so `null` is a value like any other: `Returns(() =>
(Cart?)null)` makes null the default wherever a `Cart?` fits.

Costs nothing in rendered text — both MyHotel documents regenerate byte-identical apart from the
build id — so this one does **not** wait on §7's version question. New functionality, minor version.

**It dissolves §4.1's expression.** The cast was there to satisfy exact-type matching, and
`Returns(One(Example.ServerKey))` now serves a member returning `IReadOnlyList<SigningKeyRecord>` on
its own — no cast, no `new[] { … }`, and it renders as "one SigningKeyRecord" rather than as source.
§4.1 stays open only until the auth suite is re-read that way; if the mangled sentence is gone with
the cast, it closes as overtaken rather than fixed.

### 4.6 A second assertion runs into the first, on one line — untrue-adjacent, reproduced 2026-08-07

```
Rendered:  Given IMyRepository throws NotFound
           When GetModel()
           Then throws NotFound IMyRepository.GetModel()
From:      When(_ => _.GetModel())
               .Given<IMyRepository>().Throws<NotFound>()
               .Then().Throws<NotFound>();
           Then<IMyRepository>(_ => _.GetModel());
Jarred:    two claims, no boundary between them — no line break, no binder, nothing telling a
           reader where the first ends. It reads as one malformed claim about NotFound rather than
           as "it throws" plus "it called the repository".
Wanted:    what a chained assertion already produces — `Then throws NotFound` / `  and
           IMyRepository.GetModel()`. PO's ruling wanted on the binder before the fix lands.
Status:    open, next
```

Reproduced from scratch here, not sighted in a document, after the PO asked whether it had been
recorded — it had not. SPEC-GENERATION-PLAN §5.2 covers the family (a second assertion in one
requirement) but describes a milder symptom, an orphaned sentence that at least starts a line. This
is the same cause with the line break missing too, so §5.2's phrasing understates it.

Note what does *not* exhibit it: `Then<IOrderService>(wasInvoked: Once).And<IOrderService>(…)`
renders the second claim on its own line under `and`. The break is there for a chained assertion and
missing for a second statement — so the text is right in one path and not the other.

## 5. Carried in from 2.0.0

Open at ship, by pointer rather than copy — [SPEC-GENERATION-PLAN.md](SPEC-GENERATION-PLAN.md) §5 is
still the description of each. Listed here so intake does not re-file them as new.

| # | In one line | Class | Blocked on |
|---|---|---|---|
| 5.2 | second assertion in a requirement starts an orphaned sentence | untrue-adjacent | phase-2 buffering |
| 5.3 | subject-wide assertion repeats in every branch instead of hoisting | noise | "who declared this" input |
| 5.4 | binder silent across a hoist boundary — two `Having`s, nothing relating them | lost claim | not reachable in MyHotel |
| 5.5 | nullable return type renders as non-nullable (`Room?` → `Room`) | lost claim | `NullabilityInfoContext`, not exhibited |
| 5.6 | one outlier costs its sibling group their hoist, no partial credit | reads badly | a real sighting |
| 5.7 | a block opening with `Given` loses that word under a `Given` heading | lost claim | not reachable |
| 5.8 | `One(expr)` double-articles; `new[] { … }` keeps raw C# | reads badly | — |
| 5.10 | a wrapped trailing phrase can start its line with its binder's comma | reads badly | fix belongs in composition |

Half of these are waiting on a shape MyHotel does not have. That is the argument for intake from a
second application, and the reason §1 asks for the source.

Also carried in, unresolved and **the PO's to answer** — SPEC-GENERATION-PLAN §8:

- Should `Core.Spec`'s booking headings (`When book`) match the rooms' fuller style (`When add
  room`)?
- Should `new(2026, 8, 10)` say `new DateOnly(…)` so a reader sees the type?
- Should a refused input stay its own section, or become a branch by making the varying value a tag?
- Is `, returns HttpResponseMessage` on all ten `MyHotel.Spec` headings acceptable? Correct by the
  hoist ceiling, and the least informative case for it.

## 6. Carried in from `TODO.txt`

Moved here rather than copied — these four are wanted *for the specification*, which is what §1 uses
to decide where an item lives. The rest of `TODO.txt` stays where it is: performance, fixture
ergonomics and tooling, none with a path to the page.

### 6.1 A test that asserted nothing renders as a requirement that claims nothing

Deferred execution means `[Fact] void T() => When(_ => Act());` passes without running the act at
all — green, counted as coverage, verifying nothing. It is then collected like any passing test, so
the document gains an entry that states no claim. **A false entry is worse than a missing one**, and
no other test framework can catch this, because TSpec is the one that knows what an assertion is.

- Runtime: fail in `SpecFixture` teardown when no assertion was recorded. ~20 lines. Check the suite
  does not trip it — `Specification.Is` and `Then<TService>(…)` must count as assertions.
- Compile time: a Roslyn analyzer warning on a `[Fact]` in a `Spec` subclass reaching no assertion.
  Ships alongside, generates no code, suppressible. Only if the runtime check earns its keep first.

### 6.2 Raw string literals render as source, delimiters included

The tokenizer does not handle quote runs, so any `"""…"""` expression falls out as Unknown and
renders verbatim: `$"""{The(y)}"""` stays exactly that. **The last place a specification still shows
source code inside quotes** — ordinary interpolation holes have been described since 2026-07-28.

Needs quote-run delimiters in the tokenizer, then the hole rule keyed on the `$`-run: `$$` means two
braces open a hole and a single `{` is literal, the inverse of the `{{`-escape rule for a single
`$`. Low priority on its own — a raw string inside a captured expression is rare, and a multi-line
one is collapsed by `ToSingleLine` before parsing anyway, which breaks its delimiters regardless.

### 6.3 A nested `new(…)` in an argument list is not described

`When(_ => _.AddNewItem(A<CartId>(), new(A<Sku>(), A<ProductType>(), A<Price>(), A<Vat>()),
A<string>()))` — the inner target-typed `new` does not get described. Filed under "Documentation" in
`TODO.txt` from the start; same family as SPEC-GENERATION-PLAN §5.8's `new[] { … }`, and the two
should be judged together.

### 6.4 `Nothing` as an explicit type argument

Today a type argument reaches the document only where the act uses it in that capacity — an act
taking no subject states none, an act yielding no result states no return type. That covers the
common cases without ceremony, but **nothing checks it**, and `Spec<T>` still has to mean "T in
whichever capacity is used". The document's `Subject under test:` and `returns X` lines rest on an
inferred rule.

Explicit form: `Spec<RoomService, Nothing>`, `Spec<Nothing, string>`, or `Spec<Nothing>` for
neither. Declaring `Nothing` and then using that capacity throws `SetupFailed`, so the declaration
is enforced rather than merely honoured.

Breaking: the non-generic `Spec` becomes `Spec<Nothing, Nothing>` instead of `Spec<object, object>`,
which is what it has always meant. `TODO.txt` said "fits 2.0.0, do not slip it into a minor" — 2.0.0
shipped without it, so read that as 3.0.0 now.

## 7. What a fix costs

Priced before it is proposed, not after:

- **Any change to rendered text** moves consumers' pinned `Specification.Is(…)` expectations and
  reflows every committed `SPECIFICATION.md` in the world. 2.0.0's table of moved pins (§6 there) is
  what that looks like at scale.
- **Section order and the ordering tiebreak** are a file format. Changing either reflows every
  document at once — major version, per SPEC-GENERATION-PLAN §2.
- **A pure addition** (a claim now stated that was silently dropped) still moves pins, but a reader
  upgrading gains rather than re-pins.

Open, and wanted before the first fix ships: **does a text change alone justify a minor, or does it
need a major?** `CLAUDE.md` prices docs-only as patch and new functionality as minor, and says
nothing about the rendered text, which is the one thing this file will keep changing. The lean is
minor with the moved pins tabled in the release notes, reserving major for format-level reflows —
but this is the PO's call and every entry in §4 waits on it.

## 8. Working rules

- **Fix the smallest thing.** Adjust the existing condition before adding state or a new pass; diff
  the real output of both before claiming the richer design is needed.
- **Erase mechanism, keep claims.** The suite cannot judge readability, so a change justified only
  by "reads better" needs the PO, and a change justified semantically does not.
- **One entry, one change.** A fix touching three §4 entries at once cannot be reverted when one of
  them turns out to have been the PO's preference.
- **Regenerate both documents and read the whole diff** — a change aimed at one line habitually
  moves twenty, and the twenty are the actual proposal.
