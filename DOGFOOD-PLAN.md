# Dogfood TSpec

Standing plan for making `Core.Test` a suite TSpec can read. Analysis taken 2026-08-09 against
`main` at 191c2bc, on a clean tree.

**Scope:** `Core.Test`, and `SampleProjects/MyHotel/{Core.Spec, MyHotel.Spec}` as the control group.
Observations about what a generated `SPECIFICATION.md` *says* belong in
[SPECIFICATION-IMPROVEMENT-PLAN.md](SPECIFICATION-IMPROVEMENT-PLAN.md); this document is about how
the tests are *written*. Stages past generation stay in [TSpec-vision.md](TSpec-vision.md).

**Method:** measured against TSpec's own documented conventions — README §5.6, §6.1, §6.2 — not
against a count of foreign API calls. Those two questions give very different answers, and §6 lists
five items a foreign-API census flagged that turned out to be nothing.

---

## 1. The finding, in one paragraph

`Core.Test` is not a bad xUnit suite. It is a good xUnit suite that TSpec cannot read, and the
single reason is not the 273 `Xunit.Assert` calls — it is that **468 of its 1199 test methods carry
two logical assertions: what the code does, and what the specification text says about it.** That
second assertion is what forces 663 statement-block bodies where MyHotel has none, what makes a
rendering change re-pin tests across the suite, and what has quietly made the renderer's only
end-to-end coverage a passenger on behaviour tests. Meanwhile `Core.Test` itself generates no
`SPECIFICATION.md`, so the one artifact that would make those 468 pins largely unnecessary does not
exist.

---

## 2. The two suites, measured

| | `Core.Test` | `Core.Spec` | `MyHotel.Spec` |
|---|---|---|---|
| Test files | 302 | 12 | 10 |
| `[Fact]` / `[Theory]` | 1041 / 158 | 32 / 0 | 50 / 0 |
| Statement-block test bodies | **663** | **0** | **0** |
| `Specification.Is(…)` pins | **472** | 0 | 0 |
| `Xunit.Assert.*` | 273 | 0 | 0 |
| `because:` | 8 | 0 | 0 |
| Files with no `When` | 142 | 0 | 0 |
| Generates `SPECIFICATION.md` | **no** | yes | yes |

MyHotel is the model and shows the conventions are livable: 82 facts, every one arrow-bodied, every
one a single logical assertion, `When…`/`Given…` nesting throughout. It is also small and tame — one
subject, one return type — which is exactly why `Core.Test` matters as a second source. Per
`SPECIFICATION-IMPROVEMENT-PLAN` §1, *a wart seen in two suites outranks one seen in one*, and today
there is effectively one.

Breakdown of the 1199 test methods:

| Shape | Count |
|---|---|
| Behaviour assertion **and** `Specification.Is` | **468** |
| Behaviour assertion only | 721 |
| `Specification.Is` alone | 3 |
| No assertion detected | 7 |

---

## 3. Findings, ranked

### F1 — TSpec does not specify itself

`Core.Test` has no `[assembly: AssemblyFixture(typeof(SpecificationDocument))]`. The naming rule in
README §6.2 already holds — `TSpec.Test` → `TSpec`, referenced directly, resolving to 2.2.0 in
`deps.json` — so this is one line away.

Everything else in this plan is judged against the document it produces, so it goes first even
though the output will be bad. **Risk:** the completeness gate requires every non-skipped fact on
every concrete `Spec` subclass to pass before the file is written; over 1041 facts on three target
frameworks that is far tighter than over 82.

### F2 — 468 test methods assert twice

The dominant shape:

```csharp
[Fact]
public void ThenArrayHasTwoElements()
{
    Then().Result.Has().Count(2);
    Specification.Is(
@"Given two MyModels
  and IMyRepository.List() returns a MyModel[]
When List()
Then Result has count 2");
}
```

Four costs, in descending order of how much they matter:

1. **The renderer's end-to-end coverage is a passenger.** Only 3 of 1199 test methods assert the
   rendered text *alone*. The other 468 are behaviour tests that happen to also check rendering, so
   the renderer has component tests (`ExpressionDescriber`, `TextBuilder`, `DocumentRenderer`) and
   no suite of its own above them.
2. **A rendering change re-pins unrelated tests.** `SPECIFICATION-IMPROVEMENT-PLAN` §5 records "19
   lines re-pinned in `Core.Test`" for one article change — a recurring, already-observed tax.
3. **Two logical assertions per method**, against README §5.6 and §6.1.5.
4. **663 statement blocks**, against §6.1.6, essentially all of them caused by needing a second
   statement.

**What would replace them — and what would not.** Turning on F1 gives a committed, deterministic
document plus the CI check README §6.2 already prescribes:

```bash
dotnet test && git diff --exit-code -- "**/SPECIFICATION.md"
```

