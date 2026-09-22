# Moq replacement plan

TSpec mocks with its own engine on Castle.Core since 3.0.0; 3.2.0 shipped 2026-09-22. What remains is
closing the gap to Moq, and passing it where TSpec owns the mocking layer. A living document: correct
it in place, and move a finished item to History in one or two lines.

## 1. TODO, in order

### Next

1. **Failure messages.** Two read badly, found with 3.2.0's children per address: `Expected Result to
   be IChild but found IChild`, since every mock of a type prints as its alias, and, where the subject
   handed on the child it was given, `Expected IChildSink.Accept(the IChild) to be invoked once but was
   never invoked` above a listing containing `IChildSink.Accept(IChild)` — the call it says never
   happened. A reader can only reach the diagnosis by elimination. **Design** (decided): a mock names
   itself by where it came from. The shared mock is `the IChild`, which is what `The<IChild>()` returns;
   a child is `IChild from IParent.GetChild(1)`, its owner's name and the call that made it, composing
   down a chain. The name is fixed at birth, at the only two places a mock is made — a root in
   `MockRegistry`, a child in `MockChildren.At` — and reaches the reader through the one place a mock
   becomes text: the `ToString` the interceptor answers, which carries both sides of a failed assertion
   and every mock argument in a listing. `ReceivedCalls.Describe` already writes the call. Left as they
   are: a delegate mock and a class overriding `ToString` never reach that seam, and the VALUES block
   reads `IParent:1 = the IParent`. Further ideas: a chain's later steps in the listing, the argument
   that differed, setups no call matched.

2. **Several mocks of one type.** `A<IRule>()` and `ASecond<IRule>()` are the same mock, and an
   `IEnumerable<IRule>` constructor parameter gets an empty collection (probed), while the README says
   "distinct mentions get distinct values where the type has room for them". Composites, validator
   lists and pipeline behaviours are everyday DI. **To design:** how a setup or a verification
   addresses one of several; `Then<IRule>` counts them all, as it does chain children. The per-address
   rule from 3.2.0 answers the same question for calls and is the place to start.

3. **Set up a call by name**: `Given<IChat>().That(nameof(IChat.Complete))`. Reaches a member no
   expression can name, folds `ThatProtected` into `That`, and a protected property's setter waits on
   it. **Design** (decided in the improvement plan): the return type matches exactly; a name covers
   every overload; a from-arguments `Returns` narrows it to one; it renders "Given IChat.Complete
   returns …"; it carries "any type argument", which `ILogger` needs.

### Waiting for a real spec from Cdr or M5 that is worse without it

4. **Assert on what a call received.** Neither TSpec nor Moq has it. Today `Tap` into a field and
   `Then(field)`, or `Any<T>(constraint)`, whose failure says only "never invoked". MyHotel's
   `Then<IBookingStore>(nameof(Save), Once)` is a candidate for a stronger claim.
5. **Microsoft's `ILogger`.** `LogError(…)` is an extension over `Log<TState>` with an internal state
   type, so only a count by name can reach it, not level or message; Moq needs `It.IsAnyType`. Neither
   accepts the extension call itself, `Then<ILogger<OrderService>>(_ => _.LogError(Any<Exception>(),
   Any<string>()))`, which TSpec would translate. **Ask:** does either project verify logging?
6. **Call order across mocks** — "saved before published". Needs a sequence number shared by every
   mock's log.
7. **Raise an event on a mock** (Moq's `Raise`, `Raises`). Subscribing is logged as a call, nothing more.
8. **A partial mock that runs the real member** (Moq's `CallBase`): an abstract class's template method
   as the subject, and default interface members.
9. **No other calls** (Moq's `VerifyNoOtherCalls`).
10. **`because` on a verification** (Moq's `Verify(…, failMessage)`).
11. **A mock implementing further interfaces** (Moq's `As<TInterface>()`).
12. **A service-wide sequence**: `Given<IChatCompletion>().First().Returns(…).AndNext()…`.
13. **`Any<T>()` inside an argument, matched by structure** (PO, 2026-09-15: revisit, possibly
    support). **To decide:** members an initializer leaves out, positional records, nesting inside a
    constraint's own lambda.

**Parked, not mocking:** a pipeline timeout — Cdr hand-rolls `Patience` constants; check xUnit's
`[Fact(Timeout)]` on synchronous test methods first.

## 2. How to work an item

- Before any code, one proposal: the failing example, the intended behaviour, any new user-facing
  wording, and the docs line if a reader would otherwise get something wrong. The PO approves once.
- Then test first, implement, and refactor what was touched — names included. Report once: what
  changed, test results, decisions left. Stop there; the next item starts on the PO's word.
- Each item lands on its own: suite green on all three frameworks, MyHotel green.
- A setup reads the same whether the member is sync or async (PO, 2026-09-14).
- Probe a mocking change against a property, a generic method, an `out` parameter, an overload set, a
  non-virtual member and a `Task`-returning void — the six that broke the reverted by-name attempt.
- **Probing**: the current engine with a throwaway spec in `Core.Test`, deleted after. Moq 4.20.72
  with a console probe on the cached package; the unpublished Moq-based 3.0 from `git archive
  2a9a985`, with a probe test dropped into its `Core.Test`.

## 3. The engine

`Core/Internal/TestData/Generation/Strategies/Mocking/`:

- `MockHandle` — one mock, receiving each call in three steps: `SetupGuard` refuses what a setup lambda
  may not do, `CallLog` records it as a `MockInvocation` with the pipeline phase it was made in (and
  counts, chains included), `CallSetups` answers with the latest matching setup, else
  `FluentDefaultProvider` does, once per address: `KeptAnswers` keeps what was answered, `CallAddress`
  names it and leads a property's set to its getter through `PropertyAccess`, and a call the defaults
  answer with the mock of its type is answered with a child of that address instead. Only calls made
  from `Act` on are counted. `MockInstance` makes the instance: a Castle proxy of `object` implementing
  an interface, or of a class, or a delegate `DelegateForwarder` compiles; of `object`'s members only
  `ToString` is intercepted, answering with the type's alias.
- `CallMatcher` — which calls a setup or verification is about: a method (through overrides, with
  generic type arguments), one predicate per argument, and out values, which match anything and are
  handed back. `CallReader` reads the lambda: a method, property getter or delegate invocation on the
  service, refusing a non-virtual member. `ArgumentMatcher` makes each predicate: by value (collections
  by content), `Any<T>()`, `Any<T>(constraint)`. `ArgumentRefusals` refuses Moq's `It.*`, an `Any`
  converted to a type that cannot hold it, an `Any` inside an argument (`NestedAny`), an argument that
  reads the mock (`MockRead`).
- `CallChain` splits `_ => _.GetChild(2).Get(1)` into its first step and the rest; `MockChildren` holds
  a mock's chained setups and its children by address; `AsyncAnswer` faults a task on a throw and wraps
  a value in one; `MockRegistry` keeps one handle per type; `MockingStrategy` decides which types are
  mocked; `ReceivedCalls` lists a mock's calls for a failed verification.
- Outside the folder: `Pipelines/MockCallSequence` (a sequence's steps and its taps),
  `Pipelines/ProtectedMember` (`ThatProtected`), `ExpressionDescriber.MockCallBinder`, and
  `TestResult.VerifyCall`, which counts `CallMatcher` matches in the log — a chain on the mocks its
  first step actually answered with, refusing a step that answered with anything else.

**Pinned, so an engine change cannot drop it silently**: the order unmatched calls are answered in
(`WhenReturnsDefaultValue`, `WhenMockReturnsSelf`, `WhenValueTaskOfInterface`), async throws fault
(`WhenAMockedAsyncCallThrows`), and every member kind and unstated Moq behaviour
(`WhenMockingEachMemberKind`).

## 4. Settled behaviour

- **What is mocked**: interfaces, abstract classes and delegates always; a concrete class once the test
  sets it up; a sealed one never. An abstract class behaves as an interface throughout (probed).
- **`The<T>()` is the answer to no call** — a call answering with a mock gives a child per address, so
  a verification names the child as the call that reached it, `The<IParent>().GetChild(1)`.
- **Internal types** need `[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]` in the assembly
  that declares them, as Moq did (PO, 2026-09-15: behave as Moq until the need is understood). A
  `DispatchProxy` spike would lift it for interfaces only. No docs.
- **Rendering**: a delegate mock as text is its full type name where an interface mock is its alias, as
  Moq's was; an abstract class overriding `ToString` answers a generated string. `.Result` in a chain is
  left out of the specification by text, so a mocked member really named `Result` is left out too.
- **An unmatched default interface member** answers TSpec's default without running its body, as Moq
  did; pinned in `WhenADefaultInterfaceMemberIsMocked`. Running it is the partial-mock item.
- **Verifying a mock the subject never received** counts the calls it received, none, so
  `Then<IFoo>(wasInvoked: Never)` passes as specified. No claim is made that the subject held it.
- **An argument that is the mock itself** (`_ => _.Compare(_)`) still throws the raw
  `InvalidOperationException`; raise only if asked.
- **A mock from another library**: README §4.7 says make it there, hand it in with `Using`, arrange and
  verify it there. Left unsaid (PO): `Given<IFoo>()`/`Then<IFoo>()` then reach TSpec's own mock, which
  the subject no longer receives.
- **Moq stays in the repository on purpose**: `Core.Test/AutoMock/MoqIt.cs` stands in for `Moq.It` so
  the refusal can be tested. The package references no Moq.
- Inside any `TSpec.*` namespace a bare `Times` binds to `TSpec.Times`.
- Undecided: the README's opening comparison with "plain xUnit with Moq".

## 5. History

### Done

- **2.8.0** — `TSpec.Times` replaces `Moq.Times` in the public API. 2026-09-13.
- **3.0.0** — the engine: a seam over Moq, then Castle.Core 5.2.1 behind a switch until the failure
  count hit 0, then Moq's files and package deleted; obsolete surface deleted. Chained calls with a
  child per address, async answers and faults, sequences and taps, and the refusals of `It.*`, a nested
  `Any` and a mock-reading argument. Pinned across `WhenMockingAChainedCall`, `WhenAVerificationFails`,
  `WhenMockingEachMemberKind`. 2026-09-13–15.
- **3.0.1** — a service-wide `Returns`/`Throws` counts as arranging the mock; `Then(x)` refuses a value
  type handed over before the pipeline runs. 2026-09-15.
- **3.1.0** — a class set up with `Given<T>()` is mocked (`WhenAClassIsSetUp`), through its greediest
  constructor where it has no parameterless one (`WhenAClassHasNoParameterlessConstructor`), and a
  chain goes through any class that is not sealed (`WhenAChainGoesThroughAClass`). A property's set and
  read are named with `Set`/`Get` (`WhenAPropertySetIsMocked`, `WhenAPropertyReadIsVerified`). Calls
  made while arranging are not counted (`WhenCallsAreMadeWhileArranging`). Verifying by name refuses a
  property, a field, or a name of no method (`WhenVerifyingByName`). The pipeline gained its `Mock`
  phase, values before setups (`WhenASetupReadsAMock`), and `Using<T>(setup)` on an interface runs on
  its mock. 2026-09-19.
- **3.2.0** — a property keeps its value (`WhenAPropertyKeepsItsValue`); every call no setup matches is
  answered once per address and answers the same from then on, with a child per address where it
  answers with a mock (`WhenACallIsAnsweredPerAddress`, `WhenAChainIsVerifiedThroughAnUnmatchedStep`);
  a verification through a step that answered with anything but a mock is refused, naming the setup to
  write (`WhenAChainIsVerifiedThroughAClassThatIsNotSetUp`); the 3.1.0 refusal of a set in a setup
  lambda is gone, while a read there is still refused. 2026-09-22.
- **Production upgrades** — M5: 1276 green, the one break `using static Moq.Times`. Cdr: the .NET 10
  SDK needs the MTP opt-in in `global.json`, and then refuses VSTest options such as `--logger trx`.

### Not taken

- A **`Keeps()`** verb for a stateful property: it is the default, and `Get`/`Set` earn their place on a
  gap a stateful property has not.
- **Mocking every class a mock returns**, data included; and mocking one because a verification names
  it, which would depend on whether that verification is the one that runs the pipeline.
- A **set through a chain**, `Set(_.Child.Name, …)`: `Given<IChild>().That(_ => Set(_.Name, …))` reaches
  the child more simply. `ThatSet(_ => _.Name, value)` and `That(_ => _.Name).Set(…)`, whose value would
  make `Any` a generated value, and Moq's plain lambda against a recording mock.
- A **service-wide default no member answers with** is not refused: it is valid in a base test class,
  and one the type has no use for is the developer's to see.
- `Verifiable`/`VerifyAll`: verification belongs in `Then`. `Mock.Of`'s LINQ form: `Given<T>().That(…)`
  covers it. Conditional setups; `MockRepository`. Custom default providers: `Using` covers them.
  `Protected().As<>()`: the by-name item covers it.
- `Throws` from the call's arguments: `Throws(() => new NotFound(The<int>()))` states the same.
  `ref` arguments matched as any and written back: as Moq, and no need seen. Delayed async answers: a
  `TaskCompletionSource` returned as the task does it. From-arguments `Returns` on a sequence step;
  reopen only if the engine makes it free.
