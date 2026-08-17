# Self-hosting — testing TSpec with TSpec

Plan for the capability that lets a TSpec pipeline be the *subject* of a TSpec test, rather than
being both the code under test and the thing asserting on it.

Written 2026-08-09 against `main`. Companion to [DOGFOOD-PLAN.md](DOGFOOD-PLAN.md), which is about
the suite; this one is about the four product changes that suite needs. Findings about what a
generated document *says* still belong in
[SPECIFICATION-IMPROVEMENT-PLAN.md](SPECIFICATION-IMPROVEMENT-PLAN.md).

---

## 1. The problem in one sentence

A `Spec` instance is currently both the pipeline under test and the vehicle asserting on it, so
anything that pipeline does to *itself* — fail its own setup, record its own specification — can
never be an observed outcome.

That single conflation is the root of three separate symptoms already measured in `Core.Test`:

| Symptom | Size |
|---|---|
| `Xunit.Assert.Throws<SetupFailed>` — a setup failure cannot be an outcome | 46 sites |
| `Specification.Is(…)` bolted onto a behaviour test — the recording cannot be a subject | 468 test methods |
| `Xunit.Assert.Throws<XunitException>` — an assertion failure cannot be an outcome | 179 sites |

---

## 2. What is already verified

Prototyped and run this session. Everything below is observed, not inferred.

| # | Finding | Evidence |
|---|---|---|
| V1 | A spec **can** be the subject of another spec | `Spec<CounterSpec, int>` compiles; TSpec auto-constructs and auto-disposes the inner spec; a result assertion passes first try |
| V2 | The ambient context is a **single slot**, last-writer-wins | `AsyncLocal<SpecificationContext?>`; `Create()` overwrites, `Release()` nulls |
| V3 | Consequently the **outer spec's assertions are lost** | Outer rendered `When … / Then` with its own `.Is(2)` missing — it recorded into the inner context |
| V4 | Pipeline steps and assertions take **different routes** | steps → the instance's `Specification` field; assertions → `SpecificationContext.Current` |
| V5 | An inner `SetupFailed` **does** reach the outer `try` | stack trace runs inner `PrepareToExecute` → outer `SpecFixture.Invoke` → outer `Pipeline.Execute` |
| V6 | Only the filter refuses it | [Pipeline.cs:170](Core/Internal/Pipelines/Pipeline.cs:170), `catch (Exception ex) when (ex is not SetupFailed)` |
| V7 | Removing that filter costs **2 tests of 1615** | both are themselves `Xunit.Assert.Throws<SetupFailed>(() => … .Then().DoesNotThrow())` — conversions, not regressions |
| V8 | The filter still earns its keep | `SpecFixture.Invoke` throws `SetupFailed` on a bad act signature; unscoped removal would let a `Throws<SetupFailed>()` assertion pass for the wrong reason |
| V9 | The outer act **renders badly** | `When when ++s.Counter.Having(s.Counter++).Then().Result` |
| V10 | `TSpec.Assert` has **no standalone throw assertion** | every `Throws` hangs off `TestResult` |
| V11 | Parallelism is **not** affected | `AsyncLocal` isolates test methods; xUnit gives each test its own flow. The defect is nesting within one flow |

---

## 3. The four changes

### C1 — Context scoping ✅ *done 2026-08-09*

Landed as **two** changes, not one. The stack alone was necessary but not sufficient — see the note
below, which is the part the plan got wrong.

**C1a — the slot becomes a stack.** `Create()` remembers what it displaced; `Release()` is now an
instance method that restores it, guarded by `ReferenceEquals` so releasing out of order or twice
leaves a live context alone. `Fixture.TearDown` calls `Specification.Release()`.

**C1b — a spec re-takes the slot when its claim is read.** `Pipeline.Claim` calls
`Specification.MakeCurrent()`. Construction order decides who *holds* the slot, but an assertion
belongs to the spec whose claim it follows — so `outer.Then().Result.Is(1)` records into the outer
spec even while an inner one is alive. `Claim` is the single choke point: `Result`, `Then()` and
every `Then<TService>()` overload route through it.

**Why C1a alone was not enough.** It fixed the *disposed* case but not the *alive* case: while an
inner spec lives it legitimately holds the slot, and a stack cannot know which spec a static
`TSpec.Assert` extension is about. C1b supplies that. The original plan described only C1a.