But the document is **not** a verbatim substitute, and the plan must not pretend otherwise. It
hoists clauses shared by sibling branches into headings, drops the `Then` keyword, and
**de-duplicates requirements with identical rendered signatures** — so two tests that render alike
collapse to one bullet, and a leaf's exact text is not always shown. The document covers *what each
requirement claims*; the pins additionally cover *the exact per-test rendering*.

So the question for the PO is not "delete the pins" but **where the renderer's end-to-end coverage
should live**. Three options, in §7.

### F3 — The reflexive core: 225 sites that test TSpec failing

| Shape | Sites | Status |
|---|---|---|
| `Xunit.Assert.Throws<XunitException>` + `HasMessage` / `ex.Message` | 179 | see below |
| `Xunit.Assert.Throws<SetupFailed>` | 46 | **unreachable by design** |
| `Xunit.Assert.Throws<ValuesExhausted>` and one-offs | 14 | mostly fluent-call-time |

A failing assertion *can* already be the act — verified by running it:

```csharp
When(_ => arr.Has().Count(2)).Then().Throws<XunitException>()
    .that.Message.Is("Expected arr to have count 2 but found 1: [1]");
```

That passes. Two things stop it being a general answer. It renders wrong — the inner assertion's own
recording leaks into the enclosing specification as an orphan line and the `Then throws …` line goes
missing (**G1**). And it needs the act slot: **8 of the 19 files** holding message-only probes
already bind `When`, often in a shared base, so the failing assertion cannot become the act there at
all.

`SetupFailed` is excluded from capture on purpose — [Pipeline.cs:170](Core/Internal/Pipelines/Pipeline.cs:170),
`catch (Exception ex) when (ex is not SetupFailed)` — so a misconfigured test fails as misconfigured
rather than passing as an expected throw. That rule is right and should not be traded away; it puts
46 sites permanently outside TSpec's vocabulary.

Also worth naming: `HasMessage` (125 sites) and `HasAssignments` (4) are `internal` extension methods
in `Core/Assert/AssertionExtensions.cs`. **The shipped package carries test-only scaffolding**,
reachable through `InternalsVisibleTo("TSpec.Test")`, because there is no public way to say "this
assertion fails, saying this".

### F4 — Small residue

34 plain `Xunit.Assert.Equal/Same/True/False/StartsWith/Contains` calls across 7 files, and 1 direct
`Mock.Get(…).Verify(…)`. All expressible today except the 4 `Same` calls, which need **G2**.

---

## 4. Gaps in TSpec

### G1 — A failing assertion as the act records the wrong thing

Not a missing vocabulary — a rendering defect. Observed:

```
When arr.Has().Count(2)
Arr has count 2          ← the inner assertion's recording, orphaned under no Then
                         ← "Then throws XunitException" missing entirely
```

Wanted: `When arr.Has().Count(2)` / `Then throws XunitException that Message is "…"`. The decision
inside it: the inner recording shares the outer test's `SpecificationContext`, which is why
`HasMessage` reads it out of `ex.InnerException`. Suppressing it is probably right — an act's
internals are not the claim — but only if it stays reachable through the caught exception, because
118 sites assert on it. **Small in the renderer; gates the F3 rewrite.**

### G2 — No `Is().SameAs(obj)` for objects

README §5.5.1 gives collections `Is().SameAs(otherList)`. §5.2.1 gives objects no equivalent, so
reference identity has no faithful assertion — `Is(x)` states equality. Small, and a real hole in the
vocabulary rather than a dogfooding convenience.

### G3 — Nothing to say "this assertion fails"

The public surface has no vocabulary for it, which is why the product ships two `internal` helpers
for its own suite. A user writing a custom assertion has the same problem and no `InternalsVisibleTo`.
Whether that is worth public API is a PO call; G1 may make it unnecessary.

---

## 5. The work

| # | Item | Where | Blocked on |
|---|---|---|---|
| 1 | Add `[assembly: AssemblyFixture(typeof(SpecificationDocument))]`, run green, look at the output | `Core.Test` | — |
| 2 | File what the document shows into `SPECIFICATION-IMPROVEMENT-PLAN.md` §4 | — | 1 |
| 3 | Answer the F2 question in §7 | — | 1 |
| 4 | `Xunit.Assert.Equal/True/False` → `TSpec.Assert` (14 sites) | `Pipeline/AutoDispose.cs` | — |
| 5 | `Mock.Get(…).Verify(…)` → `Then<IDisposableService>(nameof(…), Never)` (1 site) | `Pipeline/AutoDispose.cs` | — |
| 6 | `Xunit.Assert.Equal(-1, …)` → `.Is(-1)` (1 site) | `Pipeline/HavingWhenUntil.cs` | — |
| 7 | `Xunit.Assert.Contains` → `.Does().Contain(…)` (1 site) | `Internal/Document/WhenResolveSubject.cs` | — |
| 8 | `Xunit.Assert.StartsWith` → `.Does().StartWith(…)` (1 site) | `AutoFixture/WhenGivenTwo.cs` | — |
| 9 | **Product:** add `Is().SameAs(obj)` (**G2**) | `Core/Assert/` | — |
| 10 | `Xunit.Assert.Same` → `.Is().SameAs(…)` (4 sites) | `Pipeline/AutoDispose.cs` | 9 |
| 11 | **Product:** fix `Throws` rendering (**G1**) | `Core/Internal/` | — |
| 12 | Message-only probes → act + `Then().Throws<T>().that` (27 sites, 11 files) | 11 files | 11 |
| 13 | Whatever §7 decides about the 468 pins | `Core.Test` | 3 |

