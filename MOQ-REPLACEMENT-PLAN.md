# Moq replacement plan

TSpec takes Moq off its public surface, then out of the package, then builds the mocking language it
could not build on top of Moq. Written for a Claude session in this repository; a living document —
correct it in place as work lands, and move a finished stage to Done as a line.

| Release | Stage | Breaks |
|---|---|---|
| 2.8 | Moq's `Times` leaves the public API, replaced by a TSpec-owned count | done |
| 3.0 | Obsolete and unreachable surface is deleted | done |
| 3.1 | TSpec's own mocking engine on Castle.Core; the Moq package goes | done — 3.1.0 prepared, not yet published |
| 3.2+ | A cohesive mocking language, built on the engine | additive |

## 1. The engine as it stands (2026-09-13)

- **Where it lives**: `Core/Internal/TestData/Generation/Strategies/Mocking/`.
  - `MockHandle` — one mock. Castle makes the instance: a class proxy of `object` implementing an
    interface, or of an abstract class; `DelegateForwarder` compiles a delegate. Every call is logged
    as a `MockInvocation`, then answered by the latest matching setup, else by `FluentDefaultProvider`.
    Of `object`'s members only `ToString` is intercepted; it answers with the mocked type's alias.
  - `CallMatcher` — which calls a setup or verification is about: method (through overrides, with
    generic type arguments), property getter or delegate invocation; arguments by value (collections
    by content), `Any<T>()`, `Any<T>(constraint)`; out arguments match anything and get the setup's
    value. Refuses a non-virtual member and Moq's `It.*` with `SetupFailed`.
  - `AsyncAnswer` — a throw on an awaited call faults the task; a value inside a task is wrapped.
  - `MockRegistry` — one handle per type; `MockingStrategy` — which types are mocked.
- **Verification by expression** counts `CallMatcher` matches in the log (`TestResult.VerifyCall`) and
  fails like a count by name: "Expected IOrderService.CreateOrder(the ShoppingCart) to be invoked
  once but was invoked 0 times".
- **Behaviour pinned, so an engine change cannot drop it silently**:
  - unmatched calls, in order: a service-wide `Returns` value (or a task of it), the service-wide
    exception, a `Using` value, the mock itself, a wrapped task, a generated value
    (`WhenReturnsDefaultValue`, `WhenMockReturnsSelf`, `WhenValueTaskOfInterface`);
  - async throws fault (`WhenAMockedAsyncCallThrows`);
  - member kinds and Moq's unstated behaviours (`WhenMockingEachMemberKind`): `Equals`/`GetHashCode`/
    `ToString` are not invocations, event accessors are; collections match by content; setup
    arguments are read at setup; generic methods by type argument; out parameters; overloads;
    virtual members of a class answer, non-virtual ones run their own code, and setting one up is
    refused; delegates; internal interfaces (the test project declares
    `InternalsVisibleTo("DynamicProxyGenAssembly2")`, as users of Moq did); `HttpMessageHandler`; how
    a mock renders.
- **Moq left in the repository, on purpose**: `Core.Test/AutoMock/MoqIt.cs` stands in for `Moq.It` so
  the refusal can be tested; `Generic.cs` still renders `It.IsAny<T>()` as "any T" and
  `StringExtensions.NormalizeTimes` still accepts Moq's `Times.Once()` — rendering only, harmless; the
  package tags still include `moq`.
- **Inside any `TSpec.*` namespace a bare `Times` binds to `TSpec.Times`.**
- **Moq features TSpec has no verb for**: raising events, `ref` arguments, property setters and
  stateful properties, partial mocks that call the base, strict mocks / "no other calls".

## 2. Open from 3.1

**Not verified here: M5's `Core.Spec`/`Integration.Spec`.** They were part of 3.1's acceptance, but
are not in this repository. Run them against 3.1.0 before publishing.

**Generic type names in `FluentDefaultProvider`'s messages.** Its `SetupFailed` messages name types
with `Type.Name`, so a generic type reads as C# never writes it: the interface-in-a-task refusal says
`Task<IEnumerable`1>` and suggests `Returns(A<IEnumerable`1>)` for a `Task<IEnumerable<MyModel>>`, and
`MostSpecific` would list `ICollection`1`. Use `Alias()`, as the rest of TSpec's messages do. Test
first: extend `WhenMockReturnTaskOfInterface` with a generic value type.

**A tap before `First()` is dropped**, from both the call and the specification:
`That(…).Tap(a).First().Returns(…)` never runs `a` and does not state it (`InSequence` passes neither
the taps nor their text). Decide whether such a tap fires on every call of the sequence, or is
refused; test first either way.

## 3. Release 3.2+ — the mocking language

Build only on the engine; each item needs a real spec that is worse without it, and each lands on its
own, with the suite green, before the next starts. Candidates, not yet designed:

1. **Set up a call by name, in the general case.** `Given<IChat>().That(nameof(IChat.Complete)).Returns(…)`
   for public members, not only protected ones. Carries over from the improvement plan, already
   decided there: the return type matches exactly; a name covers every overload of it; a
   from-arguments `Returns` states a signature and narrows the name to that overload; it renders
   "Given IChat.Complete returns …". `ThatProtected` then folds into `That` — accessibility is a fact
   about the mock, decided below the API — and is obsoleted. Sequences by name (`First`/`AndNext`)
   fall out of it. `CallMatcher.For(MemberInfo)` already matches a member with any arguments.
2. **A service-wide sequence.** `Given<IChatCompletion>().First().Returns(…).AndNext()…` with no
   method named (moved here from RELEASE-PLAN §1). The engine owns defaults, so it does not have to
   enumerate methods.
3. **The gaps Moq covered and TSpec does not** (§1). Each is taken only on evidence, per the rule above.
4. **Verification messages that read like TSpec's assertion failures**, with the invocations that
   were made. The 3.1 wording is a first cut, to be tweaked (PO, 2026-09-13).

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
- **3.1.0, the seam (steps 1–5)** — an internal `MockHandle` between the pipeline and Moq, built on
  Moq, each step green on its own. Moq's `SetReturnsDefault` turned out not to be redundant, and its
  precedence moved into `FluentDefaultProvider`. A call got one answer function, which retired the
  `is IReturnsThrows` ladders and every reliance on Moq's callback-before-returns order. Counts moved
  onto `TSpec.Times`; Moq was confined to four adapter files. PO decision: a throw on an awaited call
  faults the task, however it was set up. 2026-09-13.
- **3.1.0, the engine (E0–E5)** — built beside Moq behind `TSPEC_MOCK_ENGINE=castle`, the Castle-mode
  failure count the progress meter (186 after E1, 25 after E3, 0 after E4). E0 pinned Moq's unstated
  behaviours first (§1). Then Moq's files and package reference were deleted, and Castle.Core 5.1.1 is
  referenced directly. PO decisions: `It.IsAny`/`It.Is` are refused; a mock and a failed verification
  render in line with TSpec's other text, not perfected. Release notes and the agent reference
  updated. 2026-09-13.
