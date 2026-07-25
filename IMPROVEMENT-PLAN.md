# TSpec Improvement Plan

Origin: full code review 2026-07-19 (all 1201 tests green on net10.0, zero warnings).
Work the items top-down; each item is self-contained. Tick the checkbox when done.

## Status (2026-07-25)

R1 and R2 are **complete**: P1, P3–P5 landed in 1.2.1; P6–P9 in 1.3.0; P10 (`WasInvoked`) in 1.3.1.

Since then, three unplanned releases landed off-plan (they are not P-items):

| Version | Content | Shipped to nuget.org? |
|---|---|---|
| 1.4.0 | `Is().A<T>()` / `An<T>()` type-narrowing assertion exposing the value via `that`; deprecates `Has().Type<T>()` | yes |
| 1.4.1 | Named-method invocation assertion `Then<TService>(nameof(...), Times)` | yes |
| 1.4.2 | Unified `wasInvoked:` invocation-count grammar across all three scopes; deprecates the `Then<TService>().WasInvoked(Times)` continuation | **no — committed, not yet packed/pushed** |

No `v*` tags exist in the repo; the release procedure's tagging step has not been applied to any
release so far.

R3 is renumbered to **1.5.0** because 1.4.x was consumed by the off-plan work above. **P11 (ValueTask)
is done** (2026-07-24) and `PackageVersion` is now **1.5.0**; since 1.4.2 was never pushed, its release
notes are folded into the 1.5.0 notes. **P14 is resolved** by the already-shipped `Is().A<T>().that`.
**P13b is resolved** (2026-07-25, also in 1.5.0) as a docs correction plus enum variation — the uniqueness
guarantee was dropped, not implemented. P12 and P13 are dropped; P17 and P15a (the CRTP generalization) and P18
(2026-07-25) are done and P15b (source generator) is declined, leaving P16 and P19 open.

## Release train

| Release | Version | Content | Bump rationale (per CLAUDE.md: docs/packaging = patch, new functionality = minor) |
|---|---|---|---|
| R1 | **1.2.1** ✅ | P1, P3–P5: correctness fixes, no new API surface | Bug fixes only → patch |
| R2 | **1.3.0 / 1.3.1** ✅ | P6–P10: assert-library API additions (P6 also fixes the P2 bug) | New functionality → minor |
| — | **1.4.0–1.4.2** | Off-plan assert/verification additions (see status table) | New functionality → minor |
| R3 | **1.5.0** | P11, P13b, P14: pipeline/generation features, plus P15a and P17 | New functionality → minor |
| — | (no release) | P16, P19: internal refactors (P15a, P17 and P18 shipped in 1.5.0) | Ship with whichever release comes next; no standalone release needed |
| R4 | **2.0.0** | All/most remaining items done **+ removal of every deprecated member** | Removals are binary- and source-breaking → major |

### 2.0.0 — the destination

2.0.0 is the target once all or most of the plan is done. It is the release that **drops the
deprecated surface** accumulated in 1.x. Everything currently carrying `[Obsolete]`:

| Deprecated member | Replacement | Deprecated in |
|---|---|---|
| `Spec.Then<TService>()` / `ITestPipeline.Then<TService>()` / `TestPipeline.Then<TService>()` (parameterless) | `Then<TService>(wasInvoked: Times)` | 1.4.2 |
| `IAndVerify.And<TObject>()` / `AndVerify.And<TObject>()` (parameterless) | `And<TObject>(wasInvoked: Times)` | 1.4.2 |
| `HasObject.Type<TObject>()` | `Is().A<T>()` / `Is().An<T>()` | 1.4.0 |

Removing these also lets `IVerifyService<TResult>` and `VerifyService` go — nothing else produces them.
Keep this table current: **any new deprecation added in 1.x gets a row here in the same change.**