**Verified:** `Core.Test/Internal/Specification/WhenSpecsNest.cs`, 5 requirements, written failing
first. Suite 1620/1620 on net8.0, net9.0 and net10.0. Both MyHotel documents regenerate
**byte-identical** apart from the build id — the strongest available regression check on a recording
change.

**Learned, worth keeping:** asserting on a specification is itself a recorded step, so one test can
pin only one specification — pinning two writes the first assertion into the second's record. That is
README §6.1.5's one-assertion-per-test rule showing its teeth, and it is a constraint on how the 468
pins in `DOGFOOD-PLAN.md` F2 can ever be split.

### C1 — original description *(kept for the record)*

`Create()` overwrites the ambient slot without remembering; `Release()` nulls it instead of
restoring. Make it a stack:

```csharp
private SpecificationContext? _previous;

internal static SpecificationContext Create()
{
    var created = new SpecificationContext { _previous = _currentAssertionContext.Value };
    _currentAssertionContext.Value = created;
    return created;
}

internal void Release()
{
    if (ReferenceEquals(_currentAssertionContext.Value, this))
        _currentAssertionContext.Value = _previous;
}
```

`Fixture.TearDown` calls `Specification.Release()` instead of the static. The `ReferenceEquals`
guard makes an out-of-order or double dispose a no-op rather than blanking a live context.

**Safe by construction:** with one spec `_previous` is null, so behaviour is identical to today.
Still `AsyncLocal`, so test isolation is unchanged.

### C2 — Run the act inside a discarded scope ✅ *done 2026-08-09*

**Hypothesis confirmed:** C1's primitive fixes both halves of G1, and one `SpecificationContext.Create()`
around `_fixture.Invoke` in `Pipeline.Execute`, released in a `finally`, was the whole change.

**The mechanism, now understood.** The two halves of G1 had one cause. An act that asserts records
into the enclosing context; when that assertion *fails*, building its failure message **observes**
the enclosing specification, and observing freezes it (see
`WhenRecordAfterObservingSpecification`). So the orphan line appears *and* the real `Then throws …`
can never be added afterwards — it is recorded into a frozen specification. A scope of the act's own
fixes both at once.

Before → after:

```
When arr.Has().Count(2)          When arr.Has().Count(2)
Arr has count 2            →     Then throws XunitException
```

```
When arr.Has().Count(1)          When arr.Has().Count(1)
Arr has count 1            →     Then does not throw
Then does not throw
```

**Verified:** `Core.Test/Pipeline/WhenTheActAsserts.cs`, 3 requirements, written failing first.
Suite 1623/1623 on all three frameworks. Both MyHotel documents byte-identical in every
specification line.

**The risk that did not materialise:** value mentions resolved during the act (`An<int>()` in
`When(_ => new MyModel { Id = An<int>() })`) record assignments, and a discarded scope could have
lost them. `WhenIsValueFail`'s four `HasAssignments` requirements still pass — assignments are held
by the pipeline, not the ambient context.

**Unlocks** `DOGFOOD-PLAN.md` item 12 (38 message-only probes) and, with C3, item 16.

### C3 — Scoped `SetupFailed` capture ✅ *done 2026-08-09, with one limitation*

**The rule shipped:** a failure that has *left a pipeline* came from a nested specification and is an
outcome the enclosing act may report; the enclosing pipeline's own has not left anything yet and must
escape. Three small pieces:

- `SetupFailed.LeftItsPipeline`, internal, set once;
- `Pipeline.Run` marks it on the way out — `catch (SetupFailed ex) { ex.MarkLeftItsPipeline(); throw; }`;
- the filter becomes `catch (Exception ex) when (ex is not SetupFailed setup || setup.LeftItsPipeline)`.

No throw site was touched — there are 61 of them, and tagging each was rejected as too wide. The
ordering works because the inner `catch` handles and rethrows during the first pass, before the outer
`when` filter is evaluated.

**The guard is tested, not assumed:** `WhenTheSpecItselfIsMisconfigured` asserts that a spec's own
setup failure still escapes rather than satisfying its own `Throws<SetupFailed>()`. Per §7 this is
the change that could silently weaken a guarantee, so it has a requirement of its own.

