# Moq replacement plan

TSpec takes Moq off its public surface, then out of the package, then builds the mocking language it
could not build on top of Moq. Written for a Claude session in this repository; a living document —
correct it in place as work lands, and move a finished stage to Done as a line.

| Release | Stage | Breaks |
|---|---|---|
| 2.8 | Moq's `Times` leaves the public API, replaced by a TSpec-owned count | nothing (obsoletes) |
| 3.0 | Obsolete and unreachable surface is deleted | yes |
| 3.x | TSpec's own mocking engine on Castle.Core; the Moq package goes | see §4.5 |
| 3.y | A cohesive mocking language, built on the engine | additive |

## 1. Facts established (2026-09-13)

- **Moq in the public API is `Moq.Times` only**, in 12 signatures: `wasInvoked:` on `Then<TService>`
  and `And<TService>` — `Spec_Then.cs` (4), `Continuations/ITestPipeline.cs` (4),
  `Continuations/IAndVerify.cs` (4), each as `Times` and as `Func<Times>` for the method-group form.
  `Any<T>()` and `Any<T>(constraint)` are rewritten to `It.IsAny`/`It.Is` below the API
  (`Internal/Pipelines/AnyArgument.cs`).
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
- **Rendering of counts** goes through `StringExtensions.NormalizeTimes` and
  `TestResult.DescribeInvocationTimes`, which accept `Once`, `Times.Once()` and `Times.Once`.
- **Moq used directly in the repository**: Core.Test, 11 files (`using static Moq.Times` ×4, `Mock.Get`
  ×1, `It.IsAny` ×2, `MockException` ×1, `typeof(Mock<>)` in rendering tests); MyHotel `Core.Spec`, 3
  files (`using static Moq.Times`).
- **Moq features TSpec has no verb for**: raising events, `out`/`ref` arguments, property setters and
  stateful properties, partial mocks that call the base, strict mocks / "no other calls".

## 2. Release 2.8 — a TSpec-owned invocation count

**Problem.** `wasInvoked:` takes `Moq.Times`, so every spec that counts invocations writes
`using static Moq.Times;` and TSpec cannot drop Moq without breaking them.

**Proposal.** A TSpec type with the same members, so a spec migrates by changing its `using` line:
`Then<IQueue>(q => q.Send(Any<Msg>()), Once)` reads as it does today. Add overloads for it beside the
`Times` ones in all 12 places, and mark the `Times` overloads `[Obsolete]` through `Obsoletions`.
Rendering must read identically for the new type in both the `using static` and the qualified form.

**Decide** (PO): the type's name — it is written in every `using static` line and in the qualified
form (`wasInvoked: X.Once`). Members: `Once`, `Never`, `AtLeastOnce`, `AtMostOnce`, `Exactly(n)`,
`AtLeast(n)`, `AtMost(n)`, `Between(a, b)` — confirm whether all of Moq's range is wanted.

**Watch.** A spec that imports both `using static Moq.Times` and the new type gets an ambiguous
`Once`; the obsolete message should say to replace the `using`, not add one.

**Done when** Core.Test and MyHotel use the new type, no spec in the repository imports `Moq.Times`,
both docs show only the new form, and the suite is green on net8.0/net9.0/net10.0.

## 3. Release 3.0 — delete what is obsolete or unreachable

Delete, with the tests that only exist to cover them:
- `Given<TValue>(Action<TValue>)` / `Given<TValue>(Func<TValue, TValue>)` and the matching
  `IGivenTestPipeline.And` overloads (`Obsoletions.TypeSetup`) — use `Using<TValue>(…)`.
- `Then().DoesNotThrow()` / `DoesNotThrow<TError>()` (`Obsoletions.DoesNotThrow`).
- `Another<T>()` / `Another<T>(setup)` (`Obsoletions.Another`) — keep "another" in the renderer only
  if 3.0 still reads 2.x specs; otherwise drop its word too.
- The `Times` overloads obsoleted in 2.8.
- `IVerifyService` — public, but nothing public returns it since the `WasInvoked` continuation went.
  Make it internal with `VerifyService`, or delete both.

**Decide** (PO): `SomeOther<T>()` and its four setup/transform overloads (`Spec_Values.cs`) — public,
in neither doc. Document, rename, or delete.

**Announce in the 3.0 release notes** what §4.5 decides about users who use Moq only through TSpec.

**Done when** no `[Obsolete]` remains in `Core`, the docs mention nothing removed, and MyHotel builds.

## 4. Release 3.x — TSpec's own mocking engine

### 4.1 First, a seam (no user-visible change; can start any time)

An internal mock abstraction between the pipeline and Moq, still implemented with Moq, suite green.
Its surface is what the engine must provide, and it is the precise size of the swap:
- create a proxy for an interface or an abstract/virtual class, and know the type it mocks
  (retires `MockCompiler`'s reflection on Moq internals);
- set up a call named by expression, or by member name for a protected member, and give it an
  answer — a value, a value computed from the arguments, an exception, a callback;
- a default-value hook (`FluentDefaultProvider`'s logic, unchanged);
- the invocation log;
- verify by expression, by member name, or the whole service, with a count, failing with TSpec's own
  exception and message.

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

- **Semver.** Removing the Moq package breaks code that uses Moq only because TSpec brings it in
  (`Mock<T>`, `Mock.Get` on a TSpec mock, `It.IsAny`, `MockException`). Either the 3.0 notes declare
  the transitive reference outside TSpec's contract and 3.x removes it, or the package goes in a major
  (4.0, or 3.0 waits for the engine).
- **`It.IsAny` and `It.Is` inside TSpec expressions.** Today they work and render as "any T". On the
  new engine a user who still references Moq would have them evaluated as plain values — `default(T)`
  — and matching silently wrong. The engine must recognise calls on `Moq.It` by name and either
  translate them or refuse with a `SetupFailed` pointing at `Any`. Refusing is simpler.
- **The failure exception.** `MockException` becomes an xUnit failure; one Core.Test test catches it.

**Done when** `Core.csproj` has no Moq reference, the §4.3 probes pass, and §4.4 holds.

## 5. Release 3.y — the mocking language

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

(nothing yet)
