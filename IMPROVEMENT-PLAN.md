# TSpec Improvement Plan

Origin: full code review 2026-07-19 (all 1201 tests green on net10.0, zero warnings).
Work the items top-down; each item is self-contained. Tick the checkbox when done.
Baseline version: **1.2.0** (current `PackageVersion` in `Core/Core.csproj`).

## Release train

| Release | Version | Content | Bump rationale (per CLAUDE.md: docs/packaging = patch, new functionality = minor) |
|---|---|---|---|
| R1 | **1.2.1** | P1, P3–P5: correctness fixes, no new API surface | Bug fixes only → patch |
| R2 | **1.3.0** | P6–P10: assert-library API additions (P6 also fixes the P2 bug) | New functionality → minor |
| R3 | **1.4.0** | P11–P14: pipeline/generation features | New functionality → minor |
| — | (no release) | P15–P19: internal refactors | Ship with whichever release comes next; no standalone release needed |

**Release procedure (every release):**
1. Update `PackageVersion` and `PackageReleaseNotes` in `Core/Core.csproj`.
2. Update `README.md` **and** `TSpec-agent-reference.md` for every public API / observable behavior change (hard rule from CLAUDE.md).
3. Run the full suite on **all three** target frameworks (net8.0, net9.0, net10.0):
   `dotnet build Core.Test -f <tfm>` then run `Core.Test/bin/Debug/<tfm>/TSpec.Test.exe`
   (`dotnet test` swallows xunit-v3 exe-runner output — don't use it).
4. Pack and push to nuget.org:
   `dotnet pack Core -c Release` → `dotnet nuget push Core/bin/Release/TSpec.<version>.nupkg --api-key <key> --source https://api.nuget.org/v3/index.json`
   (`GeneratePackageOnBuild` is already enabled; the `.nupkg` also lands under `bin/<config>` on normal builds.)
5. Tag the commit `v<version>`.

---

## R1 — 1.2.1 (correctness patch)

### P1. Standalone `TSpec.Assert` throws NullReferenceException ✅ verified by repro
- [x] Fixed 2026-07-19: `SpecificationContext.Current` now lazily creates a detached context (`??= new()`); standalone tests in `Core.Test/Assert/WhenStandalone.cs`; console repro passes; full suite (1207) green on net8/9/10. Failure messages keep the same format incl. assertion-chain spec block.
- **Problem:** README §5 claims TSpec.Assert "can be used on its own as an alternative to FluentAssertions", but every assertion routes through `SpecificationContext.Current.Assert(...)` (`Core/Assert/Continuations/Constraint.cs:190`), and `Current` is `_currentAssertionContext!` (`Core/Internal/Specification/SpecificationContext.cs:17`) — null unless a `Spec` ctor ran on the current thread. Verified repro: `3.Is().GreaterThan(2)` in a plain console app → NRE. Other call sites that would NRE the same way: `ContinueWith.Continue` (AddAssertConjunction), `ContinueWithThat.that` (AddThat), `AssertionExtensions.And` (AddThen/SetSubject).
- **Suggested fix:** make `SpecificationContext.Current` lazily create a detached, no-op-recording context when none exists (assertions still throw proper `XunitException`s; the spec-text block is simply empty/omitted). Alternative (lesser): correct the README claim. Prefer the code fix — the standalone story is a selling point.
- **Tests:** new test that clears the context (or a separate non-Spec test class) and asserts `3.Is().GreaterThan(2)` passes and a failing assertion produces a clean `XunitException` without the `----` spec section.
- **Docs:** README §5 wording; agent reference if it repeats the claim.

### P2. `Has().Order<TItemComp>()` broken when type argument ≠ TItem ✅ verified by repro
- [x] Folded into P6 (decision 2026-07-19): the P6 redesign deletes the broken casts, so no separate 1.2.1 patch. 1.2.1 ships with the misleading behavior intact; the bug details and tests move to P6.

### P3. Thread-static specification context vs async test methods
- [x] Fixed 2026-07-19: `[ThreadStatic]` → `AsyncLocal` (3-line change in SpecificationContext.cs); regression test `WhenAsyncTestMethod` verified to fail under ThreadStatic; full suite green on net8/9/10.
- **Problem:** `[ThreadStatic]` on `SpecificationContext._currentAssertionContext` (`SpecificationContext.cs:14`). An `async Task` test method that awaits before asserting can resume on a different thread pool thread → `Current` is null (NRE) or another test's context (assertions recorded into the wrong specification, wrong failure text). Note `AsyncHelper` (`Core/Internal/Pipelines/AsyncHelper.cs`) already hops threads for the SUT invocation; it works today only because recording happens on the test thread before/after.
- **Suggested fix:** replace `[ThreadStatic]` with `AsyncLocal<SpecificationContext?>`. `Create()`/`Release()` semantics stay identical. Check `SpecificationContext.PendingSubject` and `Release()` call in `Fixture.TearDown` still behave.
- **Tests:** an `async Task` test method with `await Task.Delay(1)` (or `Task.Yield`) before `Then()` and before an `Is()` assertion; assert the spec text is still produced correctly.

### P4. `Throws(Func<TError>)` compares thrown exception by reference
- [x] Fixed 2026-07-19: contract decision — reference equality is **kept** (it meaningfully verifies the arranged instance propagated); when the actual is a same-type/same-message lookalike, the failure now explains the identity contract and points to the type/condition overloads. XML docs (ITestResult + TestResult), README §2.2.3 and agent reference document the semantics. Tests: `WhenThrowsExpectedInstance`. Suite (1210) green on net8/9/10.
- **Problem:** `Core/Internal/Verification/TestResult.cs:167-174` uses `expected != actual` (reference equality). Works when the func is a mention (`An<ArgumentException>` yields the same cached instance the mock threw), but `Then().Throws(() => new ArgumentException("x"))` can never pass, and the failure message prints two identical-looking strings.
- **Suggested fix:** keep reference equality as the primary check, but when it fails and `expected.GetType() == actual.GetType() && expected.Message == actual.Message`, either pass or produce a message explaining the identity mismatch ("same type and message but different instance — did you mean Throws<T>() or a mention?"). Decide and document.
- **Tests:** mention-instance pass; new-instance behavior per the chosen contract.

### P5. Small correctness/consistency fixes (batch into R1)
- [x] Done 2026-07-19: `Throws()` (untyped) and `DoesNotThrow<TError>()` now record to the specification ("Then throws" / "Then does not throw InvalidOperationException"); new `AddAssertDoesNotThrow<TError>` phrase; spec-text tests in `WhenThrowsExpectedInstance`. Suite (1212) green on net8/9/10.
- [x] Done 2026-07-19: Moq-flow chains and `DataProvider` scope switches now throw `SetupFailed` (`Cannot apply Returns/Throws to '<callExpr>': unhandled mock continuation <type>` / `Unsupported scope: <scope>`); branch order untouched. Test: `WhenUsingScopeNone` (For.None is user-reachable via Using). `TypeConversionStrategy.GetRelays` deliberately keeps `NotImplementedException` — a failure there is a missing framework case, not user error. Suite (1213) green on net8/9/10.
- [x] Done 2026-07-19: deferred sequences are wrapped in a lazily-caching `CachedSequence` at the `Is()`/`Has()`/`Does()` entry points (`AssertionExtensionsEnumerable.Stabilize`) — each element produced at most once, replayed from cache on re-enumeration, so short-circuiting assertions still work on infinite sequences. Already-materialized collections pass through by reference (SameAs unchanged). Remaining trade-off: SameAs on a *deferred* sequence compares the wrapper. Tests: `WhenDeferredEnumerable` incl. infinite-sequence case. README §5.5 + agent reference updated. Suite (1217) green on net8/9/10.
- [x] Done 2026-07-19 (scope extended by user): (1) `CountContinuation` failures now show the actual (condition-filtered) count via a `Describe` override; the count prefix in `EnumerableConstraint.Describe` no longer requires `ICollection`, so lazy sequences get it too. (2) `FormatValue` caps collections at 5 elements + `...` everywhere (incl. the assignments section — no more 10,000-element dumps) and renders elements by their `ToString` capped at 50 chars (records/tuples read naturally; nested shapes are not expanded); strings keep their quotes. `DescribeAtMostFive` (which showed only 4) is deleted. Tests: `WhenFormatLargeCollections`, updated `WhenCount`/`WhenFiveItemsCondition`/`WhenDeferredEnumerable`. README §5.5 notes the format. Corresponding TODO.txt line removed. Suite (1220) green on net8/9/10.
- [ ] Pointless finalizer `~SpecFixture()` (`SpecFixture.cs:13`) — nothing unmanaged; remove finalizer + `GC.SuppressFinalize` dance.
- [ ] Misleading indentation in `Spec_When.cs:146-151` (`AddDelay` body reads as if the call sits under `return;`) — reformat.
- [ ] Discarded side-effecting LINQ in `Spec_Given.cs:74` (`defaultValues?.Take(5).Select(...).ToArray();`) — rewrite as a plain loop (violates the project's own no-gymnastics style rule).
- **Docs:** uniqueness claim in README §3.1 is overstated for narrow types: `PrimitiveStrategy` casts a shared counter, so `byte` wraps at 256, `char` cycles at 94, `bool` has 2 values, `Semantic Age` wraps at 120 — no duplicate guard outside sequences. Either scope the claim ("unique where the value space allows") or add guards. Also note: `bool` generation depends on counter *parity* (`counter.Next % 2 == 1`, `PrimitiveStrategy.cs:29`), so adding an unrelated mention flips every generated bool — TODO.txt already tracks random bools/enums; consider fixing here.

---

## R2 — 1.3.0 (assert-library API additions)

### P6. Generalize `Order(by)` to arbitrary comparable keys (includes the P2 bug fix)
- [ ] Implement
- **Problem 1 (API gap):** selector is `Func<TItemComp, int>` — can't order by string/DateTime/decimal keys.
- **Problem 2 (P2 bug, ✅ verified):** `Core/Assert/Continuations/Enumerable/HasEnumerable.cs:268-278` does `(this as HasEnumerable<TItemComp>)!` — records aren't covariant, so the cast is null whenever `TItemComp != TItem`; `Actual as IEnumerable<TItemComp>` is also null for e.g. `object[]`. Verified: `((object[])[1,2,3]).Has().Order<int>().Ascending()` fails with misleading "Expected numbers to be ascending but found null"; if the assertion passed, chaining `.and` would NRE (`OrderContinuation.Continue()` → `_parent.Continue()` with null `_parent`, `OrderContinuation.cs:65`). The type parameter exists only to smuggle in the `IComparable` constraint; the one scenario it was designed for (non-comparable `TItem`, comparable subtype) is exactly the scenario the casts break.
- **Suggested API:** `Order()` (requires `TItem : IComparable<TItem>` — via extension method on `HasEnumerable<TItem>` with a constraint, so no type-argument trickery) and `Order<TKey>(Func<TItem, TKey> by) where TKey : IComparable<TKey>`. `TItem` stays fixed → delete the `(this as HasEnumerable<TItemComp>)!` cast and `OrderContinuation`'s `TItem : IComparable<TItem>` constraint (compare keys, not items).
- **Tests:** ordered/unordered by string/DateTime keys; non-comparable item type with key selector; chaining `.and` after `Order`.
- **Breaking-change note:** signature change from `Order<TItemComp>(Func<TItemComp,int>?)`; source-compatible for the common calls (`Order()`, `Order(it => it.IntProp)`). Acceptable in a 1.x minor; call it out in release notes.

### P7. Dictionary assertions
- [ ] Implement
- **Gap:** no `ContainKey` / `ContainValue` / key-indexed access; dictionaries fall back to enumerable-of-KVP assertions which read poorly.
- **Suggested API:** `dict.Does().ContainKey(k)`, `.ContainValue(v)`, `dict.Has().Value(k).that.Is(...)` (follow the existing `ContinueWithThat` pattern from `OneItem().that`).

### P8. String assertion gaps
- [ ] Implement
- **Gap:** no regex `Matches`, no `Contain`/`StartWith`/`EndWith` with `StringComparison`, no string length assertion.
- **Suggested API:** `s.Does().Match(pattern)`, comparison overloads on existing `DoesString` methods, `s.Has().Length(n)` (reuse `CountContinuation` mechanics for AtLeast/AtMost/InRange).

### P9. Exception-message sugar on Throws
- [ ] Implement
- **Gap:** message assertion today requires the condition form `Throws<T>(e => e.Message.Contains(...))`.
- **Suggested API:** `Then().Throws<T>().WithMessage("...")` (+ `WithMessageContaining`), returning the existing `IAndThen<TResult>` continuation. Renders better in spec text too (`AssertionPhrases`).

### P10. `VerifyNoOtherCalls` equivalent
- [ ] Implement
- **Gap:** the one Moq verification feature unreachable via the `Using(mock.Object)` escape hatch, because TSpec owns the mock instances (`MockRegistry`).
- **Suggested API:** `Then<TService>().ReceivedNoOtherCalls()` per service, and/or spec-wide `ThenNothingElseHappened()` looping `MockRegistry`'s mocks. Wire through `Pipeline`/`TestResult.Verify` like the existing verification overloads.

---

## R3 — 1.4.0 (pipeline & generation features)

### P11. `ValueTask` / `ValueTask<T>` support
- [ ] Implement
- **Gap:** `When`/`Having`/`Until` and mock `Returns` only handle `Task`; `ValueTask`-returning methods need manual `.AsTask()` or hit "unexpected signature" (`SpecFixture.Invoke` switch, `SpecFixture.cs:43-58`).
- **Suggested fix:** add `Func<TSUT, ValueTask>`/`Func<TSUT, ValueTask<TResult>>` (+ subject-less) overloads to `Spec_When`, `TestPipeline`, `ITestPipeline`, and the `Invoke` switch; mock-side `ReturnsAsync` for ValueTask setups in `GivenThatCommonContinuation`.

### P12. Global generation extensibility
- [ ] Implement
- **Gap:** `DataGenerator`'s strategy list is closed (`DataGenerator.cs:19-30`); custom type families (NodaTime, strongly-typed IDs not derived from `Semantic<T>`) can only be intercepted per-test via `Using<T>().From(...)`.
- **Suggested API:** assembly-level registration in the `Using` vocabulary, e.g. static `TSpecConfig.Using<T>(factory)` / `TSpecConfig.Using(IGenerationStrategy)` consulted between `TypeConversionStrategy` and `DefaultStrategy`. Mind thread-safety (static registry, parallel test collections).

### P13. Auto-convert via static factory scan (from TODO.txt)
- [ ] Implement
- **Detail:** TODO line 1: "look for first public static method with one argument of type TSource that returns TTarget". `TypeConversionStrategy.TryGenerate` already probes implicit operators, single-param ctors, and static methods on the *target* (`TypeConversionStrategy.cs:37-64`) — the missing half is scanning without an explicit `Using<TTarget>().From<TSource>()` registration. While in there: cache the `GetMethods` reflection scans (currently re-run per generated value).

### P14. `Result.As<T>()` (from TODO.txt)
- [ ] Re-evaluate
- **Detail:** TODO says "seems impossible in current C# version" for `Result.As<MyType>().MyProperty.Is(123)`. The `ContinueWithThat<TContinuation, TThat>`/`that` pattern (used by `OneItem().that`) may now express it: `Then().Result.As<MyType>()` returning the casted value after an implicit type assertion, with `SetSubject` bookkeeping like `AssertionExtensions.And`. Also document the position on `[Theory]`/parameterized tests (nothing prevents calling `Given(...)` inside a theory method before `Then()` — decide whether that's supported or an anti-pattern, and say so in README §6).

---

## Internal refactors (no release needed; fold into R2/R3 work)

### P15. Collapse the ordinal/fluent boilerplate with a source generator
- [ ] Implement
- **Scope:** `IGivenContinuation.cs` (511 lines), `GivenContinuation.cs` (254), `Spec_Value.cs` (320), `Spec_Values.cs` (304), plus full-API delegation in `TestPipeline.cs` (156) — ~1,700 hand-written lines of A/An/ASecond…AFifth × {value, setup, transform} × interface/impl/delegate. Inconsistencies already hide in it (e.g. `GivenContinuation.A<TValue>(Action…)` forwards to `_spec.A` while `An` forwards to `_spec.An` — harmless only because they're aliases). Use an incremental source generator (or T4) with one template over five ordinals × three shapes. Do this **before** P6–P14 if possible — it makes every subsequent API addition much cheaper.
- **Caution:** keep XML doc comments in the generated output (`GenerateDocumentationFile` is on; `TreatWarningsAsErrors` will catch omissions).

### P16. Rework the `Constraint` assertion state machine
- [ ] Implement
- **Scope:** `Constraint.cs:121-175` — `[Flags]` enum with pre-declared combined values (`InvertedEither = 3`, `EitherSucceeded = 6`…), `DoAssert` mutating `State`/`Exception` mid-flight, inversion via swallowing `XunitException`. It is the kernel of the assert library and its hardest code. Replace with an explicit evaluation result (Passed / Failed(ex)) plus separate either-tracking; drop the pre-combined enum members. The either/or/not test suite (`Core.Test/Assert/**`) is the safety net — behavior must not change, including `ContinueWith.Continue`'s and/or/but validation rules.

### P17. Deduplicate `HasEnumerable` OneItem…FiveItems
- [ ] Implement
- **Scope:** `HasEnumerable.cs:20-244` — 250 lines of copy-paste across ×5 arities ×2 (with/without condition). Private helper `TItem[] NItems(int n, Func<TItem,bool>? condition, string? expr)` doing the assert, wrapped by five thin tuple-builders → ~60 lines.

### P18. Halve the numeric assertion duplication with generic math
- [ ] Implement
- **Scope:** `AssertionExtensionsNumerical.cs` (219) vs `AssertionExtensionsNullableNumerical.cs` (235) + 24 near-empty `IsX`/`IsNullableX` records. `INumber<T>` is already used in `UsingFromExtensions.Numeric.cs`, so generic math is available on all TFMs. Keep the per-type records only where the continuation type must differ (e.g. fractional `Around`).

### P19. Data-layer clarity fixes
- [ ] Implement
- `DataProvider.TryGetValue` (`DataProvider.cs:78-92`): lookup mutates the dictionary; the double assignment on lines 84–85 is an undocumented reentrancy guard for self-referencing factories — restructure or comment. It currently reads like a bug.
- `Repository.TryGetDefault` (`Repository.cs:54-65`): a Try-getter that generates and mutates state — rename or split.
- `ObjectStrategy.cs:38-47`: silent fallback to parameterless ctor when the greedy ctor throws can mask arrangement errors — record a warning into the specification text (keep the fallback).
- Naming pass over the internal chain `Pipeline → Fixture → SpecFixture` / `Context → Repository → DataProvider/Mutator/DataGenerator` — six nouns whose roles aren't discoverable from the names. Rename opportunistically while touching files; no big-bang rename.

---

## Verification baseline (for every item)

- Build: `dotnet build Core.Test -f net10.0` (also net8.0/net9.0 before release).
- Run: `Core.Test/bin/Debug/net10.0/TSpec.Test.exe` (filter: `-class Namespace.ClassName`). Baseline: 1201 passed / 0 failed.
- Standalone repro harness used in the review: console app referencing `Core/Core.csproj` calling `3.Is().GreaterThan(2)` without a `Spec` — useful for P1/P3 regression tests.