**Known limitation — a failure raised while the inner spec is still being *configured* still
escapes.** `new InnerSpec().When(…).When(…)` throws from the fluent call itself, before any pipeline
runs, so nothing marks it and the outer cannot tell it from its own. Documented by
`GivenTheInnerFailsWhileBeingConfigured_ThenItStillEscapes`. Closing it means marking at the throw
sites — a wider change than this rule, and it should be judged on its own. This is the same
fluent-call-time category already noted in `DOGFOOD-PLAN.md` G6.

**Verified:** `Core.Test/Pipeline/WhenTheActRunsASpec.cs` + `WhenTheSpecItselfIsMisconfigured`, 4
requirements, written failing first. Suite 1627/1627 on all three frameworks. Both MyHotel documents
byte-identical in every specification line.

### C4 — A standalone throw assertion in `TSpec.Assert`

Per V10, anything thrown outside the act has no TSpec vocabulary. An `Action`-level `Throws<T>()`
closes that, and is the only one of the four that helps tests with no pipeline at all.

Orthogonal to C1–C3 and independently shippable.

---

## 4. Use cases

### 4a. TSpec's own suite

| | Case | Unlocked by | Size |
|---|---|---|---|
| I1 | Pipeline semantics — `Having`/`When`/`Until` order, double-`When`, `Then` before `When` | C1+C3 | `HavingWhenUntil.cs`, 8 facts |
| I2 | Lifecycle and disposal order | C1 | `AutoDispose.cs`, 13 facts — already constructs inner specs, but from a plain xUnit class |
| I3 | Setup-failure behaviour as an *outcome* | C1+C3 | 46 sites |
| I4 | Assertion-failure behaviour as an *outcome* | C2 (mainly), C1 | 179 sites |
| I5 | The rendered specification as a *subject* — `Then().SubjectUnderTest.Specification.Is(…)` | C1 | 468 test methods |
| I6 | Data-generation exhaustion (`ValuesExhausted`) | C1+C3 or C4 | 11 sites |
| I7 | Mock-verification failure messages | C2 | subset of I4 |

I5 is the strategically important one: it is the only mechanism found so far that lets a behaviour
test and a rendering test be **separate tests**, instead of two assertions in one method.

### 4b. Users building on TSpec

| | Case | Why it needs this |
|---|---|---|
| U1 | **Testing a shared base spec** — `ApiSpec<TResult>`, `DomainSpec`, etc. | The strongest external case. README §2.4 documents the pattern (`Using(CreateClient, owned: true)` in a base class); a mature base spec accumulates auth, tenant setup, seeding and client lifetime. Today the only way to test it is to run every derived spec and infer |
| U2 | **Testing custom assertion extensions** | Exactly TSpec's own I4 problem. A user writing `Result.Is().AValidIban()` needs to check it fails with the right message, and has no access to the `internal` helpers TSpec uses for itself. README §5 supports `TSpec.Assert` standalone, so custom assertions are expected usage |
| U3 | **Testing shared spec helpers and builders** | Same shape as U1, one level down |
| U4 | **Executable documentation of TSpec behaviour** | A team's own conventions doc can assert what its base spec does, rather than describing it |

### 4c. Byproducts

| | Effect | From |
|---|---|---|
| P1 | Any act that internally uses `TSpec.Assert` stops polluting the specification | C2 |
| P2 | Failure messages carry the right specification in nested scenarios | C1 |
| P3 | `HasMessage` / `HasAssignments` can leave the shipped package | C2+C4 |

### 4d. Already possible — **not** reasons to do this

Recorded so they are not counted as benefits.

- **Testing custom data generators and type conversions.** `Using<T>().From<S>()` registrations are
  already testable by asserting on generated values inside an ordinary spec.
- **Meta-tests over a suite** ("every spec declares an act"). That is reflection, not self-hosting.
- **Anything about runtime behaviour, parallelism or performance.** Nothing here touches those.

### 4e. Explicitly out of reach

- **The document fixture end-to-end** — `SpecificationDocument`, `ExpectedRequirements`, the
  collector. These are assembly-scoped and process-wide static; `TODO.txt` already notes they
  "cannot be self-tested from inside the same assembly". Self-hosting does not change that; a second
  assembly is still needed.
- **xUnit integration points** — fixture wiring, `TestState` at `Dispose`, end-of-assembly ordering.
  Same reason.

