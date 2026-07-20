# TSpec Improvement Plan

Origin: full code review 2026-07-19 (all 1201 tests green on net10.0, zero warnings).
Work the items top-down; each item is self-contained. Tick the checkbox when done.

**Status 2026-07-19:** R1 (1.2.1) is **code-complete** — `PackageVersion` bumped to 1.2.1 and
`PackageReleaseNotes` written in `Core/Core.csproj`; suite at 1221 green on net8/9/10.
Remaining for R1: commit, pack, push to nuget.org, tag `v1.2.1` (user does this).
**Status 2026-07-20:** R2 in progress — P6, P7 and P8 done (suite at 1274 green on net8/9/10, version 1.3.0).
**Next up:** P9 (exception-message sugar on Throws), then P10. P15 (source generator) remains deferred; it gained added scope from P7 (CRTP generalization of the enumerable constraints).

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
- [x] Fixed 2026-07-19: contract decision — reference equality is **kept** (it meaningfully verifies the arranged instance propagated); when the actual is a same-type/same-message lookalike, the failure says exactly "Expected the exact exception instance, but a different instance with the same type and message was thrown" (user trimmed it — no overload hints in the message; guidance lives in the XML docs, README §2.2.3 and agent reference instead). Tests: `WhenThrowsExpectedInstance`. Suite green on net8/9/10.
- **Problem:** `Core/Internal/Verification/TestResult.cs:167-174` uses `expected != actual` (reference equality). Works when the func is a mention (`An<ArgumentException>` yields the same cached instance the mock threw), but `Then().Throws(() => new ArgumentException("x"))` can never pass, and the failure message prints two identical-looking strings.
- **Suggested fix:** keep reference equality as the primary check, but when it fails and `expected.GetType() == actual.GetType() && expected.Message == actual.Message`, either pass or produce a message explaining the identity mismatch ("same type and message but different instance — did you mean Throws<T>() or a mention?"). Decide and document.
- **Tests:** mention-instance pass; new-instance behavior per the chosen contract.