**Items 4–8 first in practice** — they need nothing and settle nothing, so they can run alongside
item 1. Items 6–8 in `WhenAddText`, `WhenComposeText` and `WhenPlaceBreakPoints` are *neutral* swaps
(`Xunit.Assert.Equal(a, b)` and `b.Is(a)` read the same); take TSpec's failure output as the reason,
and drop any individual site where the swap reads worse.

Item 12 covers only the 11 of 19 files whose act slot is free. The other 8 need item 13's answer or
stay as they are.

**Verify every conversion against the code it replaces, not against a blank file** — see §6.

---

## 6. Closed — do not re-propose

Five items a foreign-API census flagged that evidence closed.

- **`[Theory]` is not a finding.** TSpec is xUnit: same runner, same attributes, `TheoryAttribute`
  derives from `FactAttribute`, so the completeness gate counts a theory once. Rows render
  identically (TSpec describes source, not values) and `Requirement.From` collapses them to one
  claim, which is the vision's sublinear growth falling out of the existing design. Sixteen theory
  methods pin their own rendered text, so this is observed: `When add x, y / Then Result is sum`
  reads as the universal it is. The only cost is a naming discipline — parameter names become the
  document's words, so `x, y, sum` reads and `t1, t2` does not.
- **`using var` for a local fixture is correct.** Tried, implemented over 7 sites, reverted. README
  §2.4 does endorse `Using(x, owned: true)` as a `using`-statement replacement — but in the shape it
  documents: *a factory in a shared base spec*, one line covering every test. Applied inline around
  a local constructed in the test body it costs a `Spec<T>` type argument the act never uses, puts
  mechanism on the specification page, and grew 17 lines to 32.
- **`NormalizeLineEndings` is not plumbing to remove.** `HasMessage` normalizes both sides
  explicitly before calling `.Is()`, which shows `.Is()` does not normalize. It is a platform-
  independence device.
- **`SetupFailed` via `Then().Throws` is impossible**, deliberately. See F3.
- **`Spec<TInternal>` is impossible.** xUnit requires public test classes (xUnit1000) and a public
  class cannot derive from a less accessible base (CS0060). `SpecificationSubject`,
  `PendingDocument`, `ProjectReferences` and `TextBuilder` are internal, so their specs cannot
  declare them as the result type. Projecting to a public member inside the act (`.Path`, `.Name`)
  distorts what is under test — close those rather than work around them.
- **`using static Moq.Times` is not a finding.** `Then<T>(name, Times)` is documented surface
  (README §4.6.1) and `Times` is Moq's type.

**The rule these establish:** the goal is a better suite; rendering into the specification is the
proxy. Where they disagree the goal wins and the item is closed, not forced. "It produces a
specification line" is not an argument on its own.

---

## 7. For the PO

**The F2 question — where should the renderer's end-to-end coverage live?** This decides item 13,
which is 468 test methods. Answer it from item 1's output, not in advance.

- **(a) Keep the pins.** They fail at the test that broke, naming it. Cost: the status quo — two
  assertions per method, 663 statement blocks, re-pinning on every rendering change.
- **(b) Move to the document.** `SPECIFICATION.md` plus `git diff --exit-code` becomes the renderer's
  end-to-end suite; behaviour tests keep one assertion each and go arrow-bodied. Cost: a rendering
  regression shows as a file diff rather than a named failing test, and de-duplication and hoisting
  mean the document does not pin every leaf verbatim.
- **(c) Split.** Give the renderer a suite of its own — a few dozen specs whose subject *is* the
  rendering — and drop the pins from behaviour tests that are not about rendering. Most work, and
  the only option that leaves both concerns first-class.

Two smaller ones:

- **Is `3.Is().GreaterThan(2)` a requirement of TSpec?** 142 files assert on literals with no `When`,
  produce no pipeline, and contribute nothing to a document. If they are requirements, they need an
  act; if they are unit tests of a library function, close the question and accept that
  `TSpec.Assert` is specified by prose.
- **Should `HasMessage` / `HasAssignments` leave the shipped package?** They are test scaffolding
  inside the product. G1 may remove the need; otherwise they become public API or move into
  `Core.Test`.
