# Moq replacement plan

TSpec mocks with its own engine on Castle.Core since 3.0.0 (published 2026-09-15); 3.0.1 carries the
fixes from upgrading production projects. What remains is closing the gap to Moq, and past it where
TSpec owns the mocking layer. Written for a Claude session in this repository; a living document —
correct it in place as work lands. Strike a finished item through in §3, and add it to Done in one
or two lines.

## 1. The engine

- **Where it lives**: `Core/Internal/TestData/Generation/Strategies/Mocking/`.
  - `MockHandle` — one mock. Castle makes the instance (a proxy of `object` implementing an interface,
    or of a class); `DelegateForwarder` compiles a delegate. Every call is logged as a
    `MockInvocation`, then answered by the latest matching setup, else by `FluentDefaultProvider`.
    Of `object`'s members only `ToString` is intercepted, answering with the type's alias.
  - `CallMatcher` — which calls a setup or verification is about: method (through overrides, with
    generic type arguments), property getter or delegate invocation; arguments by value (collections
    by content), `Any<T>()`, `Any<T>(constraint)`; out arguments match anything and get the setup's
    value. Refuses with `SetupFailed` a non-virtual member, Moq's `It.*`, an `Any` converted to a type
    that cannot hold it, an `Any` inside an argument (`NestedAny`), an argument that reads the mock
    (`MockRead`).
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

1. ~~**A concrete class that is set up is ignored.**~~ Done in 3.1.0; continued in item 5.
2. ~~**Calls made while arranging are counted.**~~ Done in 3.1.0.
3. **Mocking properties.** Design the parts together (PO, 2026-09-19).
   - Rule (PO): a mock assumes nothing about a property the test did not arrange — no stored set, no
     stable value, since `NextId` may answer anew on every read.
   - Works (probed): a getter through `That(_ => _.NextId)` with `Returns`, `Throws`, `Tap` and
     `First`/`AndNext`; an indexer getter through `That(_ => _[1])`, unpinned.
   - Untrue: a set the test makes on a mock is ignored while the specification states it.
     `A<IIdSource>(s => s.Name = "arranged")` reads "Given a IIdSource with Name = "arranged"", and
     the getter answers "String1". Refuse it, pointing at `That(_ => _.Name).Returns(…)`.
   - Untrue: `Then<IIdSource>(nameof(IIdSource.NextId), Once)` fails "never invoked" above a listing
     of `IIdSource.NextId`; `TestResult.VerifyInvoked` compares `Method.Name` (`get_NextId`).
   - On evidence: a setter setup, since an expression tree cannot assign (Moq's `SetupSet` runs a
     plain lambda against a recorder), e.g. `ThatSetting(_ => _.Name)`, where matching the value set
     is the hard part; verifying a set with its value, today only `Then<IIdSource>("set_Name", Once)`;
     `Then<IIdSource>(_ => _.NextId)`, which does not compile since a property read is not a
     statement, so it takes `Then<IIdSource, int>(…)`.
4. **A service-wide default that no member answers with.** `Given<PlainClient>().Returns(() =>
   "mocked")` on a class whose `string` members are not virtual reads "Given PlainClient returns
   "mocked"", and they answer "real" (probed). To decide: refuse a default no interceptable member of
   the mocked type returns, for an interface too, where it is vacuous rather than untrue.

### B. Decided, to build

5. **Mocking a class, continued** (PO, 2026-09-15: support it).
   - A class with no parameterless constructor, abstract or set up: today Castle's
     `ArgumentException` "Can not instantiate proxy of class … Could not find a parameterless
     constructor", as Moq 4.20.72 threw; `Using<X>(subclassInstance)` works. As an input object is
     made: the constructor with the most parameters, arguments generated, handed to
     `CreateClassProxy`. To settle: protected constructors (`ConstructorCompiler` sees public ones
     only), and a base constructor that rejects generated arguments.
   - A chain through a class — the Azure clients' `GetBlobContainerClient(…).GetBlobClient(…)` — is
     refused, untruly since 3.1: "IClientFactory.Client returns a VirtualClient, which TSpec does not
     mock" (probed).
6. **Several mocks of one type.** README: "Distinct mentions get distinct values where the type has
   room for them" — but `A<IRule>()` and `ASecond<IRule>()` are the same mock, and an
   `IEnumerable<IRule>` constructor parameter gets an empty collection (probed). Composites, validator
   lists and pipeline behaviours are everyday DI. To design: how a setup or verification addresses one
   of them; `Then<IRule>` counts them all, as it does chain children.

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
  leads on. Pinned in `WhenCallsAreMadeWhileArranging`. 2026-09-19.
