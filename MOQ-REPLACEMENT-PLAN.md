# Moq replacement plan

TSpec takes Moq off its public surface, then out of the package, then builds the mocking language it
could not build on top of Moq. Written for a Claude session in this repository; a living document —
correct it in place as work lands, and move a finished stage to Done as a line.

| Release | Stage | Breaks |
|---|---|---|
| 2.8 | Moq's `Times` leaves the public API, replaced by a TSpec-owned count | done |
| 3.0 | Obsolete and unreachable surface is deleted | done |
| 3.1 | TSpec's own mocking engine on Castle.Core; the Moq package goes | announced in 3.0's notes |
| 3.2+ | A cohesive mocking language, built on the engine | additive |

## 1. Facts established (2026-09-13)

- **No Moq type is in the public API since 3.0.** `wasInvoked:` takes `TSpec.Times`; below the public
  overloads everything still runs on `Moq.Times`, reached through `TSpec.Times.ToMoq()`, which maps to
  the matching Moq factory so Moq's failure wording is kept. The seam (§4.1) takes `TSpec.Times` and
  `ToMoq()` goes. `Any<T>()` and `Any<T>(constraint)` are rewritten to `It.IsAny`/`It.Is` below the
  API (`Internal/Pipelines/AnyArgument.cs`).
- **Inside any `TSpec.*` namespace a bare `Times` binds to `TSpec.Times`**, ahead of `using Moq;` —
  TSpec's own code writes Moq's as `Moq.Times`, and so do raw Moq calls in Core.Test (`AutoDispose.cs`).
- **Moq inside TSpec**: 23 files, about 1,600 lines touch it, through these seams:
  - `MockRegistry` — creates `Mock<T>` by reflection, one per type.
  - `GivenThatCommonContinuation` (365 lines) and the other `GivenThat*` continuations — `Setup`,
    `Returns`/`ReturnsAsync`, `Throws`/`ThrowsAsync`, `Callback`, largely as `is ICallback<TService,
    Task<TReturns?>>` ladders over Moq's fluent interfaces. A sequence is already a TSpec queue behind
    one Moq `Setup` (improvement plan item 7).
  - `ProtectedMember` (165 lines) — `mock.Protected()`, `ItExpr.IsAny`.
  - `TestResult`/`AndVerify` — `mock.Verify(expr, times)`; by-name and whole-service counts read
    `mock.Invocations`.
  - `FluentDefaultProvider` — TSpec's own default-value logic behind Moq's `DefaultValueProvider`.
  - `MockCompiler` — reads Moq's **non-public** `MockedType` property through reflection.
- **A failed verification throws Moq's `MockException`**, with Moq's message text.
  `WhenVerifyExpressionCountPlaceOrder.ThenExpressionOnceFailsWhenNeverCalled` catches that type.
- **Rendering of counts** reads the expression text, through `StringExtensions.NormalizeTimes` and
  `TestResult.DescribeInvocationTimes`, which accept `Once`, `Times.Once` and Moq's `Times.Once()`.
- **Moq used directly in the repository**: Core.Test — `Mock.Get(…).Verify(…, Moq.Times.Never())` in
  `AutoDispose.cs`, `It.IsAny` ×2 in `WhenMockWithAnyArgument.ThenItIsAnyRendersAsAny`, `It.Is` in
  `Tests/ShoppingService/WhenPlaceOrder.cs`, `MockException` ×1, `typeof(Mock<>)` in rendering tests.
  MyHotel no longer uses Moq.
- **Moq features TSpec has no verb for**: raising events, `out`/`ref` arguments, property setters and
  stateful properties, partial mocks that call the base, strict mocks / "no other calls".

## 4. Release 3.1 — TSpec's own mocking engine

### 4.1 First, a seam (no user-visible change)

An internal mock abstraction between the pipeline and Moq, still implemented with Moq. Its surface is
what the engine must provide, so it is the precise size of the swap. Five steps, each landing on its
own with Core.Test (all three frameworks) and both MyHotel suites green and their documents unchanged.

