# Moq replacement plan

TSpec takes Moq off its public surface, then out of the package, then builds the mocking language it
could not build on top of Moq. Written for a Claude session in this repository; a living document —
correct it in place as work lands, and move a finished stage to Done as a line.

| Release | Stage | Breaks |
|---|---|---|
| 2.8 | Moq's `Times` leaves the public API, replaced by a TSpec-owned count | done |
| 3.0 | Obsolete and unreachable surface is deleted | done |
| 3.1 | TSpec's own mocking engine on Castle.Core; the Moq package goes | done; 3.1.0 ready to pack and publish |
| 3.2+ | A cohesive mocking language, built on the engine | additive |

## 1. The engine as it stands (2026-09-14)

- **Where it lives**: `Core/Internal/TestData/Generation/Strategies/Mocking/`.
  - `MockHandle` — one mock. Castle makes the instance: a class proxy of `object` implementing an
    interface, or of an abstract class; `DelegateForwarder` compiles a delegate. Every call is logged
    as a `MockInvocation`, then answered by the latest matching setup, else by `FluentDefaultProvider`.
    Of `object`'s members only `ToString` is intercepted; it answers with the mocked type's alias.
  - `CallMatcher` — which calls a setup or verification is about: method (through overrides, with
    generic type arguments), property getter or delegate invocation; arguments by value (collections
    by content), `Any<T>()`, `Any<T>(constraint)`; out arguments match anything and get the setup's
    value. Refuses a non-virtual member and Moq's `It.*` with `SetupFailed`.
  - `CallChain` — splits `_ => _.GetChild(2).Get(1)` into its first step and the rest. A receiver
    TSpec does not mock is refused naming the member that returns it ("IParent.Name returns a
    string, which TSpec does not mock, …").
  - `MockChildren` — a mock's chained setups and its children by address; a new child takes the
    matching chained setups of its parent's type, then its parent's own.
  - `AsyncAnswer` — a throw on an awaited call faults the task; a value inside a task is wrapped.
  - `MockRegistry` — one handle per type; `MockingStrategy` — which types are mocked.
- **Verification by expression** counts `CallMatcher` matches in the log (`TestResult.VerifyCall`); a
  logged call keeps what it answered with, so a chain is counted on the mocks its first step actually
  answered with, each once. It fails like a count by name: "Expected IOrderService.CreateOrder(the ShoppingCart) to be invoked
  once but was never invoked", then the calls the mock received (`ReceivedCalls`).
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

3.1.0 is prepared (version, release notes, agent reference) but not packed or published. "Probed"
means observed on the Castle engine; how Moq behaved is inferred where marked, or probed against the
cached Moq 4.20.72 package.

Work test first, stop after each item to report and evaluate, and propose any new user-facing
wording before pinning it. No item is worse than 3.0 any more.

### 2.1 Before publishing

**Goal** (PO, 2026-09-15): 3.1 closes with a package free of Moq that mocks at least slightly better
than Moq did. In order:

Nothing left to build: every item is in Done. Pack and publish per CLAUDE.md.

M5's `Core.Spec`/`Integration.Spec` — 3.1.0 ships without running them (PO, 2026-09-15); the PO runs
them after, and what they find is fixed in 3.1.1.

### 2.2 Could break a 3.0 user's test (regressions against Moq)

None open.

### 2.3 Edges: probed or read, likely harmless

- **An abstract class with no parameterless constructor** throws Castle's `ArgumentException` "Can not
  instantiate proxy of class: X. Could not find a parameterless constructor. (Parameter
  'constructorArguments')", whether the subject depends on it, it is set up, or asked for with `A<X>()`.
  3.0 threw the very same exception: Moq let Castle's through (probed against Moq 4.20.72), so this is
  not worse than 3.0. `Using<X>(instance)` of a hand-written subclass works. Probed. Kept as is for
  3.1; supporting it is §3 item 6.
- **Rendering**: a delegate mock as text is its full type name (`System.Func`2[System.Int32,System.String]`,
  `TSpec.Test.AutoMock.TryLookup`) where an interface mock is its alias (`IMemberKinds`); Moq's was the
  same full name (probed). Kept for 3.1. An abstract class that overrides `ToString` answers with a
  generated string. Read.
- **An unmatched default interface member** answers with TSpec's default instead of running its
  body. Probed; Moq did not run it either (probed against Moq 4.20.72). Pinned in
  `WhenADefaultInterfaceMemberIsMocked` before moving to Castle.Core 5.2.1, whose changelog adds
  default interface methods to proxies without target.
- **A `ref` argument** matches by value and is not written back. Probed; as Moq (probed). Unpinned.
- **An indexer setup** (`That(_ => _[1])`) works. Probed; unpinned.

### 2.4 Decisions and docs

- **Mocking an internal type** needs `[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]` in
  the assembly that declares the type — usually the production assembly, not the test project (in
  `Core.Test` they are the same). Castle emits proxies into its own dynamic assembly and refuses up
  front a type that assembly cannot see: "Can not create proxy for type IHidden because it is not
  accessible…". Moq had the same need. PO (2026-09-15) dislikes it. Spiked: .NET's own
  `DispatchProxy` proxies an internal interface without it — method, generic method, out parameter
  written back, default interface member intercepted, `ToString` overridable — since it lets its
  generated assembly ignore access checks. It proxies interfaces only, so an internal abstract class
  would still need the line. PO (2026-09-15): behave exactly as Moq until the need is understood — and
  3.1 already does: Moq 4.20.72 threw the same `ArgumentException` with the same text, for an internal
  interface and an internal abstract class (probed). No change, no docs.
- **Moq still named on purpose** (PO, 2026-09-15): `CallMatcher`'s refusal of `Moq.It` by name stays
  (without it a leftover `It.IsAny` would match only the default), and so do the release-notes lines
  telling a 3.0 user what changed. Undecided: the README's opening comparison with "plain xUnit with
  Moq".

## 3. Release 3.2+ — the mocking language

Build only on the engine; each item needs a real spec that is worse without it, and each lands on its
own, with the suite green, before the next starts. A setup reads the same whether the member is sync
or async (PO, 2026-09-14), as the rest of TSpec does. Candidates, not yet designed:

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
   - matching any type argument of a generic method (`It.IsAnyType`).
4. **Verification messages that read like TSpec's assertion failures.** The 3.1 wording is a first
   cut, to be tweaked (PO, 2026-09-13). Its listing shows only the verified mock's own calls, so a
   chain's later steps, received by the child's mock, are missing from it.
5. **`Any<T>()` inside an argument, matched the right way** (PO, 2026-09-15: revisit, possibly
   support). `_.Find(new Filter { Id = Any<int>() })` or `_.Sum(new[] { Any<int>(), 2 })` would match
   by structure: each member or element the expression writes either matches its `Any` or is equal.
   3.1 refuses it (see Done). To decide first: whether members the initializer leaves out take part,
   positional records (a constructor argument is not named by a member), and nesting inside a
   constraint's own lambda, which the refusal does not look into.
6. **Mock an abstract class that has no parameterless constructor** (PO, 2026-09-15: support it, in a
   later version). As an input object is made: its constructor with the most parameters, arguments
   generated, handed to Castle's `CreateClassProxy`. No new API. To settle: protected constructors
   (`ConstructorCompiler` looks at public ones only), and a base constructor that rejects generated
   arguments.

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
- **3.1.0, generic type names** — `FluentDefaultProvider`'s refusal when no provided default is most
  specific names types with `Alias()`, pinned in `WhenReturnsAssignableValue`. 2026-09-13.
- **3.1.0, tasks of interfaces** — the refusal "Interface types returned as task must be provided
  explicitly" is gone: it arrived with the Xspec merge, uncommented, and guarded nothing on the new
  engine nor on 3.0's Moq 4.20.72 (only its own specs failed without it). An unset `Task<T>`/`ValueTask<T>`
  member answers as a `T` member does; pinned in `WhenMockReturnTaskOfInterface`,
  `WhenGivenArrayOfModelsAsync` and `WhenAChainGoesThroughATask`. 2026-09-14.
- **3.1.0, delegate out parameters** — `DelegateForwarder` writes by-ref arguments back after the
  call, so a delegate mock sets its out values as an interface mock does; pinned in
  `WhenADelegateIsMocked`. 2026-09-14.
- **3.1.0, `Mock.Get`** — the release notes say a TSpec mock is not a Moq mock, so `Mock.Get` on one
  throws; referencing Moq directly does not bring that back. 2026-09-14.
- **3.1.0, `Any<T>()` across a conversion** — refused with `SetupFailed` when the parameter's type
  cannot hold a `T` (`Any<MyValueInt>()` on `Get(int)`), as Moq refused it ("Matcher … is
  unmatchable"); it had silently matched nothing. Pinned in `WhenMockWithAnyArgument`. 2026-09-14.
- **3.1.0, chained calls** (PO design) — a child per address, the member and the actual arguments,
  made only where a chained setup's first step matches; elsewhere the shared mock of the type. A child
  answers its chained setups, then the type's, then defaults; a chain is counted on the mocks its first
  step answered with. `.Result` steps through a task and is left out of the specification; the renderer
  sees text, not types, so a mocked member really named `Result` after a call is left out too (a flag
  from the expression tree would make it exact). Deliberately unlike Moq 4.20.72 (probed): a child per
  `Any` address, and setups at one address combine. A call left of a dot renders as a call, not a
  phrase. Pinned in `WhenMockingAChainedCall`. 2026-09-14.
- **3.1.0, the verification listing** — every failed count (whole mock, by name, by expression) is
  followed by the calls the mock received, in order ("IOrderService received:" and one call per line,
  or "IOrderService received no calls", which a count of the whole mock leaves out at 0, as the count
  already says it); arguments by `FormatValue`, a property as `.Name` /
  `.Name = "x"`, a generic method with its type arguments, a delegate by its alias; setups not listed.
  PO wording: a count of 0 reads "was never invoked", of 1 "was invoked once". Pinned in
  `WhenAVerificationFails` and the ShoppingService count specs. 2026-09-14.
- **3.1.0, nested `Any` refused** — `Any<T>()` or `Any<T>(constraint)` anywhere inside an argument,
  rather than as it, throws `SetupFailed`: "Any<int>() matches a whole argument, not a part of one, so
  inside the cart argument of IOrderService.CreateOrder it can match nothing. Write Any<ShoppingCart>()
  for any ShoppingCart, or Any<ShoppingCart>(cart => ...) for any ShoppingCart satisfying a condition". It had been
  evaluated as one generated value while the specification read "any int". Neither version ever meant
  any value: on 3.0 (probed against Moq 4.20.72) it matched only `default`, a nested constraint was
  ignored, and a failed Moq verification crashed formatting its message. `Any<T>(setup)` yields a
  value and is not refused. Pinned in `WhenAnyIsNestedInsideAnArgument` and
  `WhenAnyIsNestedInsideAVerifiedArgument`. Supporting it is §3 item 5. 2026-09-15.
- **3.1.0, a tap before `First()`** (PO: fires on every call) — `That(…).Tap(a).First()…` had dropped
  `a` from the call and the specification, exactly as 3.0 did (probed on a build of `2a9a985`: its
  sequence's Moq callback replaced the tap's). `First()` now hands the taps in hand to
  `MockCallSequence`, which runs them before every step, past the last one too; the opening step
  states them, then "first" as a word of its own: "Given IMyValueIntRepo.Get(any int) tap(_asked.Add)
  first returns "a"". With "first" no longer appended to the call's source, a sequence on a call
  with `Any<int>()` reads "any int" instead of the raw `Any<int>()` it showed on 3.0 and 3.1. README
  §4.5 and the agent reference say it. Pinned in `WhenTapASequence`. 2026-09-15.
- **3.1.0, Moq leftovers and the three frameworks** — the `moq` package tag, `Generic.cs` reading
  `It.IsAny<T>()` as `Any` and `NormalizeTimes` stripping Moq's `Times.Once()` are gone. Suite green on
  net8.0, net9.0 and net10.0 (1935); MyHotel's `MyHotel.Spec` (52) and `Core.Spec` (32) green with
  their `_specification/` unchanged. 2026-09-15.
- **3.1.0, Castle.Core 5.2.1** (PO) — changelog read: generic `CreateClassProxy<TClass>` overloads,
  default interface methods in proxies without target, nullable annotations, two by-ref/`out`
  parameter bugfixes. Suite green on all three frameworks (1937), MyHotel green with its
  `_specification/` unchanged. 2026-09-15.
- **3.1.0, a `Task` call past a sequence's last step** (PO) — answered a null task on 3.0 and 3.1,
  which throws `NullReferenceException` when awaited; it now answers as `Returns()` does, with a
  completed task (`MockCallSequence.TryNext`). `Task<T>` and `ValueTask` already completed. Pinned in
  `WhenCallAsyncActionTwice.GivenCalledPastTheLastStep`. 2026-09-15.
- **3.1.0, a call to a mocked delegate in text** (PO) — read broken on 3.0 and 3.1 (probed on both):
  "Given Func<int, string>. 1 returns "one"", "Given TryLookup._(1, out _found) returns true", and the
  same in a verification and its failure message. `CallDescriber` reads `_(…)` as the delegate
  invoked, and `ExpressionDescriber.MockCallBinder` joins a delegate to its call with nothing instead
  of a dot, in setups, verifications and the failure message alike: "Given Func<int, string>(1)
  returns "one"", "Then Func<int, string>(1)". A second setup on the same delegate names it
  again ("and Func<int, string>(2) returns "two""), since "(2)" cannot stand alone. Pinned in
  `WhenADelegateCallIsSpecified`. 2026-09-15.
- **3.1.0, an argument that reads the mock** (PO: refuse) — `_ => _.Get(_.Id)` threw a raw
  `InvalidOperationException` from compiling the argument, as Moq did (probed). `CallMatcher` now
  refuses it: "The id argument of IStore.Get reads IStore.Id from the mock itself, but arguments are
  read before any call is made. Set up IStore.Id to return a value, and write that value as the
  argument" — naming the first member the argument reads on the mock, also inside a larger
  expression and on a parent reached through a chain. Supporting it was weighed and not taken: rarely
  needed, since the test sets up what `_.Id` returns; and reading it while matching would be a call on
  the mock that counts see. Not refused: an argument that is the mock itself (`_ => _.Compare(_)`),
  which still throws the raw exception. Pinned in `WhenAnArgumentReadsTheMock` and
  `WhenAChainedArgumentReadsTheMock`. 2026-09-15.
- **3.1.0, the way out through `Mock.Get`** (PO: document, don't build) — on 3.0 `Mock.Get(The<IFoo>())`
  returned the `Mock<IFoo>` TSpec had made, and a Moq setup on it reached the subject (probed on a
  build of `2a9a985`), so a test could use any Moq feature. It was never TSpec's API: supported in the
  early XspecT days, since then working by accident — no Moq type public since at least 2.0, no
  README mention. On 3.1 it throws. README §4.7 (not the agent reference, PO) says to make such a
  mock with another library, hand it in with `Using`, and arrange and verify it with that library.
  Left unsaid (PO): `Given<IFoo>()` and `Then<IFoo>(…)` on that type reach TSpec's own mock, which the
  subject no longer receives (probed). The release notes' Moq line points to `Using`. 2026-09-15.
- **3.1.0, a throw on an awaited call faults the task — checked against the sync/async promise.**
  Probed on 3.0 and 3.1 with non-async test methods: `Then().Throws<E>()` passes on both whether the
  subject awaits the mocked call or hands its task straight back, for `Task` and `Task<T>`. Only a
  subject that starts two calls before awaiting either tells them apart: on 3.0 the first threw at
  the call, so the second was never made; on 3.1 both are made, as with a real async implementation.
  2026-09-15.
