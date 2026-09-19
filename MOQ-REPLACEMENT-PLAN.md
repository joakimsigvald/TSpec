# Moq replacement plan

TSpec mocks with its own engine on Castle.Core since 3.0.0 (published 2026-09-15); 3.0.1 carries the
fixes from upgrading production projects. What remains is closing the gap to Moq, and past it where
TSpec owns the mocking layer. Written for a Claude session in this repository; a living document —
correct it in place as work lands, and move a finished item to Done as one or two lines.

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

- Test first; stop after each item to report and evaluate; propose new user-facing wording before
  pinning it.
- Each item needs a real spec that is worse without it, and lands on its own, suite green on all three
  frameworks and MyHotel green, before the next starts.
- A setup reads the same whether the member is sync or async (PO, 2026-09-14).
- Probe a mocking change against a property, a generic method, an `out` parameter, an overload set, a
  non-virtual member and a `Task`-returning void — the six that broke the reverted by-name attempt
  (improvement plan, 2026-09-10).

## 3. The gap, in priority order

From reading Moq 4.20, NSubstitute and the engine, and a probe of the current engine (2026-09-19,
net10.0). Silent wrong answers first, then what backend code needs daily, then parity. Items 1–5 are
defects; for 6–8, evidence from Cdr or M5 before designing.

### A. Silently wrong

1. **A concrete class that is set up is ignored.** Done, see Done; what is left of it is item 19.
2. **A mock's property does not keep what is set.** A set is swallowed, and an unset getter answers a
   new value on every read ("String1|String2", probed). So `A<IOrderLine>(_ => _.Quantity = 3)` does
   nothing, and nothing says so. Moq has `SetupProperty`/`SetupAllProperties`; NSubstitute stores sets
   by default. A getter answering the last set value, else one stable default, would also make
   interface-typed models usable as test data.