---

## 5. Order

1. ~~**C1**, with a test that an outer spec's assertion survives an inner spec's construction and
   disposal.~~ ✅ done — landed as C1a (stack) + C1b (`MakeCurrent` on `Claim`).
2. ~~**Verify the C2 hypothesis.**~~ ✅ confirmed — the same primitive fixes both halves of G1.
3. ~~**C2**~~ ✅ done. **Next: convert a handful of I4 sites and read what they render.**
4. ~~**C3**~~ ✅ done. ~~`HavingWhenUntil.cs` (I1) as the pilot.~~ ✅ done — §6 carries the verdict.
5. **Stopped here, as planned.** The rendering question in §6 is now costed and is the gate. **Do
   not convert further until it is settled** — the pilot shows a blanket conversion makes the suite
   longer and the document worse.
6. **C4** — independent; can land any time.
7. I2, I5, I6 by the pilot's verdict.

---

## 6. The pilot — `HavingWhenUntil.cs`, converted 2026-08-09

**Test the mechanism, not the recommended structure.** A first attempt split the file into one class
per pipeline, per README §6.1, and cost 8 facts → 14 and 105 lines → 164 for no gain. Reverted. The
conversion that shipped keeps the original shape and changes only what the tests assert *on*.

|  | before | after |
|---|---|---|
| Requirements | 8 | **8** |
| Classes | 2 | 3 (the third is an empty `CounterSpec`) |
| Lines | 105 | 121 |
| `Xunit.Assert` | 4 | **1** |

**Minimal subject.** `CounterSpec : Spec<MyStateService, int> { }` — empty, over a subject that is one
`int` field. What is specified is the pipeline; the subject only needs a value to move.

**Three of the four `Xunit.Assert.Throws` are gone.** A setup failure is now an outcome:
`.Then().Throws<SetupFailed>().that.InnerException.Is().A<ApplicationException>()`. The fourth is the
fluent-call-time case C3 does not cover, kept with a remark pointing at the limitation.

**The constraint that decides the shape.** When the *outer* spec asserts the result, the inner
pipeline has no claim of its own, so its specification ends at `Then`:

```
When ++Counter
Having Counter++
Then
```

That is honest and it is enough — these requirements are about the *order* of the clauses, which is
exactly what the pin shows. Chasing a complete inner specification instead means making the act the
whole inner test, which forces `When(void (CounterSpec _) => …)` and renders far worse. **Let the
inner specification end at `Then`.**

**Verified:** 8/8, suite 1627 on all three frameworks, MyHotel documents byte-identical.
Mutation-checked in both directions — dropping a `Having` line from a pin fails, and a wrong `Result`
fails — because a pin ending in `Then` could otherwise hide a truncation.

## 6b. Still open — the outer act's own rendering

Unchanged by the pilot and still the gate on converting anything further:

```
When when ++s.Counter.Having(s.Counter++).Then().Result
```

Options, in the order I would try them:

- **Exclude specs-testing-specs from the document.** Cheapest, and defensible — a specification of
  TSpec's own pipeline is not a claim about a user-facing subject.
- **Describe the inner pipeline structurally** rather than as source text. Most work, best result.
- **Hide the construction behind a named method on the inner spec.** Cheap, but it moves the pipeline
  out of the test that is about it.

**Recommendation regardless:** convert for the *capability*. Where a setup or assertion failure needs
to be an outcome, this is the only way and it pays. Tests that already assert cleanly on their own
pipeline gain nothing.

---

## 7. Risks

- **C3 is the one that can silently weaken a guarantee.** If origin-tagging is wrong in either
  direction, a misconfigured test passes as an expected throw. It has a 2-test regression surface
  (V7), so the tests will not catch a subtle over-capture — review it by reasoning, not by the suite.
- **`Spec<CounterSpec, int>` is a strange thing to meet in a codebase.** The pattern earns its place
  for framework and base-class authors, not for everyday tests. Whatever ships should be documented
  as such in README §6, or users will reach for it where a plain spec is right.
- **C1 changes a process-wide primitive.** Low risk by construction (single-spec behaviour is
  identical), but it is the kind of change whose failure mode is a wrong *recording* rather than a
  failing test — the quiet kind. Add a test that reads the context after nesting, not just one that
  passes.