**Why steps, read 2026-09-13.** The code is not large but the Moq coupling is implicit. The setup
continuations carry Moq's fluent return object as `object` and branch on its runtime type in five
`is IReturnsThrows<TService, …>` ladders (`GivenThatCommonContinuation`); taps, sequences and
from-arguments `Returns` depend on Moq's orderings rather than on anything TSpec states. Each of these
is behaviour the seam has to state as a contract, or the engine will break it silently:
- Moq reports an invocation to the callback **before** asking what to return — `Computed` in
  `GivenThatContinuation` relies on it to hold the answer between the two.
- Moq keeps **one** callback per setup, which is why taps are folded (improvement plan item 22).
- A later setup of the same call **overrides** an earlier one.
- An unmatched call asks the default provider, which may answer with the mock itself
  (`IsReturningSelf`), wrap in `Task`/`ValueTask`, throw a service-wide exception, or refuse an
  interface inside a task.
- A service-wide `Returns` is set twice: TSpec's provided default *and* Moq's `SetReturnsDefault`
  for `T`, `Task<T>` and `ValueTask<T>` (`GivenServiceContinuation`) — one may be redundant.

**Step 1 — the mock handle.** DONE 2026-09-13. `MockHandle` (`…/Strategies/Mocking/`) holds Moq's
`Mock`: `MockedType`, `Instance`, and `Invocations` as `MockInvocation(Method, Arguments)` records.
`MockRegistry` creates handles; `Repository`/`Context`/`Fixture` hand them out; `MockingStrategy` and
by-name/whole-service verification read them. `FluentDefaultProvider` no longer derives from Moq's
`DefaultValueProvider`: each Moq mock gets a `MoqDefaultValueProvider` that answers for its handle,
which retired `MockCompiler`'s reflection. The escape hatch is `MockHandle.MoqMock`, reached from
`Spec.GetMock<T>()` (setups) and `TestResult.Mocked<T>()` (verification by expression).

**Step 2 — default answers.** The default-value hook already answers for the handle (step 1). What
remains: service-wide provided defaults and exceptions go behind the handle, and whether the
`SetReturnsDefault` calls in `GivenServiceContinuation` are redundant is found by removing them against
the suite.