3. **Verification by name misses properties.** `Then<IProbe>(nameof(IProbe.Name), Once)` fails with
   "never invoked" above a listing of `IProbe.Name = "x"` and `IProbe.Name` (probed):
   `TestResult.VerifyInvoked` compares `Method.Name`, which is `set_Name`/`get_Name`. A set cannot be
   verified with its value at all (Moq's `VerifySet`), since an expression tree cannot assign.
4. **Verifying a mock the subject never received passes.** `Then<INotInjected>(wasInvoked: Never)` is
   vacuous; after 3.0.1's item 1 the remaining silent case. May break existing tests.
5. **Calls made while arranging are counted.** A `Having` that drives the subject, as MyHotel's
   `ButIsAlreadyBooked` does, adds its calls to every count: `Then<IProbe>(_ => _.Get(Any<int>()),
   Once)` fails on `Get(1)` from `Having` and `Get(2)` from `When` (probed). Moq users clear
   `Invocations`; TSpec has no lever. To decide: whether a verification counts the act only.

### B. Common needs TSpec cannot express

6. **Several mocks of one type.** `A<IRule>()` and `ASecond<IRule>()` are the same mock, and an
   `IEnumerable<IRule>` constructor parameter gets an empty collection (probed). Composites, validator
   lists and pipeline behaviours are everyday DI; a delegate factory is the only way out today. To
   design: how a setup or verification addresses one of them, and that `Then<IRule>` counts them all,
   as it does chain children.
7. **Microsoft's `ILogger`.** `LogError(…)` is an extension over `Log<TState>` with an internal state
   type, so neither a setup nor a verification can name it; by name it is counted (a name covers every
   type argument, probed) but level and message cannot be checked. Moq needs `It.IsAnyType` — match any
   type argument of a generic method. Neither Moq nor TSpec accepts the extension call itself:
   `Then<ILogger<OrderService>>(_ => _.LogError(Any<Exception>(), Any<string>()))`, translated by TSpec.
8. **Assert on what a call received** (neither has it). Today: `Tap` into a field and `Then(field)`
   (refused for a value type since 3.0.1), or `Any<T>(constraint)`, whose failure says only "never
   invoked". MyHotel's `Then<IBookingStore>(nameof(Save), Once)` is where a stronger claim about the
   saved booking belongs. Shape to design; it must read in the specification as an assertion.
9. **Set up a call by name, in the general case.** `Given<IChat>().That(nameof(IChat.Complete))
   .Returns(…)` for public members too. Decided in the improvement plan: the return type matches
   exactly; a name covers every overload; a from-arguments `Returns` states a signature and narrows the
   name to that overload; it renders "Given IChat.Complete returns …". `ThatProtected` then folds into
   `That` and is obsoleted; sequences by name fall out; it carries "any type argument" (item 7) in a
   setup. `CallMatcher.For(MemberInfo)` already matches a member with any arguments.
10. **Raise an event on a mock** (Moq's `Raise`, and `Raises` on a setup). Subscribing is logged as a
    call today, nothing more.
11. **A partial mock that runs the real member** (Moq's `CallBase`): an abstract class's template
    method as the subject, and default interface members, which answer TSpec's default today.
12. **`Throws` from the call's arguments.** `Returns` takes up to five; `Throws` none (Moq:
    `Throws((int id) => new NotFound(id))`).

### C. Verification

13. **Failure messages that read like TSpec's assertion failures.** The 3.0 wording is a first cut, to
    be tweaked (PO, 2026-09-13). The listing shows only the verified mock's own calls, so a chain's
    later steps are missing. Beyond Moq: mark which argument differed (NSubstitute does), and name
    setups no call matched — an argument mismatch is the usual reason a mock "does not work".
14. **Call order across mocks** — "saved before published". Moq has only setup-side `MockSequence`,
    NSubstitute `Received.InOrder`. Needs a sequence number shared by every mock's log.
15. **No other calls** (Moq's `VerifyNoOtherCalls`, strict mocks). `wasInvoked: Never` covers only a
    mock left untouched.
16. **`because` on a verification** (Moq's `Verify(…, failMessage)`). `because` reaches assertions only.

### D. Parity, rarely needed

17. **A mock implementing further interfaces** (Moq's `As<TInterface>()`), for a subject that checks
    `is IDisposable`.
18. **`ref` arguments**: match any and write a value back (today matched by value, not written back,
    as Moq; unpinned); out values computed from the arguments (today fixed at setup).
19. **Mocking a class, continued.** A class with no parameterless constructor, abstract or set up (PO,
    2026-09-15: support it): today Castle's `ArgumentException` "Can not instantiate proxy of class …
    Could not find a parameterless constructor", as Moq 4.20.72 threw; `Using<X>(subclassInstance)`
    works. As an input object is made: the constructor with the most parameters, arguments generated,
    handed to `CreateClassProxy`. To settle: protected constructors (`ConstructorCompiler` sees public
    ones only), and a base constructor that rejects generated arguments. And a chain through a class —
    the Azure clients' `GetBlobContainerClient(…).GetBlobClient(…)` — refused today: "IClientFactory.Client
    returns a VirtualClient, which TSpec does not mock" (probed).
20. **A service-wide sequence.** `Given<IChatCompletion>().First().Returns(…).AndNext()…` with no call
    named; the engine owns defaults, so it need not enumerate methods.
21. **`Any<T>()` inside an argument, matched by structure** (PO, 2026-09-15: revisit, possibly
    support). `_.Find(new Filter { Id = Any<int>() })`: each member or element the expression writes
    matches its `Any` or is equal; refused today. To decide: members the initializer leaves out,
    positional records, and nesting inside a constraint's own lambda, which the refusal does not see.
22. **Delayed async answers** (Moq's `ReturnsAsync(value, delay)`). Today a `TaskCompletionSource`
    returned as the task does it.

**Not taken**: `Verifiable`/`VerifyAll` (verification belongs in `Then`); `Mock.Of`'s LINQ form (item 2
covers it through `A<T>(setup)`); conditional setups; `MockRepository`; custom default providers
(`Using` covers them); `Protected().As<>()` (item 9 covers it). Dropped, reopen only if the engine
makes it free: from-arguments `Returns` on a sequence step (improvement plan item 7, 2026-09-11).

**Parked here, not mocking**: a pipeline timeout — Cdr hand-rolls `Patience` constants; check xUnit's
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
- **An indexer setup** (`That(_ => _[1])`) works; unpinned.
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