Note on a removal that **could not** be announced with `[Obsolete]`, and so was simply made in 1.5.0: the
one-type-argument `AssertionExtensionsEnumerable.Order<TItem>(this HasEnumerable<TItem>)` overload. It was
*also* the better overload-resolution candidate for the ordinary `Order()` call on a plain enumerable (more
derived parameter type), so attributing it warned on correct code — verified 2026-07-25, it flagged seven
type-argument-free call sites in the suite. Since the only spelling it uniquely served was the vestigial
P6-era `Order<int>()` — where naming the item type restates what the receiver already fixes — it was
deleted outright rather than deprecated. See [P15a](#p15a-crtp-generalization-of-the-enumerable-constraints--done-2026-07-25-shipped-in-150).

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
- **Structure:** `HasDictionary<TKey,TValue>` derives `HasEnumerable<KeyValuePair<TKey,TValue>>` so `Count`/`OneItem`/etc. keep working on dictionaries. Accepted trade-off for P7: after an inherited enumerable assertion, `.and` returns the *enumerable* continuation (no `.Key`) — the fixed-`TContinuation` limitation. Fixed 2026-07-25 by [P15a](#p15a-crtp-generalization-of-the-enumerable-constraints--done-2026-07-25-shipped-in-150) — the chain now keeps the dictionary vocabulary.
- **Tests:** Key/Value pass+fail (+`no` forms incl. spec text "has no key"), `Has(key).that` chained value assertions, non-string key types, failure messages show full pair listing, mixed chain `Has().Key(k).and.Count(n)`, concrete `Dictionary`/`FrozenDictionary` receivers compile.
- **Included fix (2026-07-20):** `.that` after an inverted assertion — `list.Has().not.OneItem().that` compiles today and hands back a meaningless `default` value on the inverted-pass path (same for TwoItems…FiveItems). Simple solution decided: `ContinueWithThat` learns whether the producing assertion was inverted and `.that` throws `SetupFailed` in that case. Dictionary `Has(key).that` is unaffected (no inverted path reaches it) but uses the same guard.

### P8. String assertion gaps
- [x] Done 2026-07-20: `Does().Match(pattern)` + `Match(Regex)` overload (custom options); `StringComparison` overloads on `Contain`/`StartWith`/`EndWith` (separate overloads, fully non-breaking; comparison renders as " ignoring case" / " using invariant culture" etc. in both spec text and failure message via `DescribeComparison`); `s.Has()` → new `HasString : HasEnumerable<char>` (same pattern/degradation as P7's `HasDictionary`) with `Length(n)` and `Length()` → dedicated `LengthContinuation` ("has length at least 3" phrasing; failure shows actual length: `found 3: "abc"`). `Length` added to `_methodsWithCount`. Drive-by fix: `AsWords` `PresentSingularS` pluralization now handles sibilants (`Match` → "matches", was "matchs"); no existing spec text affected. Tests: `WhenMatch`/`WhenCompareWithComparison`/`WhenLength` (23). README §5.3.2–§5.3.3 + agent reference + release notes updated. Suite 1274 green on net8/9/10.

### P9. Expose the thrown exception via `that` (redesign of "exception-message sugar", decided 2026-07-20)
- [x] Done 2026-07-20 (shipped in 1.3.0): `Then().Throws<TError>()`/`Throws()` return `IThrowsThen<TResult, TError>` (`: IAndThen`, impl `ThrowsThen`) adding `TError that` (records `AddThat`; guarded against inverted paths via the P7 `WasInverted` flag). Full assert vocabulary applies: `.that.Message.Is(...)`, `.that.Message.Does().Match(p)`, `.that.ParamName.Is(...)`. Spec text rides `ParseActual`'s `that.`-stripping → "Then throws Exception that Message is the string" (verified — `ActualDescriber` handles the pipeline-side chain shape). Condition/action/instance overloads unchanged. Test: `WhenGivenThatThrows.GivenSpecificException_ThenThrowsExceptionWithMessage`. README §5.7 (new Asserting-exceptions section covering the whole Throws family + `.that`) + agent reference updated. Additive (return-type change is source-compatible).
- **Gap:** message assertion today requires the condition form `Throws<T>(e => e.Message.Contains(...))` — lambda soup in spec text, and the failure ("didn't satisfy \<expr\>") doesn't show the actual message.
- **Rejected:** the original `WithMessage("...")` / `WithMessageContaining(...)` sketch — combinatorial method-name growth (`Containing`, `Matching`, `StartingWith`, ...) against TSpec's composition philosophy, and a `WithMessage().Matching(...)` form would need a parallel participle grammar shadowing the indicative assert vocabulary.
- **Decided API:** `Then().Throws<TError>()` and untyped `Throws()` return `IThrowsThen<TResult, TError>` (`: IAndThen<TResult>`, untyped binds `TError = Exception`) adding `TError that` — the caught exception, following the `OneItem().that` convention. The entire existing assert vocabulary (incl. P8's additions) then applies with zero new grammar: `.that.Message.Is("...")`, `.that.Message.Does().Match(pattern)`, `.that.ParamName.Is(...)`, inner exceptions, custom properties.
- **Spec text** rides existing machinery: `that` records `AddThat`, and `ParseActual` already strips chains up to `that.` (how `FourItems().that.fourth.Is(...)` renders) → "Then throws ArgumentException that message is "Invalid cart"". Verify `ActualDescriber` handles this pipeline-side chain shape during implementation.
- **Scope:** the condition/action/instance `Throws` overloads keep returning `IAndThen` unchanged. One message assertion style everywhere — no `WithMessage` sugar at all.
- **Breaking note:** `ITestResult.Throws<TError>()`/`Throws()` return-type change — source-compatible for consumers (derived interface), binary-breaking, acceptable in the 1.x minor per the P6 precedent; release-notes line.

### P10. Aggregate mock-invocation assertion `WasInvoked(Times)` (redesign of "VerifyNoOtherCalls equivalent", decided 2026-07-20)
- [x] Done 2026-07-20: parameterless `Then<TService>()`/`And<TService>()` return `IVerifyService<TResult>` (impl `VerifyService`) with `WasInvoked()`/`WasInvoked(Times)`/`WasInvoked(Func<Times>)` → `TestResult.VerifyInvoked` validating `mock.Invocations.Count` via `Times.Validate(count)` (public in Moq 4.20.72), reusing the `CombineWithErrorOnFail` error-combination. Spec phrase `AddWasInvoked` (in `AssertionPhrases`): none⇒"was invoked", Never⇒"was not invoked", Once⇒"was invoked once", else "was invoked {expr}"; `StringExtensions.NormalizeTimes` strips `Times.`/`()` so qualified and `using static` forms render alike. Wired through `ITestPipeline`/`TestPipeline`/`Pipeline`; no `MockRegistry` change. Counts all invocations incl. property access (documented). Tests: `WhenWasInvoked` (`WhenPlaceOrderInvocations`/`WhenCreateCartInvocations`, 10) incl. method-group `using static Moq.Times;`, composition, failures, spec text. README §4.6.1 + agent reference + release notes updated. Suite 1285 green on net8/9/10. **Purely additive.**
- **Gap:** no way to assert a collaborator was *not* called, or was called a bounded number of times in aggregate. The granular form `Then<T>(_ => _.Method(x))` verifies one method; there is no per-mock total. `mock.VerifyNoOtherCalls()` is unreachable because TSpec owns the mocks (`MockRegistry`) and never hands out the `Mock<T>`.
- **Rejected:** (a) `ReceivedNoOtherCalls()` — leans on Moq's stateful consumed-invocation tracking, order-sensitive and subtle. (b) spec-wide `AndNothingElseHappened()` looping all mocks — the mock set is the *implicitly realized* auto-mocks, invisible to the author; scope is unknowable a priori and it forces verifying every arranged-return call. Both dropped.
- **Decided API:** parameterless `Then<TService>()` / `And<TService>()` return `IVerifyService<TResult>` exposing `WasInvoked()` (⇒ `AtLeastOnce`), `WasInvoked(Times)`, `WasInvoked(Func<Times>)`. Asserts `mock.Invocations.Count` against the `Times`. Service is always **named** (no invisibility); assertion is a **stateless count** (order-independent). The dual `Times`/`Func<Times>` overloads mirror the existing `Then` surface so `using static Moq.Times;` gives paren-free method-group calls: `WasInvoked(Never)`, `WasInvoked(Once)` (via `Func<Times>`), `WasInvoked(Exactly(2))` (via `Times`).
  - Composes to cover the old "no other calls" intent as independent facts: `Then<IOrderService>().WasInvoked(Once).And<IOrderService>(_ => _.Create(cart))` ⇒ exactly-one + that-one-was-Create ⇒ Create was the sole call, with no order dependence.
- **Semantics:** counts *all* `mock.Invocations` — method calls **and** property gets/sets/indexers. This is deliberate (a property read is a real interaction; `WasInvoked(Never)` must catch it) and documented. Name is `WasInvoked` (matches Moq's `Invocations`), not `WasCalled`, to signal the any-interaction aggregate tier vs. the method-specific `Then<T>(expr)`.
- **Spec text:** unlike the existing method-verifications (which don't render `Times`), `WasInvoked` must render it. Capture via `[CallerArgumentExpression]` on the times param → `AddWasInvoked<TService>(timesExpr)`: none ⇒ "was invoked", "Never" ⇒ "was not invoked", "Once" ⇒ "was invoked once", else "was invoked {expr}" (e.g. "was invoked Exactly(2)").
- **Wiring:** `Then<TService>()`/`And<TService>()` → `Pipeline`/`TestPipeline`/`ITestPipeline`/`IAndVerify` → `TestResult.VerifyInvoked<TService>(times, timesExpr)`, reusing the `CombineWithErrorOnFail` try/catch (SUT-error → `AggregateException`) but recording `AddWasInvoked` instead of `AddVerify`. No `MockRegistry` changes (uses existing `GetMock<TService>`).
- **Breaking:** none — purely additive (new overloads + new continuation interface).

---

## R3 — 1.5.0 (pipeline & generation features)

### P11. `ValueTask` / `ValueTask<T>` support
- [x] Done 2026-07-24: `ValueTask`/`ValueTask<T>` overloads on `When` (×4: with/without subject, with/without result), `Having` and `Until` in `Spec_When`/`ITestPipeline`/`TestPipeline`, plus the four `SpecFixture.Invoke` switch cases (`.AsTask()` into the existing `AsyncHelper`). Mock side: `That<TReturns>(Expression<Func<TService, ValueTask<TReturns>>>)` overload on `IGivenServiceContinuation` unwraps the value-task like the `Task<T>` one, and `GivenThatCommonContinuation` gained ValueTask branches in all four setup paths (`Returns()`, `Returns(func)`, `Throws<T>()`, `Throws(func)`) incl. `SetupSequence`; `Given<TService>().Returns(...)` also registers a `ValueTask<T>` default. Generation: new `ValueTaskCompiler` (compiled `new ValueTask<T>(value)`), `FluentDefaultProvider` wraps auto-generated results for `ValueTask`/`ValueTask<T>` members (`GetAsyncResult` shared with the Task path), `DataProvider.TryGetValueOfAsync` covers both. Tests: `Core.Test/Pipeline/WhenValueTask.cs` (11) over new subjects `CounterService`/`ICounterStore`. README §2.5 + agent reference updated. Suite 1317 green on net8/9/10.
- **Breaking (compile-time only, narrow) — 5 call sites in our own 386:** a lambda with no inferable return type (`async _ => ...`, or a `throw` body) is now ambiguous between the Task and ValueTask overloads (CS0121). Fix: state the return type — `When(async Task<int> (_) => ...)`, `Until(void (_) => throw ...)` — or drop the `async` and pass the call directly. **An optional `Ignore` tie-breaker parameter does not work** (tried): Roslyn's optional-parameter tie-break only fires when one candidate has *no* omitted optional parameters, and `[CallerArgumentExpression] expr` is always omitted. Annotating the lambda *parameter* type does not help either — only the return type does. Note the fix changes the recorded specification text (the captured expression now includes the return type), and TSpec wraps spec lines at 80 chars.
- ~~**Gap:** `When`/`Having`/`Until` and mock `Returns` only handle `Task`~~

### P12. Global generation extensibility — **dropped 2026-07-24**
- Rejected: a static `TSpecConfig.Using<T>(...)` registry buys global mutable state across parallel test
  collections, initialization-order fragility, and — decisively — arrangement that never appears in the
  generated specification. The convenience case is already served by calling `Using(...)` in a shared base
  Spec constructor (as `Core.Test/AutoFixture/WhenSomeOther.cs:7` does), at zero API cost.
- If a third-party adapter package (NodaTime, strongly-typed IDs) is ever actually wanted, the answer is a
  public `IGenerationStrategy` hook — revisit then, not speculatively.
- Cheap alternative worth doing instead: `ObjectStrategy.cs:47`'s `Failed to create value for type {X}` should
  name the remedy (`Using<X>(...)`), and README §6 should document the shared-base-class pattern.

### P13. Auto-convert via static factory scan (from TODO.txt) — **dropped 2026-07-24**
- Rejected: `TypeConversionStrategy.TryStatic` already implements TODO line 1 (public static one-arg method on
  the target returning the target) — it just requires an explicit `Using<TTarget>().From<TSource>()`. Doing the
  scan *without* a registration would mostly discover `Parse`-shaped factories, which throw on generated input
  (`Sku.Parse("string1")`), replacing today's clear `SetupFailed` with a `FormatException` from inside a
  stranger's method. "First public static method" is also reflection-order-dependent, so a type with several
  `From*` factories could bind differently between runs. Explicit registration is one line and always correct.
- The trailing perf note (cache the per-value `GetMethods`/`GetConstructor` scans in `TypeConversionStrategy`,
  which the neighbouring `IlCompilation` classes already do for constructors/operators) moves to [P19](#p19-data-layer-clarity-fixes)
  as an opportunistic cleanup — worth a measurement first; a typical spec generates a handful of values.

### P13b. Mention uniqueness — **resolved 2026-07-25 as a docs fix + enum variation**
- [x] Done, shipped in 1.5.0.
- **Verdict: there is no uniqueness guarantee, and there should not be one.** The documentation was wrong, not the implementation. Generation promises *variation* and *determinism*: distinct mentions get distinct values where the type has room for them; small value spaces (`bool`, `char`, enums) repeat once exhausted. `Three<bool>()` is `true, false, true`. Users who need particular values state them (`Given([true, false])`, `Using<bool>().From([...])`) rather than have the generator second-guess them.
- **Docs corrected** (the false claim lived in four places): README §3.1 (heading "Uniqueness" → "Variation"), `TSpec-agent-reference.md`, and the XML docs on `Spec_Value.A<T>()`/`An<T>()`. The uniqueness claims on `Using<T>().From<T>()` sequences (`UsingFromExtensions`, `ValuesExhausted`, README §3.4) are accurate and stay — an explicit finite sequence is a user-declared value space, so exhausting it is a genuine setup error.
- **Enum fix (the one real defect):** `EnumStrategy` ignored the counter and returned member 0 for *every* mention, so even a 47-member enum had no variation at all. Now `values.GetValue(counter.Next % values.Length)`, exactly parallel to `bool`'s `counter.Next % 2`; the strategy takes the `Counter` and is instance-constructed in `DataGenerator` (it was `static readonly`, which cannot hold a per-test counter). Sparse enums are indexed by member position, never by numeric value, so `{One=2, Five=5, Ten=10}` cycles correctly; empty enums keep the `Activator.CreateInstance` fallback; `[Flags]` cycles declared members only. Tests: `Core.Test/Given/WhenGivenEnums.cs` (4). Suite 1322 green on net8/9/10.
- **Rejected, with reasons:**
  - *`ValuesExhausted` when a type's values run out* — `AThird<bool>()` throwing would break a legitimate arrangement. Exhaustion is only an error for explicitly declared sequences.
  - *Per-type counters* — the shared counter is a feature: it gives values a rough provenance signature (`String7` and `7` come from different arrangement points), where per-type counters would make every spec's ints `1, 2, 3` and strings `String1, String2`, which read like hand-written literals. The value-shifting it causes only bites tests that hardcode an observed generated value, which the mention idiom exists to prevent.
  - *Bounded retry at the mention layer* to avoid the collision in `A<bool>(); A<string>(); ASecond<bool>()` — declined: two `false`s in a row is a 25% coincidence, not a bug, and re-rolling second-guesses a generator the user can override in one line.
  - *Random values for enums/bools* (was `TODO.txt` line 6, now deleted) — randomness makes collisions probabilistic instead of absent and costs determinism.
  - *Ownership boundary for user-supplied sources/transforms* — moot; there is no guarantee left to bound.

### P14. `Result.As<T>()` (from TODO.txt)
- [x] Resolved 2026-07-24 by `Is().A<T>().that` (shipped 1.4.0) — **no new API needed.** `Then().Result.Is().A<MyType>().that.MyProperty.Is(123)` does exactly what the TODO asked for, and renders as "Then Result is a MyRecord that Name is "Ada"". Verified by `Core.Test/Assert/Continuations/IsObject/WhenResultIsA.cs`. The `ContinueWithThat` route the item speculated about is the one `A<T>()` took; a separate `As<T>()` spelling would only duplicate it. Already documented (README §5 table, agent reference).
- Dropped from this item: a stray note about documenting the position on `[Theory]` tests. It was filed here by mistake (P14 is the TODO.txt cast idea) and describes no defect — arranging inside a theory method works, the suite does it in three places, and README §6.1 already presents itself as a recommendation rather than a rule.

---

## Internal refactors (no release needed; fold into R2/R3 work)

### P15a. CRTP generalization of the enumerable constraints — **done 2026-07-25** (shipped in 1.5.0)
- [x] `HasEnumerable<TItem, TContinuation>` (bound `where TContinuation : HasEnumerable<TItem, TContinuation>, new()`) with a one-parameter `HasEnumerable<TItem>` alias closing over `HasEnumerableContinuation<TItem>`, so existing call sites and the `Has()` entry point are untouched. `HasDictionary` and `HasString` re-root onto their own continuations; `CountContinuation` and `OrderContinuation` gained `TContinuation`; `LengthContinuation` retargeted to `HasStringContinuation`. Removes the P7 degradation: `dict.Has().Count(2).and.Key("a")` and `"abc".Has().Count(3).and.Length().AtLeast(2)` now compile. Tests: `WhenChainingAfterInheritedAssertion` (4) — all were compile errors before. Suite 1326 green on net8/9/10, **zero existing tests changed**. README §5.5.4 note inverted; release-notes line added.
- **Two things worth remembering:** (1) the constraint must bind to `HasEnumerable`, not `EnumerableConstraint` — the looser bound doesn't give `not.Some(...)`, which `None` delegates to. (2) `Order` briefly kept a one-parameter overload so the P6 form `Order<int>()` still bound; it was then **deleted** (decision 2026-07-25) since naming the item type only restates what the receiver fixes. Removing it broke exactly one line in the suite — the `GivenExplicitTypeArgument` assertion that existed solely to lock the P6 promise — which was deleted with it. Source-breaking for `Order<int>()` only; `Order()` and `Order(by)` are unaffected.
- **Not in scope, still open:** the `Is()`/`Does()` round trip. `HasDictionaryContinuation.Is()` returns `IsEnumerable<KVP>`, so crossing verbs and coming back lands in the plain enumerable vocabulary. Separate problem, no user complaint yet.

### P15b. Collapse the ordinal/fluent boilerplate with a source generator — **declined 2026-07-25**
- [ ] Not planned. Revisit only if the generated surface starts growing again.
- **Scope it would have covered:** `IGivenContinuation.cs` (511 lines), `GivenContinuation.cs` (254), `Spec_Value.cs` (320), `Spec_Values.cs` (304), plus full-API delegation in `TestPipeline.cs` (156) — ~1,700 hand-written lines of A/An/ASecond…AFifth × {value, setup, transform} × interface/impl/delegate.
- **Why not:** the original rationale was "do it before P6–P14 so every later API addition is cheaper" — those are all done, so it has expired. Five ordinals is a closed set and the three shapes are stable, so the remaining code is mechanical and low-defect. Decisive argument (user, 2026-07-25): a generator is automagic that makes the code harder to understand, and that cost is paid on every future read.
- **Correction on record:** an earlier claim that a generator would degrade go-to-definition *for package users* was wrong. This generator would run in TSpec's own build; consumers get a compiled DLL plus the XML doc file and cannot tell generated members from hand-written ones. All costs are maintainer-side.
- **What would change the answer:** a sixth ordinal, a fourth mention shape, or P18's numeric duplication turning out not to collapse under `INumber<T>`.

### P16. Rework the `Constraint` assertion state machine
- [ ] Implement
- **Scope:** `Constraint.cs:121-175` — `[Flags]` enum with pre-declared combined values (`InvertedEither = 3`, `EitherSucceeded = 6`…), `DoAssert` mutating `State`/`Exception` mid-flight, inversion via swallowing `XunitException`. It is the kernel of the assert library and its hardest code. Replace with an explicit evaluation result (Passed / Failed(ex)) plus separate either-tracking; drop the pre-combined enum members. The either/or/not test suite (`Core.Test/Assert/**`) is the safety net — behavior must not change, including `ContinueWith.Continue`'s and/or/but validation rules.

### P17. Deduplicate `HasEnumerable` OneItem…FiveItems
- [x] Done 2026-07-24: all ten methods (×5 arities ×2, with/without condition) are now one-line projections over a single private `NItems<TThat>(int n, Func<TItem[], TThat> project, Func<TItem,bool>? condition, string? conditionExpr, [CallerMemberName] string? methodName)`. **−118 lines** (31 insertions / 149 deletions); XML docs and the ten distinct signatures untouched. Zero test changes — 1317 green on net8/9/10, which also proves spec text and failure messages are byte-identical.
- **The thing that made it look unextractable:** `Constraint.Assert` takes `[CallerMemberName] string? methodName`, which drives both the verb ("has three items") and the count-prefix in failures via `EnumerableConstraint._methodsWithCount`. A naive extraction renders every one of these as the helper's name. Fix: `[CallerMemberName]` on the helper too, forwarded explicitly as `methodName:` — the attribute chains.
- **Trap respected:** `var items = new TItem[n]` is allocated *before* the assert, so on inverted/failed paths it stays all-`default` and the projection still builds a well-formed tuple, exactly as the old `TItem? firstItem = default` locals did. Allocating inside the lambda would throw `IndexOutOfRangeException` instead. Covered by `WhenThatAfterInverted` (`not.TwoItems()` on a 1-element array reaches the projection with an untouched array).
- **Was not a trap:** `OneItem` lost `Xunit.Assert.Single` in favour of `Equal(1, length)`; the message is TSpec's own (`Expected arr to have one item but found 0: []`), so nothing changed.
- **Note for [P15](#p15-collapse-the-ordinalfluent-boilerplate-with-a-source-generator):** the CRTP generalization still has to rewrite these ten return types (`ContinueWithThat<HasEnumerableContinuation<TItem>, …>` → `ContinueWithThat<TContinuation, …>`), but the bodies are now one line each, so that edit is cheap.

### P18. Halve the numeric assertion duplication with generic math — **done 2026-07-25** (partial; shipped in 1.5.0)
- [x] The 16 empty integral records (`IsByte`…`IsULong` and the `IsNullable*` twins) are replaced by two generic continuations, `IsIntegral<TActual>` and `IsNullableIntegral<TActual>` (both `where TActual : struct, IBinaryInteger<TActual>`, three lines each). −16 files, ~−130 lines. Suite 1325 green on net8/9/10, no test changes.
- **The extension methods could not be collapsed, and this is structural.** The obvious generic form — `Is<T>(this T actual, …) where T : IBinaryInteger<T>` — does not compile: it is ambiguous (CS0121) with the general `AssertionExtensions.Is<TValue>`. Constraints filter candidates but never make one *more specific*, so the two generic methods tie. The 32 per-type overloads win today only because **non-generic beats generic** in overload resolution, which makes them load-bearing rather than duplication. The reason is recorded in the two classes' XML docs so nobody re-attempts it. Actual saving: roughly a fifth of the duplication, not half.
- **Drive-by:** `uint` had a bare `Is()` but no `Is(expected)` overload, so `5u.Is(6u)` fell through to the general object comparison. The regenerated file is symmetric; `uint` now takes the numeric path like every other integral type. No test covered it either way.
- **Fractional deliberately untouched:** `IsDecimal`/`IsDouble`/`IsFloat` are *not* empty — they implement `AssertEqual`/`AssertNotEqual`, and inconsistently: `IsDecimal` compares `Math.Abs(actual - expected) <= precision` while the other two delegate to xUnit's `Assert.Equal(expected, actual, tolerance)` with its own NaN handling and message. Collapsing them onto `TActual.Abs(...)` via `INumber<T>` is feasible but changes double/float failure text and NaN semantics — a behavior change needing its own decision, not a cleanup. Same for the nullable fractional trio.
- **Breaking:** the `IsInt`/`IsByte`/… public types are gone (source- and binary-breaking for anyone naming them; they normally only appear behind `var`).

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