### P5. Small correctness/consistency fixes (batch into R1)
- [x] Done 2026-07-19: `Throws()` (untyped) and `DoesNotThrow<TError>()` now record to the specification ("Then throws" / "Then does not throw InvalidOperationException"); new `AddAssertDoesNotThrow<TError>` phrase; spec-text tests in `WhenThrowsExpectedInstance`. Suite (1212) green on net8/9/10.
- [x] Done 2026-07-19: Moq-flow chains and `DataProvider` scope switches now throw `SetupFailed` (`Cannot apply Returns/Throws to '<callExpr>': unhandled mock continuation <type>` / `Unsupported scope: <scope>`); branch order untouched. Test: `WhenUsingScopeNone` (For.None is user-reachable via Using). `TypeConversionStrategy.GetRelays` deliberately keeps `NotImplementedException` — a failure there is a missing framework case, not user error. Suite (1213) green on net8/9/10.
- [x] Done 2026-07-19: deferred sequences are wrapped in a lazily-caching `CachedSequence` at the `Is()`/`Has()`/`Does()` entry points (`AssertionExtensionsEnumerable.Stabilize`) — each element produced at most once, replayed from cache on re-enumeration, so short-circuiting assertions still work on infinite sequences. Already-materialized collections pass through by reference (SameAs unchanged). Remaining trade-off: SameAs on a *deferred* sequence compares the wrapper. Tests: `WhenDeferredEnumerable` incl. infinite-sequence case. README §5.5 + agent reference updated. Suite (1217) green on net8/9/10.
- [x] Done 2026-07-19 (scope extended by user): (1) `CountContinuation` failures now show the actual (condition-filtered) count via a `Describe` override; the count prefix in `EnumerableConstraint.Describe` no longer requires `ICollection`, so lazy sequences get it too. (2) `FormatValue` caps collections at 5 elements + `...` everywhere (incl. the assignments section — no more 10,000-element dumps) and renders elements by their `ToString` capped at 50 chars (records/tuples read naturally; nested shapes are not expanded); strings keep their quotes. `DescribeAtMostFive` (which showed only 4) is deleted. Tests: `WhenFormatLargeCollections`, updated `WhenCount`/`WhenFiveItemsCondition`/`WhenDeferredEnumerable`. README §5.5 notes the format. Corresponding TODO.txt line removed. Suite (1220) green on net8/9/10.
- [x] Done 2026-07-19: finalizer and `Dispose(bool)` pattern removed from `SpecFixture`; `Dispose()` is now a plain idempotent teardown. Suite (1221) green on net8/9/10.
- [x] Done 2026-07-19: `Spec_When.AddDelay` reformatted (early-return + blank line).
- [x] Done 2026-07-19: `Spec_Given` array-overload now assigns the first five values with a plain guarded for-loop instead of discarded side-effecting LINQ. Suite (1221) green on net8/9/10.
- ~~Docs: uniqueness claim~~ **Dropped from 1.2.1** (decision 2026-07-19): no doc stopgap; the uniqueness question is resolved properly by [P13b](#p13b-mention-uniqueness-redesign-from-the-p5h-discussion-2026-07-19) (mention-layer guarantee, per-type counters, ownership boundary).

---

## R2 — 1.3.0 (assert-library API additions)

### P6. Generalize `Order(by)` to arbitrary comparable keys (includes the P2 bug fix)
- [x] Done 2026-07-20: `Order()` is now an extension on `HasEnumerable<TItem>` (`TItem : IComparable<TItem>`, in `AssertionExtensionsEnumerable`); `Order<TKey>(Func<TItem,TKey> by)` (`TKey : IComparable<TKey>`) replaces `Order<TItemComp>` — `TItem` stays fixed, keys compared via `Comparer<TKey>.Default` (null-key safe), broken casts deleted, `OrderContinuation` constraint dropped (holds a compare delegate). Old `Order<int>()`/`Order<int>(it => ...)` calls where the type arg equals the item type still compile (extension fallback / TKey binding); type-arg ≠ item-type now a compile error (was the P2 runtime bug). Tests: `WhenOrder` (18) incl. string/DateTime keys, non-comparable items, null keys, `.and` chaining, explicit-type-arg compat. Version bumped to 1.3.0 with release notes; README §5.5.3 + agent reference updated. Suite 1230 green on net8/9/10.
- **Problem 1 (API gap):** selector is `Func<TItemComp, int>` — can't order by string/DateTime/decimal keys.
- **Problem 2 (P2 bug, ✅ verified):** `Core/Assert/Continuations/Enumerable/HasEnumerable.cs:268-278` does `(this as HasEnumerable<TItemComp>)!` — records aren't covariant, so the cast is null whenever `TItemComp != TItem`; `Actual as IEnumerable<TItemComp>` is also null for e.g. `object[]`. Verified: `((object[])[1,2,3]).Has().Order<int>().Ascending()` fails with misleading "Expected numbers to be ascending but found null"; if the assertion passed, chaining `.and` would NRE (`OrderContinuation.Continue()` → `_parent.Continue()` with null `_parent`, `OrderContinuation.cs:65`). The type parameter exists only to smuggle in the `IComparable` constraint; the one scenario it was designed for (non-comparable `TItem`, comparable subtype) is exactly the scenario the casts break.
- **Suggested API:** `Order()` (requires `TItem : IComparable<TItem>` — via extension method on `HasEnumerable<TItem>` with a constraint, so no type-argument trickery) and `Order<TKey>(Func<TItem, TKey> by) where TKey : IComparable<TKey>`. `TItem` stays fixed → delete the `(this as HasEnumerable<TItemComp>)!` cast and `OrderContinuation`'s `TItem : IComparable<TItem>` constraint (compare keys, not items).
- **Tests:** ordered/unordered by string/DateTime keys; non-comparable item type with key selector; chaining `.and` after `Order`.
- **Breaking-change note:** signature change from `Order<TItemComp>(Func<TItemComp,int>?)`; source-compatible for the common calls (`Order()`, `Order(it => it.IntProp)`). Acceptable in a 1.x minor; call it out in release notes.

### P7. Dictionary assertions
- [x] Done 2026-07-20: `HasDictionary<TKey,TValue> : HasEnumerable<KVP>` with `Key`/`Value`/`no` (in `HasDictionary.cs` + `HasDictionaryContinuation.cs`), entry points `Has()`/`Has(key)` on `IReadOnlyDictionary` in `AssertionExtensionsDictionary.cs`. `Key`/`Value` return `ContinueWith<HasDictionaryContinuation>` so dictionary chains (`Key(a).and.Key(b)`) keep the dictionary vocabulary; only chains through inherited enumerable assertions degrade (as accepted). Key lookup/`Has(key)` respect the dictionary's own comparer (pattern-match to `ContainsKey`/`TryGetValue`, enumeration fallback). Bonus: `dict.Has().not.Key(...)` doesn't even compile (`not` returns the plain enumerable continuation), so `no` is the only negation. Included fix shipped: `ContinueWithThat` carries a `WasInverted` flag (propagated via a new `Constraint.WasInverted` since `Continue()` resets `State`) and `.that` throws `SetupFailed` after inverted assertions. Tests: `WhenKey`/`WhenValue`/`WhenValueForKey`/`WhenThatAfterInverted` (21). README §5.5.4 + agent reference + release notes updated. Suite 1251 green on net8/9/10.
- **Gap:** no key/value/indexed-access assertions; dictionaries fall back to enumerable-of-KVP assertions which read poorly in spec text and failure messages.
- **API (decided 2026-07-20, supersedes the original `Does().ContainKey/ContainValue` sketch — everything lives under `Has`):**
  - `dict.Has().Key(k)` / `dict.Has().Value(v)` — containment, continue asserting on the dictionary.
  - `dict.Has().no.Key(k)` / `dict.Has().no.Value(v)` — `no` is a synonym for `not` (same inversion state, possession-correct grammar: "has no key"), available **only** on the dictionary `Has()` continuation. The inherited general `not` still compiles there; `no` is the documented form.
  - `dict.Has(key).that.Is(...)` — asserts the key exists, exposes the value via `.that` (existing `ContinueWithThat` pattern from `OneItem().that`). Spec phrasing: "Dict has value for key "a" that is 3" (not "has key "a" that is 3" — the value is what `.that` refers to).
- **Receiver:** single overload set on `IReadOnlyDictionary<TKey,TValue>` (offering `IDictionary` too makes calls on a concrete `Dictionary` ambiguous — it implements both). The dictionary `Has()` wins over the enumerable `Has()` by specificity, no `Ignore` trick needed. Doc note: variables *declared* `IDictionary<K,V>` fall back to the KVP-enumerable assertions.
- **Failure messages:** show the entire dictionary as key-value pairs in both `Key` and `Value` failures (the existing `FormatValue` 5-element cap + ellipsis applies).
- **Structure:** `HasDictionary<TKey,TValue>` derives `HasEnumerable<KeyValuePair<TKey,TValue>>` so `Count`/`OneItem`/etc. keep working on dictionaries. Accepted trade-off for P7: after an inherited enumerable assertion, `.and` returns the *enumerable* continuation (no `.Key`) — the fixed-`TContinuation` limitation. Proper fix is the CRTP generalization noted under [P15](#p15-collapse-the-ordinalfluent-boilerplate-with-a-source-generator); document the degradation in README until then.
- **Tests:** Key/Value pass+fail (+`no` forms incl. spec text "has no key"), `Has(key).that` chained value assertions, non-string key types, failure messages show full pair listing, mixed chain `Has().Key(k).and.Count(n)`, concrete `Dictionary`/`FrozenDictionary` receivers compile.
- **Included fix (2026-07-20):** `.that` after an inverted assertion — `list.Has().not.OneItem().that` compiles today and hands back a meaningless `default` value on the inverted-pass path (same for TwoItems…FiveItems). Simple solution decided: `ContinueWithThat` learns whether the producing assertion was inverted and `.that` throws `SetupFailed` in that case. Dictionary `Has(key).that` is unaffected (no inverted path reaches it) but uses the same guard.

### P8. String assertion gaps
- [x] Done 2026-07-20: `Does().Match(pattern)` + `Match(Regex)` overload (custom options); `StringComparison` overloads on `Contain`/`StartWith`/`EndWith` (separate overloads, fully non-breaking; comparison renders as " ignoring case" / " using invariant culture" etc. in both spec text and failure message via `DescribeComparison`); `s.Has()` → new `HasString : HasEnumerable<char>` (same pattern/degradation as P7's `HasDictionary`) with `Length(n)` and `Length()` → dedicated `LengthContinuation` ("has length at least 3" phrasing; failure shows actual length: `found 3: "abc"`). `Length` added to `_methodsWithCount`. Drive-by fix: `AsWords` `PresentSingularS` pluralization now handles sibilants (`Match` → "matches", was "matchs"); no existing spec text affected. Tests: `WhenMatch`/`WhenCompareWithComparison`/`WhenLength` (23). README §5.3.2–§5.3.3 + agent reference + release notes updated. Suite 1274 green on net8/9/10.

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

### P13b. Mention uniqueness redesign (from the P5h discussion, 2026-07-19)
- [ ] Implement (behavior change in generated values → minor release; pairs with TODO's random-enums/bools item)
- **Principle: uniqueness is a property of the mention system, not the generator.**
  1. Guarantee: distinct mentions of a type get distinct values up to the type's cardinality; beyond that throw `ValuesExhausted` (consistent with sequence semantics) — e.g. `AThird<bool>()` throws. Enforce at the repository/mention layer: retry generation while the value collides with an already-stored mention of that type.
  2. Exemption: anonymous generation (object-property fills in `ObjectStrategy`, `Any`/`Another`, internal fills) must NOT demand uniqueness — an object with three bool properties must construct. This falls out of putting the guard at the mention layer, not in the generator.
  3. Per-type counters (replace the single shared `Counter`): fixes value *stability* — today an unrelated mention flips every downstream bool and shifts all values. `A<bool>()` becomes deterministically true, `ASecond<bool>()` false. "Equivalent types" (nullable/Task/semantic/converted) keep shared sequences automatically because they delegate to the underlying type's generator.
  4. Ownership boundary: built-in generation guarantees mention-distinctness (or throws); once a user-supplied source/conversion/transform is in play, the user owns the value space and the guarantee ends (generalizes the existing generator-function doc note; avoids the re-roll rabbit hole for transforms like `x => x % 2`). Nuance: for *conversions* the mention guard may retry by pulling the next source value, surfacing the source's `ValuesExhausted` when exhausted.
- Note: the docs-only stopgap (P5h) was dropped from 1.2.1 — this item is the sole resolution of the uniqueness question.

### P14. `Result.As<T>()` (from TODO.txt)
- [ ] Re-evaluate
- **Detail:** TODO says "seems impossible in current C# version" for `Result.As<MyType>().MyProperty.Is(123)`. The `ContinueWithThat<TContinuation, TThat>`/`that` pattern (used by `OneItem().that`) may now express it: `Then().Result.As<MyType>()` returning the casted value after an implicit type assertion, with `SetSubject` bookkeeping like `AssertionExtensions.And`. Also document the position on `[Theory]`/parameterized tests (nothing prevents calling `Given(...)` inside a theory method before `Then()` — decide whether that's supported or an anti-pattern, and say so in README §6).

---

## Internal refactors (no release needed; fold into R2/R3 work)

### P15. Collapse the ordinal/fluent boilerplate with a source generator
- [ ] Implement
- **Scope:** `IGivenContinuation.cs` (511 lines), `GivenContinuation.cs` (254), `Spec_Value.cs` (320), `Spec_Values.cs` (304), plus full-API delegation in `TestPipeline.cs` (156) — ~1,700 hand-written lines of A/An/ASecond…AFifth × {value, setup, transform} × interface/impl/delegate. Inconsistencies already hide in it (e.g. `GivenContinuation.A<TValue>(Action…)` forwards to `_spec.A` while `An` forwards to `_spec.An` — harmless only because they're aliases). Use an incremental source generator (or T4) with one template over five ordinals × three shapes. Do this **before** P6–P14 if possible — it makes every subsequent API addition much cheaper.
- **Caution:** keep XML doc comments in the generated output (`GenerateDocumentationFile` is on; `TreatWarningsAsErrors` will catch omissions).
- **Added scope (from P7, 2026-07-20):** generalize the enumerable constraints CRTP-style — introduce `HasEnumerable<TItem, TContinuation>` (and siblings as needed) so subclasses like `HasDictionary<TKey,TValue>` can close the continuation type over themselves. Removes the accepted P7 degradation where chaining `.and` after an inherited enumerable assertion on a dictionary returns the plain enumerable continuation (losing `.Key`/`.Value`/`no`); update the README note about that degradation when done.

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
- Open questions from the P5b session (user's principle: user setup error → `SetupFailed`; missing framework case → `NotImplementedException`):
  - `DataProvider.TryGetValueOfType` (`DataProvider.cs:60`) currently throws `SetupFailed` for unknown scope but is only reachable from framework code (callers always pass Input/Subject) — by the principle it should arguably revert to `NotImplementedException` (its sibling `GetDefaults` is user-reachable via `Using(x, For.None)` and stays `SetupFailed`).
  - `Using<TTarget>(For.None).From<TSource>()` slips past `TypeConversionStrategy.Register`'s overlap guard (None never "overlaps") and reaches `GetRelays` → `NotImplementedException`, though it is user error — a `For.None` check in `Register` would classify it correctly.
  - Should `Using(x, For.None)` be an error at all, or a silent no-op ("applies neither to Input nor Subject" per the enum doc)? Currently throws `SetupFailed` (test `WhenUsingScopeNone`).

---

## Verification baseline (for every item)

- Build: `dotnet build Core.Test -f net10.0` (also net8.0/net9.0 before release).
- Run: `Core.Test/bin/Debug/net10.0/TSpec.Test.exe` (filter: `-class Namespace.ClassName`). Baseline after R1: 1221 passed / 0 failed.
- Standalone repro harness used in the review: console app referencing `Core/Core.csproj` calling `3.Is().GreaterThan(2)` without a `Spec` — useful for P1/P3 regression tests.