**Step 3 — call setup, the large one.** Set up a call named by expression or by protected
`MethodInfo`, and give it a single answer: a function from the call's arguments to its unwrapped
result (or a throw), with the adapter doing the `Task`/`ValueTask` wrapping. One function that both
reads the call and answers it replaces Moq's callback-then-returns pair, so taps, sequences and
`Computed` become plain composition and the ladders go. Split if it grows: synchronous and void first,
then async, then protected (`ProtectedMember`'s refusals stay above the seam; only `Install` moves).

**Step 4 — verification by expression.** `Verify(call, TSpec.Times)` behind the handle; the adapter
keeps Moq's `Verify`, so the failure stays `MockException` with Moq's wording until the engine, where
it has to change anyway (§4.4).

**Step 5 — Moq confined.** `using Moq` only in the adapter's folder, `AnyArgument` included (its
rewrite targets Moq's matchers). The escape hatch goes; Core.Test's `Mock.Get(…).Verify(…)` in
`AutoDispose.cs` becomes `Then<IDisposableService>(nameof(…), Never)` (DOGFOOD-PLAN item 5).

Nothing Moq-typed crosses the seam. `GivenThat*`, `TestResult`, `AndVerify`, `Fixture`, `Context`,
`Repository` and `MockingStrategy` depend on the seam only.

### 4.2 The engine on Castle.Core

Castle.Core is already in every user's dependency graph (Moq depends on it), so this removes a
dependency rather than adding one. Rejected, and why:
- **`System.Reflection.DispatchProxy`** proxies interfaces only — no abstract classes, no protected
  members, so `HttpMessageHandler` could not be mocked, which the README documents.
- **An own Reflection.Emit generator** re-solves what Castle took years to get right: generic methods
  and their constraints, `in`/`out`/`ref`, `Span<T>` parameters, default interface members — and
  users' `InternalsVisibleTo("DynamicProxyGenAssembly2")` for mocking internal types, which Castle
  keeps working and an own generator would have to imitate, public key included for signed assemblies.
- **Source-generated mocks** do not fit auto-mocking of whatever the subject's constructor graph
  reaches at run time.

The engine owns: interception and the invocation log; matching a call against a setup — arguments by
value, `Any<T>()` and `Any<T>(constraint)` as TSpec's own matchers, overloads, generic methods; answers,
with `Task`/`ValueTask` wrapping; counts; and failure messages.

### 4.3 Probe before claiming it works

The reverted setup-by-name attempt (improvement plan items 4/5/6) broke on member kinds it was not
written against. Before the engine is called done, each of these has a passing spec: a property, a
generic method, an `out` parameter, an overload set, a non-virtual member (refused, saying it must be
virtual or abstract), a `Task`-returning void, a `ValueTask<T>` method, a protected method and a
protected property, an internal interface, and `HttpMessageHandler.SendAsync`.

### 4.4 Acceptance

Core.Test, MyHotel's two spec projects, and M5's `Core.Spec`/`Integration.Spec` green, with their
`_specification/` folders regenerated and unchanged. Only verification failure messages may change,
and their new wording is the PO's call — show before/after.

### 4.5 Decisions before the Moq package goes

- **Semver — decided.** Removing the Moq package breaks code that uses Moq only because TSpec brings it
  in (`Mock<T>`, `Mock.Get` on a TSpec mock, `It.IsAny`, `MockException`). The 3.0 release notes
  announce that 3.1 drops the dependency and tell such users to reference Moq themselves, and to write
  `Any` rather than `It.IsAny`/`It.Is`.
- **`It.IsAny` and `It.Is` inside TSpec expressions.** Today they work and render as "any T". On the
  new engine a user who still references Moq would have them evaluated as plain values — `default(T)`
  — and matching silently wrong. The engine must recognise calls on `Moq.It` by name and either
  translate them or refuse with a `SetupFailed` pointing at `Any`. Refusing is simpler.
- **The failure exception.** `MockException` becomes an xUnit failure; one Core.Test test catches it.

**Done when** `Core.csproj` has no Moq reference, the §4.3 probes pass, and §4.4 holds.

## 5. Release 3.2+ — the mocking language

Build only on the engine; each item needs a real spec that is worse without it, and each lands on its
own, with the suite green, before the next starts. Candidates, not yet designed:

1. **Set up a call by name, in the general case.** `Given<IChat>().That(nameof(IChat.Complete)).Returns(…)`
   for public members, not only protected ones. Carries over from the improvement plan, already
   decided there: the return type matches exactly; a name covers every overload of it; a
   from-arguments `Returns` states a signature and narrows the name to that overload; it renders
   "Given IChat.Complete returns …". `ThatProtected` then folds into `That` — accessibility is a fact
   about the mock, decided below the API — and is obsoleted. Sequences by name (`First`/`AndNext`)
   fall out of it.
2. **A service-wide sequence.** `Given<IChatCompletion>().First().Returns(…).AndNext()…` with no
   method named (moved here from RELEASE-PLAN §1). Blocked on Moq today because its return default has
   no sequence; the engine owns defaults, so it does not have to enumerate methods.
3. **The gaps Moq covers and TSpec does not**: raising an event, `out`/`ref` arguments, property
   setters and stateful properties, a partial mock that calls the base, "no other calls". Each is
   taken only on evidence, per the rule above.
4. **Verification messages that read like TSpec's assertion failures**, naming the call as the
   specification does, with the invocations that were made.

Dropped, reopen only if the engine makes it free: from-arguments `Returns` on a sequence step
(improvement plan item 7, dropped 2026-09-11).

## Done

- **2.8.0** — `TSpec.Times` (`Core/Times.cs`, PO's name): `Once`, `Never`, `AtLeastOnce`, `AtMostOnce`
  as properties, `Exactly`, `AtLeast`, `AtMost`, `Between` (inclusive) as methods; bounds no count can
  meet throw `SetupFailed`. The 24 `Moq.Times` overloads went obsolete; a file importing both `Moq` and
  `TSpec` gets an ambiguous qualified `Times`, accepted by the PO. 2026-09-13.
- **3.0.0** — every obsolete member deleted (`Given<T>(setup/transform)`, `DoesNotThrow`, `Another`, the
  `Moq.Times` overloads), with `Obsoletions`, the unreachable `IVerifyService`/`VerifyService` and the
  internals only they used. `SomeOther<T>()` kept and documented. No test needed changing. 2026-09-13.
