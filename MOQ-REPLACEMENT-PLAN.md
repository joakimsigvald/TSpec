# Moq replacement plan

TSpec takes Moq off its public surface, then out of the package, then builds the mocking language it
could not build on top of Moq. Written for a Claude session in this repository; a living document —
correct it in place as work lands, and move a finished stage to Done as a line.

| Release | Stage | Breaks |
|---|---|---|
| 2.8 | Moq's `Times` leaves the public API, replaced by a TSpec-owned count | done |
| 3.0 | Obsolete and unreachable surface is deleted | done |
| 3.1 | TSpec's own mocking engine on Castle.Core; the Moq package goes | engine done; 3.1.0 prepared, not published — §2 to triage |
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
  - `CallChain` — `_ => _.Child.Get(1)` is set up as `Child` answering with the `IChild` mock, and
    `Get(1)` on that mock; verification counts `Get(1)` there. A receiver TSpec does not mock is not a chain.
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
  the refusal can be tested. Leftovers to tidy are in §2.4.
- **Inside any `TSpec.*` namespace a bare `Times` binds to `TSpec.Times`.**

## 2. Remaining — to triage: fold into 3.1.0, or a later release

3.1.0 is prepared (version, release notes, agent reference) but not packed or published. Everything
below is open; the PO decides next session what 3.1.0 takes. "Probed" means observed on the Castle
engine on 2026-09-13; Moq is gone, so how Moq behaved is inferred where marked.

### 2.1 Before publishing

- **M5's `Core.Spec`/`Integration.Spec` not run.** Part of 3.1's acceptance, but not in this
  repository. Run them against 3.1.0.
- **Castle.Core 5.1.1 → 5.2.1** (asked by the PO). A minor version in the same major line; a user who
  also references Moq 4.20.72 gets it too, since Moq accepts Castle.Core ≥ 5.1.1. Release notes not
  read yet. Update, then run the suite on all three frameworks and MyHotel.

### 2.2 Could break a 3.0 user's test (regressions against Moq)

- **A failed verification no longer lists the calls that were made.** Moq's `MockException` listed
  the mock's performed invocations and setups, which is the main clue when a verification fails.
  Overlaps §3 item 4.

### 2.3 Edges: probed or read, likely harmless

- **An abstract class needing constructor arguments** fails with Castle's raw "Can not instantiate
  proxy of class … Constructor … not found" (wrapped in an `AggregateException`). Moq could not mock
  it either, with its own message. Could refuse with a `SetupFailed` that says to supply one with
  `Using`. Probed.
- **Rendering**: a delegate mock renders as its full type name (`TSpec.Test.….TryLookup`), not its
  alias; an abstract class that overrides `ToString` answers with a generated string. Probed/read.
- **An unmatched default interface member** answers with TSpec's default instead of running its
  body. Probed; Moq without `CallBase` most likely did the same. Unpinned.
- **A `ref` argument** matches by value and is not written back. Probed; most likely as Moq. Unpinned.
- **An indexer setup** (`That(_ => _[1])`) works. Probed; unpinned.
- **`Any<T>()` nested inside an argument** (`new Filter { Id = Any<int>() }`) is read as a generated
  value, so it matches only that value; on 3.0 it became `It.IsAny`, evaluated as `default`. Read.
- **An argument that refers to the setup's own parameter** (`_ => _.Get(_.Id)`) fails with a raw
  `InvalidOperationException` from compiling the value, rather than `SetupFailed`. Read.
- **A sequence on a `Task`-returning call, past its last step**, answers a null task (kept from 3.0).

### 2.4 Decisions and docs

- **A tap before `First()` is dropped**, from both the call and the specification:
  `That(…).Tap(a).First().Returns(…)` never runs `a` and does not state it (`InSequence` passes
  neither the taps nor their text). Decide whether such a tap fires on every call of the sequence, or
  is refused; test first either way.
- **Mocking an internal type** needs `[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]` in
  the test project. Moq users knew it from Moq's docs; TSpec's README and agent reference do not say
  it. Decide whether a reader needs it.
- **A method with arguments inside a chain renders as words**: `_ => _.GetChild(2).Get(1)` reads
  "Given IParent.get child 2.Get(1) returns …"; a property chain reads as written. PO to decide.
- **Housekeeping**: the package tags still include `moq`; `Generic.cs` still renders `It.IsAny<T>()`
  and `NormalizeTimes` still accepts Moq's `Times.Once()` — both now unreachable in practice.

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
3. **Moq capabilities TSpec never exposed.** Each is taken only on evidence, per the rule above:
   - raising an event on a mock;
   - property setters that store and stateful properties (Moq's `SetupProperty`/`SetupAllProperties`);
   - a partial mock that runs the real member (`CallBase`), including default interface members;
   - strict mocks / "no other calls" (`VerifyNoOtherCalls`);
   - a mock implementing further interfaces (`As<TInterface>()`);
   - setting a `ref` argument's value on the way out;
   - matching any type argument of a generic method (`It.IsAnyType`);
   - mocking an abstract class through a constructor that takes arguments.
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
- **3.1.0, generic type names** — `FluentDefaultProvider`'s refusals (interface inside a task, no most
  specific provided default) name types with `Alias()`, pinned in `WhenMockReturnTaskOfGenericInterface`
  and `WhenReturnsAssignableValue`. 2026-09-13.
- **3.1.0, delegate out parameters** — `DelegateForwarder` writes by-ref arguments back after the
  call, so a delegate mock sets its out values as an interface mock does; pinned in
  `WhenADelegateIsMocked`. 2026-09-14.
- **3.1.0, `Mock.Get`** — the release notes say a TSpec mock is not a Moq mock, so `Mock.Get` on one
  throws; referencing Moq directly does not bring that back. 2026-09-14.
- **3.1.0, `Any<T>()` across a conversion** — refused with `SetupFailed` when the parameter's type
  cannot hold a `T` (`Any<MyValueInt>()` on `Get(int)`), as Moq refused it ("Matcher … is
  unmatchable"); it had silently matched nothing. Pinned in `WhenMockWithAnyArgument`. 2026-09-14.
- **3.1.0, chained calls** — set up and verified through the mock of each receiver's type, one per
  type, not a mock per chain as Moq made (PO, option A). Pinned in `WhenMockingAChainedCall`. 2026-09-14.
