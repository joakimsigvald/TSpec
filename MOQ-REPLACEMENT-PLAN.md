# Moq replacement plan

TSpec mocks with its own engine on Castle.Core since 3.0.0 (published 2026-09-15); 3.0.1 carries the
fixes from upgrading production projects. 3.1.0 (shipped 2026-09-19, PO) closes section A as triaged
and mocks classes: set up, without a parameterless constructor, and through a chain. What remains is
closing the gap to Moq, and past it where TSpec owns the mocking layer. Written for a Claude session in this repository; a living document —
correct it in place as work lands. Strike a finished item through in §3, and add it to Done in one
or two lines.

## 1. The engine

- **Where it lives**: `Core/Internal/TestData/Generation/Strategies/Mocking/`.
  - `MockHandle` — one mock, receiving each call in three steps: `SetupGuard` refuses what a setup
    lambda may not do, `CallLog` records it as a `MockInvocation` with the pipeline phase it was made
    in (and counts, chains included), `CallSetups` answers with the latest matching setup, else
    `FluentDefaultProvider` does. A property comes before the defaults: `PropertyValues` keeps one
    value per address, taking what a set writes and what the first read generated, and
    `PropertyAccess` tells the accessors apart. Only calls made from `Act` on are counted. `MockInstance` makes the
    instance: a Castle proxy of `object` implementing an interface, or of a class, or a delegate
    `DelegateForwarder` compiles; of `object`'s members only `ToString` is intercepted, answering
    with the type's alias.
  - `CallMatcher` — which calls a setup or verification is about: a method (through overrides, with
    generic type arguments), one predicate per argument, and out values, which match anything and are
    handed back. `CallReader` reads the lambda: a method, property getter or delegate invocation on
    the service, refusing a non-virtual member. `ArgumentMatcher` makes each predicate: by value
    (collections by content), `Any<T>()`, `Any<T>(constraint)`. `ArgumentRefusals` refuses first
    Moq's `It.*`, an `Any` converted to a type that cannot hold it, an `Any` inside an argument
    (`NestedAny`), an argument that reads the mock (`MockRead`).
  - `CallChain` splits `_ => _.GetChild(2).Get(1)` into its first step and the rest; `MockChildren`
    holds a mock's chained setups and its children by address; `AsyncAnswer` faults a task on a throw
    and wraps a value in one; `MockRegistry` keeps one handle per type; `MockingStrategy` decides which
    types are mocked; `ReceivedCalls` lists a mock's calls for a failed verification.
  - Outside the folder: `Pipelines/MockCallSequence` (a sequence's steps and its taps),
    `Pipelines/ProtectedMember` (`ThatProtected`), `ExpressionDescriber.MockCallBinder`, and
    `TestResult.VerifyCall`, which counts `CallMatcher` matches in the log — a chain on the mocks its
    first step actually answered with.
- **Pinned, so an engine change cannot drop it silently**: the order unmatched calls are answered in
  (`WhenReturnsDefaultValue`, `WhenMockReturnsSelf`, `WhenValueTaskOfInterface`), async throws fault
  (`WhenAMockedAsyncCallThrows`), and every member kind and unstated Moq behaviour
  (`WhenMockingEachMemberKind`).
- **Moq left in the repository on purpose**: `Core.Test/AutoMock/MoqIt.cs` stands in for `Moq.It` so
  the refusal can be tested; `CallMatcher` refuses `Moq.It` by name, and the release notes tell an
  upgrading user what changed (PO, 2026-09-15). The package references no Moq.
- **Probing**: the current engine with a throwaway spec in `Core.Test`, deleted after. Moq 4.20.72
  with a console probe on the cached package; the unpublished Moq-based 3.0 from `git archive
  2a9a985`, with a probe test dropped into its `Core.Test`.
- Inside any `TSpec.*` namespace a bare `Times` binds to `TSpec.Times`.

## 2. How to work an item

- Before any code, one proposal: the failing example, the intended behaviour, any new user-facing
  wording, and the docs line if a reader would otherwise get something wrong. The PO approves once.
- Then test first, implement, and refactor what was touched — names included. Report once: what
  changed, test results, decisions left. Stop there; the next item starts on the PO's word.
- Each item lands on its own: suite green on all three frameworks, MyHotel green.
- A setup reads the same whether the member is sync or async (PO, 2026-09-14).
- Probe a mocking change against a property, a generic method, an `out` parameter, an overload set, a
  non-virtual member and a `Task`-returning void — the six that broke the reverted by-name attempt
  (improvement plan, 2026-09-10).

## 3. The gap, in priority order

Triaged 2026-09-19 against one rule: does TSpec state something untrue, or can a real spec not be
written? A holds what is untrue, B what is decided, C what waits for evidence: a real spec from Cdr
or M5 that is worse without it. Done items are struck through.

### A. Untrue today

1. ~~**A concrete class that is set up is ignored.**~~ Done in 3.1.0, with item 5.
2. ~~**Calls made while arranging are counted.**~~ Done in 3.1.0.
3. ~~**Mocking properties.**~~ Done in 3.1.0, see Done. The 3.1.0 rule — a mock assumes nothing about
   a property the test did not arrange, no stored set, no stable value — is overturned by item 19 (PO,
   2026-09-20). Not taken (PO): a set through a chain, `Set(_.Child.Name, …)`, since
   `Given<IChild>().That(_ => Set(_.Name, …))` reaches the child more simply. A protected property's
   setter, which `ThatProtected` cannot name, is left to the by-name rework, item 9 (PO: not
   important).
4. ~~**A service-wide default that no member answers with.**~~ Not taken (PO, 2026-09-19): the
   default is valid, e.g. in a base test class, and one the type has no use for is unhelpful rather
   than wrong, which is the developer's to see, not the framework's to prevent.
20. **A chain through a class that is only verified** (found after 3.1.0). With nothing set up
    through it, the first step answers with a real instance, as a class is mocked only once set up,
    so `Then<IClientFactory>(_ => _.Get("a").Fetch(), Once)` fails "never invoked", and with `Never`
    passes whatever the subject did (probed). Proposed, undecided: refuse the verification where a
    step it goes through answered with a real instance, naming the setup to write, e.g.
    `Given<IClientFactory>().That(_ => _.Get("a").Fetch())`. Not taken: answering an unmatched call
    that returns a class with a mock, which would mock every class a mock returns, data included.

### B. Decided, to build

5. ~~**Mocking a class, continued.**~~ Done in 3.1.0, see Done; what is left of it is item 20.
6. **Several mocks of one type.** README: "Distinct mentions get distinct values where the type has
   room for them" — but `A<IRule>()` and `ASecond<IRule>()` are the same mock, and an
   `IEnumerable<IRule>` constructor parameter gets an empty collection (probed). Composites, validator
   lists and pipeline behaviours are everyday DI. To design: how a setup or verification addresses one
   of them; `Then<IRule>` counts them all, as it does chain children.
19. ~~**A property keeps its value.**~~ Done in 3.2.0, see Done. A getter that yields different values
    stays a setup, `Returns(() => _next++)` or `First()…AndNext()`; no verb was added, since `Get` and
    `Set` earn their place on a gap this has not — an expression can state neither a read as a
    statement nor an assignment at all.

### C. On evidence

7. **Microsoft's `ILogger`.** `LogError(…)` is an extension over `Log<TState>` with an internal state
   type, so only a count by name can reach it, not level or message. Moq needs `It.IsAnyType`. Neither
   accepts the extension call itself: `Then<ILogger<OrderService>>(_ => _.LogError(Any<Exception>(),
   Any<string>()))`, translated by TSpec. Ask whether Cdr or M5 verify logging.
8. **Assert on what a call received** (neither has it). Today `Tap` into a field and `Then(field)`, or
   `Any<T>(constraint)`, whose failure says only "never invoked". MyHotel's
   `Then<IBookingStore>(nameof(Save), Once)` is a candidate for a stronger claim.
9. **Set up a call by name, in the general case**: `Given<IChat>().That(nameof(IChat.Complete))`.
   Decided in the improvement plan: the return type matches exactly; a name covers every overload; a
   from-arguments `Returns` narrows it to one; it renders "Given IChat.Complete returns …";
   `ThatProtected` folds into `That`; it carries "any type argument" (item 7) in a setup.
10. **Raise an event on a mock** (Moq's `Raise`, `Raises`). Subscribing is logged as a call, nothing
    more.
11. **A partial mock that runs the real member** (Moq's `CallBase`): an abstract class's template
    method as the subject, and default interface members.
12. **Failure messages** (PO, 2026-09-13: the 3.0 wording is a first cut). Needs a concrete message
    that reads badly. Ideas: a chain's later steps in the listing, the argument that differed, setups
    no call matched.
13. **Call order across mocks** — "saved before published". Needs a sequence number shared by every
    mock's log.
14. **No other calls** (Moq's `VerifyNoOtherCalls`).
15. **`because` on a verification** (Moq's `Verify(…, failMessage)`).
16. **A mock implementing further interfaces** (Moq's `As<TInterface>()`).
17. **A service-wide sequence**: `Given<IChatCompletion>().First().Returns(…).AndNext()…`.
18. **`Any<T>()` inside an argument, matched by structure** (PO, 2026-09-15: revisit, possibly
    support). To decide: members an initializer leaves out, positional records, nesting inside a
    constraint's own lambda.
21. **One answer per address, for every unmatched call** (found and probed 2026-09-22). A mock is two
    things at once today: a call returning a value generates a fresh one every time, whatever its
    arguments — `GetString(1)` twice answers "String1" then "String2" — while a call returning a
    mockable type answers every call with the one type-level mock, so `GetChild(1)` and `GetChild(2)`
    are the same child, and a child per address appears only where a chained setup was made. Proposed:
    a mock answers the same address with the same thing, value or child, which makes item 19's
    property slot the no-argument case of one rule, and dissolves item 20, since a step of a chain
    would reach a child of its own with nothing set up. To weigh: every unmatched call returning a
    mockable type then makes a child, which item 20 declined for classes; `Then<IChild>` would count
    per-address children where it counts one mock today; and a value per address no longer tells two
    calls apart in a failure listing. Item 6 asks the same question of a type with several mocks.

**Not taken:**
- `Verifiable`/`VerifyAll`: verification belongs in `Then`.
- `Mock.Of`'s LINQ form: `Given<T>().That(…)` covers it.
- Conditional setups; `MockRepository`.
- Custom default providers: `Using` covers them.
- `Protected().As<>()`: item 9 covers it.
- `Throws` from the call's arguments: `Throws(() => new NotFound(The<int>()))` states the same.
- `ref` arguments matched as any and written back: as Moq, and no need seen.
- Delayed async answers: a `TaskCompletionSource` returned as the task does it.
- From-arguments `Returns` on a sequence step (improvement plan item 7, dropped 2026-09-11); reopen
  only if the engine makes it free.

**Parked here, not mocking:** a pipeline timeout — Cdr hand-rolls `Patience` constants; check xUnit's
`[Fact(Timeout)]` on synchronous test methods first.

## 4. Edges and decisions, kept as they are

- **Internal types** need `[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]` in the assembly
  that declares them, as Moq did (PO, 2026-09-15: behave as Moq until the need is understood). A
  `DispatchProxy` spike would lift it for interfaces only. No docs.
- **Rendering**: a delegate mock as text is its full type name where an interface mock is its alias,
  as Moq's was; an abstract class overriding `ToString` answers a generated string. `.Result` in a
  chain is left out of the specification by text, so a mocked member really named `Result` after a
  call is left out too.
- **An unmatched default interface member** answers TSpec's default without running its body, as Moq
  did; pinned in `WhenADefaultInterfaceMemberIsMocked`. Running it is item 11.
- **Verifying a mock the subject never received** counts the calls it received, none, so
  `Then<IFoo>(wasInvoked: Never)` passes as specified. No claim is made that the subject held the
  mock (PO, 2026-09-19).
- **An argument that is the mock itself** (`_ => _.Compare(_)`) still throws the raw
  `InvalidOperationException`; raise only if asked.
- **A mock from another library**: README §4.7 says make it there, hand it in with `Using`, arrange and
  verify it there. Left unsaid (PO): `Given<IFoo>()`/`Then<IFoo>()` then reach TSpec's own mock, which
  the subject no longer receives.
- Undecided: the README's opening comparison with "plain xUnit with Moq".

## Done

- **2.8.0** — `TSpec.Times` replaces `Moq.Times` in the public API (`Once`, `Never`, `AtLeastOnce`,
  `AtMostOnce`, `Exactly`, `AtLeast`, `AtMost`, `Between`). 2026-09-13.
- **3.0.0** — obsolete and unreachable surface deleted; `SomeOther<T>()` kept. 2026-09-13.
- **3.0.0** — the engine: a seam over Moq, then Castle.Core behind a switch until the failure count hit
  0, then Moq's files and package deleted (PO: `It.*` refused; a throw on an awaited call faults the
  task). Castle.Core 5.2.1. 2026-09-13–15.
- **3.0.0** — tasks of interfaces answer as their value type; delegates write back out arguments; an
  `Any` that its parameter cannot hold is refused; generic type names in the no-most-specific-default
  refusal. 2026-09-14.
- **3.0.0** — chained calls (PO design): a child per address, counted on the mocks the first step
  answered with; deliberately unlike Moq, a child per `Any` address and setups at one address
  combine. Pinned in `WhenMockingAChainedCall`. 2026-09-14.
- **3.0.0** — a failed verification lists the calls the mock received; "was never invoked", "was
  invoked once" (PO). Pinned in `WhenAVerificationFails`. 2026-09-14.
- **3.0.0** — refused with `SetupFailed`: an `Any` nested inside an argument, and an argument that
  reads the mock (supporting it weighed and not taken). 2026-09-15.
- **3.0.0** — a tap before `First()` taps every call and is stated ("tap(_asked.Add) first returns");
  a `Task` call past a sequence's last step answers a completed task; a delegate call reads
  "Func<int, string>(1)". 2026-09-15.
- **3.0.0** — `Mock.Get` on a TSpec mock throws; README §4.7 points to another library with `Using`
  (PO: document, don't build). A throw on an awaited call checked against the sync/async promise.
  Moq leftovers removed; published 2026-09-15. The local `v3.0.0` tag still points at `d6ec7de`, the
  Moq build.
- **Production upgrades** — M5: 1276 green, the one break `using static Moq.Times`. Cdr: the .NET 10
  SDK needs the MTP opt-in in `global.json`, and then refuses VSTest options such as `--logger trx`.
- **3.0.1** — a service-wide `Returns`/`Throws` counts as arranging the mock
  (`WhenAnOptionalDependencyIsArranged`); `Then(x)` refuses a value type handed over before the
  pipeline runs (`WhenAValueIsHandedOverBeforeThePipelineRuns`); a plain-type verification counting
  chain mocks documented, a delegate-started chain pinned; a task that completes later pinned
  (`ValueTask<T>` likely, unpinned). 2026-09-15.
- **3.1.0** — a class set up with `Given<T>()` is mocked, the subject included (PO); a sealed one is
  refused. Pinned in `WhenAClassIsSetUp`. 2026-09-19.
- **3.1.0** — mock calls made while arranging are not counted; the subject is built as the last step
  of arranging, so its constructor's calls are not either (PO). A chain reached while arranging still
  leads on. Pinned in `WhenCallsAreMadeWhileArranging` and `WhenAChainIsReachedWhileArranging`.
  2026-09-19.
- **3.1.0, refactor** — one pipeline phase, `Declare → Arrange → Act → Assert`, advanced only by the
  `Fixture` and read through `IPipelinePhase`; running a pipeline again after it failed throws an
  `InvalidOperationException` pointing to GitHub issues (PO). Mock logs are `ConcurrentQueue`s.
  2026-09-19.
- **3.1.0** — a property or indexer set on any mock while a setup lambda (`A`…, `One`…, `Any`,
  `Using`) runs is refused, naming `That(…).Returns(…)` with the value where it is a literal, else
  `…` (PO: sets only, other calls if a real spec shows them). Pinned in
  `WhenASetupSetsAPropertyOnAMock` and `WhenAUsingSetupSetsAPropertyOnAMock`. 2026-09-19.
- **3.1.0** — `Using<T>(setup)` on an interface runs on its mock; it handed the subject null, as
  building a value for a `Using` setup turned mocking off. Only `MockingStrategy` decides what is
  mocked (PO). 2026-09-19.
- **3.1.0** — verifying by name refuses a property or field ("IMemberKinds.Name is a property;
  fields and properties cannot be verified by name"; PO: a name does not say which accessor) and a
  name of no method ("IMemberKinds has no method Nme"); accessor names such as `set_Name` still
  count. Pinned in `WhenVerifyingByName`. 2026-09-19.
- **3.1.0** — a new phase, `Declare → Arrange → Mock → Act → Assert` (PO: more may hang on it): values
  are arranged, then mocks set up, as a mock setup reads the values it is given; the subject is built
  last in Mock. A setup lambda reading a mock's property before Mock is refused, suggesting a shared
  value, `That(_ => _.Name).Returns(() => The<string>())` (PO: making both orders work is the mud).
  Pinned in `WhenASetupReadsAMock`. 2026-09-19.
- **3.1.0** — a set is set up and verified as `Set(_.Name, value)`, a marker `CallReader` reads as the
  setter, since an expression cannot assign; the value and an indexer's index match as arguments,
  `Any` included. Reads "Given IIdSource.Name = "" throws ArgumentException", "Then IIdSource[1] =
  "x" was invoked once"; an indexer getter now reads `IIdSource[1]` too. Refused: a property with no
  setter; `Set` called. Not taken (PO): `ThatSet(_ => _.Name, value)` and `That(_ => _.Name).Set(…)`,
  whose value, outside the expression tree, would make `Any` a generated value; Moq's plain lambda
  against a recording mock. One line in each doc. Pinned in `WhenAPropertySetIsMocked`. 2026-09-19.
- **3.1.0** — a read is verified as `Then<IIdSource>(_ => Get(_.Name), Once)`, one type argument
  where `Then<IIdSource, int>(_ => _.NextId)` took two: a read is no statement, so its lambda could
  only be a `Func`, and a `Then<TService>(Func<TService, object?>)` overload would have drawn every
  non-void method call. `Get` in a setup is refused for `That(_ => _.Name)`, which can answer (PO).
  The two-argument form still works, undocumented. Pinned in `WhenAPropertyReadIsVerified`.
  2026-09-19.
- **3.1.0** — a class with no parameterless constructor is mocked: a parameterless one, public or
  protected, is still used where there is one, as a class built to be mocked keeps it for that; else
  the greediest, protected ones included, as the mock is a subclass (`ConstructorCompiler.GetForMock`).
  Its arguments are filled as the subject's are, a default kept unless the test arranged the type,
  by `ConstructorArguments`, extracted from `ObjectStrategy` for both (PO: reuse). A constructor
  rejecting them is refused: "Provide ThreeLetterCode with Using instead of a mock: its constructor
  threw ArgumentException for the arguments TSpec generated, because: …". A release-notes line only.
  Pinned in `WhenAClassHasNoParameterlessConstructor`. 2026-09-19.
- **3.1.0** — a chain goes through any class that is not sealed (`MockingStrategy.IsMockable`), as
  the Azure clients need; a sealed one, `string` included, is still refused. A chain setup does not
  make the class mocked elsewhere: one handed to the subject directly stays real (probed). Pinned in
  `WhenAChainGoesThroughAClass`. 2026-09-19.
- **3.2.0** — a property keeps its value: a setup answers every read, else the last set, else what the
  first read answered, one value per indexer address and per chain child; the slot is live from the
  first read, arranging included. A set in a setup lambda is kept, so the 3.1.0 refusal is gone, while
  a read there is still refused. Pinned in `WhenAPropertyKeepsItsValue` and
  `WhenASetupSetsAPropertyOnAMock`. 2026-09-22.
