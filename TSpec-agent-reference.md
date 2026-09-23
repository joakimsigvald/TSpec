# TSpec — Agent Reference

Condensed reference for AI coding agents writing tests with TSpec (covers TSpec 3.3).
TSpec is a fluent Given–When–Then specification framework for .NET on top of xUnit v3.
Full documentation: [README.md](https://github.com/joakimsigvald/TSpec#readme).

## Core model — read this first

- A test class subclasses `Spec<TSUT, TResult>` (subject type, return type of the method under test). `Spec<T>` is `Spec<T, T>`, and also the spelling when the result is not asserted; non-generic `Spec` has neither subject nor result.
- **Execution is deferred**: nothing runs until the first `Then()` or `Result`, and the pipeline runs **at most once** per test method. All arrangement must come before it.
- **Declaration order does not matter.** Execution is always `Given` → `Having` → `When` → `Until`. `Having` steps run in reverse declaration order; `Until` steps in declaration order, after the test method returns.
- **Exactly one `When` per test.** Put the shared `When` in an abstract base constructor and vary preconditions in nested subclasses (see Recommended structure).
- The subject is auto-constructed: its dependencies are mocked or generated (see Mocking). Provide your own with `Using(instance)`.
- Test methods need not be `async`. `When(_ => _.DoAsync())` awaits `Task`, `Task<T>`, `ValueTask` and `ValueTask<T>` — don't make the lambda `async`. A lambda that is `async` binds to the `Task` overloads; state its return type to select `ValueTask` (`When(async ValueTask (_) => ...)`), and for a bare throw (`Until(void (_) => throw ...)`).

## Pipeline verbs

| Verb | Purpose |
|---|---|
| `When(_ => _.Method(args))` | The single action under test; the lambda gets the subject |
| `Given().A(value)`, `Given(tag).Is(value)` | Input test data; also `ASecond(value)`, `One(value)`, `Some(values)`, `Two<T>()`… |
| `Given<TService>().That(_ => _.Call(...)).Returns(...)` | Mocked dependency behavior (see Mocking) |
| `Using<T>(t => t.X = 1)`, `Using((int i) => i + 1)` | Set up or transform every generated value of a type, most recent first |
| `Using(value)`, `Using(() => value)`, `Using(tag)` | Default value or factory for a type; `owned: true` makes the pipeline dispose it |
| `Using<TTarget>().From<TSource>()` | Generate one type from another (see Conversions) |
| `Having(_ => _.Setup())` | Setup on the subject before `When` |
| `Until(_ => _.Cleanup())` | Teardown after the test |
| `Then().Result` | Run the pipeline and assert the return value |
| `Then(because: "reason")`, `Because("reason")` | Rationale for the assertion — once per test |
| `Then().Throws<TEx>()`, `Throws()` | Assert the thrown exception; `.that` exposes it: `Throws<TEx>().that.Message.Is("...")`. Also `Throws<TEx>(e => condition)`. `Throws(The<TEx>)` compares by **reference** — the arranged instance itself |
| `Then().Completes()` | Assert it returned rather than threw |
| `Then<TService>(_ => _.Call(...))` | Verify a mock call (see Verification) |

`Using` takes an optional scope: `For.Input`, `For.Subject` or `For.All` (default).

## Test data: mentions and tags

The same mention returns the same value throughout a test; distinct mentions get distinct values where the type has room for them.

- Single values: `A<T>()`, `An<T>()`, `The<T>()`, `AFirst<T>()` and `TheFirst<T>()` all name the **same** value; `ASecond<T>()`/`TheSecond<T>()` … up to `Fifth`. With inline setup: `A<Cart>(_ => _.Id = 3)`.
- Collections: `Zero<T>()` … `Five<T>()`, `Some<T>()` (≥1), `Many<T>()` (≥2), `AnyNumberOf<T>()`.
- Throwaway values, never referenced again: `Any<T>()`, and `SomeOther<T>(count = 2)` for an array unlike every value already mentioned — after `Three<int>()`, `Some<int>()` returns those three. In a mock setup or verification, `Any<T>()` means any value.
- Tags name values of one type: `static Tag<string> name = new();` (named after the field), then `Given(name).Is("Ada")`, `The(name)`, or `Using(name, For.Subject)`.

A requested value is the already-mentioned one; otherwise a registered conversion, then a `Using` value or factory, then built-in generation. `Using<T>` setup/transform lambdas are applied last.

## Conversions and sequences

- `Using<int>().From<byte>()` — generate targets from source values (cast operators, single-argument constructors, static factories).
- `Using<int>().From((byte b) => b + 1)` — explicit conversion.
- `Using<int>().From<int>().StartingAt(10).Spaced(5)` — numeric/temporal sequence; `Spaced` takes a constant (negative = descending) or a step function.
- `Using<Guid>().From(Guid.NewGuid)` — generator function, duplicates allowed.
- `Using<int>().From([10, 20, 30])` — exactly these values, in order.
- A sequence or list that runs out throws `ValuesExhausted`. Registrations for the same target type need disjoint scopes (`For.Input` vs `For.Subject`), else `SetupFailed`.

## Mocking

```csharp
// Return value
Given<ICartRepository>().That(_ => _.GetCart(The<int>())).Returns(A<Cart>)
// Compute from call arguments (up to 5, signature must match the call)
Given<ICalculator>().That(_ => _.Add(TheFirst<int>(), TheSecond<int>())).Returns((a, b) => a + b)
// Another call on the same service, then the next service
Given<IRoomStore>().That(_ => _.Find(The<int>())).Returns(A<Room>)
    .AndThat(_ => _.IsBooked(The<int>())).Returns(() => false)
    .And<IClock>().That(_ => _.Today).Returns(() => The<DateOnly>())
// One mock of several, by mention (not called) or tag; it wins over the type's setup
Given(TheSecond<IRule>).That(_ => _.Passes()).Returns(() => false)
// Through a chain of members; an awaited member is written with .Result
Given<IUnitOfWork>().That(_ => _.Orders(2).Find(Any<int>())).Returns(A<Order>)
// Throw
Given<IService>().That(_ => _.Get()).Throws<TimeoutException>()
// Different behavior per successive call
Given<IMyService>().That(_ => _.GetValueAsync())
    .First().Returns(() => 1)
    .AndNext().Throws(An<ArgumentException>)
    .AndNext().Returns();
// Observe arguments without changing behavior; after First or AndNext a tap belongs to that step, before First it taps every call
Given<IMyInterface>().That(_ => _.Get(An<int>())).Tap<int>(i => _captured = i).Returns(() => 42)
// Default for every method returning a type Cart fits
Given<ICartRepository>().Returns(A<Cart>)
// Protected members only by name
Given<HttpMessageHandler>().ThatProtected<HttpResponseMessage>("SendAsync").Returns(A<HttpResponseMessage>)
```

- Interfaces, abstract classes and delegates are mocked; a class once the test sets it up with `Given<T>()`. Only a class's virtual members are mocked.
- Each mention of a mocked type is a mock of its own, and the subject's dependencies are others: hand one over with `Using(The<IRule>)` to arrange or verify what the subject holds. `Given<IRule>()` and `Then<IRule>(…)` reach them all, and a count is the total across them.
- A constructor parameter with a default keeps it, unless the test arranged that type.
- Arguments match by value — `The<T>()` matches the value used in the test — except `Any<T>()`, which matches any value, and `Any<T>(b => b.Nights > 7)`, which matches any value satisfying the constraint. The constraint form throws `SetupFailed` outside a mock setup or verification.
- Setups are the same whether the member returns `T`, `Task<T>` or `ValueTask<T>`: `Returns(() => 7)` supplies the unwrapped value. For a task that completes later, state the task as the return type: `That<Task<int>>(_ => _.GetAsync()).Returns(() => _pending.Task)`.
- An expression cannot assign, so a property set is written `Set(_.Name, value)`, or `Set(_[1], value)`, and set up or verified as any call: `Then<IIdSource>(_ => Set(_.Name, Any<string>()), Never)`. A read is verified as `Get(_.Name)`; it is set up as `That(_ => _.Name)`.
- Unmocked members return generated defaults, one per address: a call no setup matches is answered once for the arguments it was called with and answers the same from then on, other arguments getting another answer.
- A property keeps its value, unless set up with `That(_ => _.Name).Returns(…)`: a read answers the last set, else what the first read answered, and an indexer keeps one value per index.
- A chain gets a mock per step and argument values: `Orders(1)` and `Orders(2)` are set up and verified apart, `Orders(1)` twice is the same mock. Calls the chain did not set up answer as `Given<IOrderStore>()` set them up; a step no chain matches reaches a mock of its own all the same, for the arguments it was called with.
- `Then<IOrderStore>(…)` counts calls on every `IOrderStore` mock, including those reached through a chain; verify through the chain to count one.
- A delegate starts a chain too, so a factory gives a mock per argument: `Given<Func<int, IOrderStore>>().That(_ => _(2).Find(Any<int>()))`.

## Verification

```csharp
using static TSpec.Times;

Then<IOrderService>(_ => _.CreateOrder(The<Cart>()))                    // called at least once
Then<IEventQueue>(q => q.MarkRejected(42, Any<string>()), Once)          // count, arguments matched
    .And<IEventQueue>(nameof(IEventQueue.MarkFailed), Never)             // any arguments, all overloads
    .And<IEntityWriter>(wasInvoked: Never);                              // any member, property access included
Then(TheSecond<IRule>, _ => _.Passes(), Once)                            // one mock of several, by mention (not called) or tag
```

`wasInvoked:` must be named on the whole-service form. Counts: `Once`, `Never`, `AtLeastOnce`, `AtMostOnce`, `Exactly(n)`, `AtLeast(n)`, `AtMost(n)`, `Between(from, to)` (inclusive).

## Assertions (`TSpec.Assert`)

Called directly on values; every assertion returns a continuation. Combinators are lowercase: `.and.`, `.not.`, `.either. ... .or.`, `.that.`, `.but.`.
Works standalone in plain xUnit tests too.

- Any value: `Is(x)`, `Is().Not(x)`, `Is().Null()`, `Is().Like(obj)` (structural), `Has(_ => _.Id == 3)`, `Is().A<T>().that` (asserts the type, exposes the value as `T`; `An<T>()` is a synonym).
- Numeric: `Is().GreaterThan(x)`, `LessThan(x)`, `Around(x, tolerance)`, `Even()`, `OneOf(values)`, `True()`, `False()`.
- Strings: `Is().Like("abc")` (case/whitespace-insensitive), `Empty()`, `NullOrEmpty()`, `NullOrWhitespace()`; `Does().Contain/StartWith/EndWith(s)` (optional `StringComparison`), `Does().Match(pattern)`; `Has().Length(n)`, `Has().Length().AtLeast/AtMost/InRange(...)`.
- Time: `Is().Before/After(t)`, `CloseTo(t, tolerance)`; TimeSpan `Positive()`/`Negative()`.
- Collections: `Is().EqualTo(list)` (same order), `Like(list)` (any order), `SameAs(list)`, `Empty()`, `Distinct()`; `Does().Contain(x)`; `Has().Count(n)`, `Count().AtLeast/AtMost/InRange(...)`, `Count(predicate).At(n)`, `Order().Ascending()/Descending()`, `Order(it => it.Key)`, `All/Some/None(predicate)`, `OneItem()` … `FiveItems()` (asserts the count and returns the items: `list.Has().OneItem().that.Age.Is(3)`).
- Dictionaries: `Has().Key(k)`, `Has().Value(v)`, `Has().no.Key(k)`, `Has(key).that.Is(v)`. These bind to `IReadOnlyDictionary`; a variable declared `IDictionary` gets only the collection assertions.
- `that` after a negated assertion (`not.OneItem().that`) throws `SetupFailed`.
- **No trainwrecks in `Then` subjects**: `Then(x.A.B)` throws `SetupFailed`; write `Then(x).A.B`.

## Lifecycle and ownership

- After the test method: `Until` steps, then TSpec disposes the `IDisposable`/`IAsyncDisposable` objects **it created** for the subject graph (subject first).
- Instances provided with `Using`, mocks and generated input are never disposed — unless provided with `owned: true`. Integration-test idiom: `Using(CreateClient, owned: true)` in the base constructor gives every test a fresh `HttpClient`, disposed after it.

## Common errors → causes

| Error message | Cause / fix |
|---|---|
| `Cannot provide setup after pipeline is set up` / `...after test pipeline was run` | Arrangement after `Then()`/`Result` ran the pipeline. Move it before the first assertion. |
| `When must be called before Then or Result` | Missing `When`. |
| `Cannot call When twice in the same pipeline` | Two `When`s; vary preconditions with nested `Given` classes instead. |
| `Tried to use Result, but an action ... was provided` | `When` got an `Action` but the test reads `Result`. Pass a `Func` returning `TResult`. |
| `No trainwrecks in Then: ...` | Hand over the root and chain after it: `Then(x).A.B`. |
| `AndNext must be preceded by First` | A sequential mock setup starts with `.First()`. |
| `Because can only be provided once per test method` | One logical assertion, one `because`, per test method. |
| `ValuesExhausted` | A `From` sequence or list ran out; widen it. |
| `InvalidTypeConversion` | No conversion path; register `Using<TTarget>().From(lambda)`. |
| `X.Member returns a T, which TSpec does not mock, so Next cannot be set up or verified through it` | A chain passes through a type that is not an interface, abstract class or delegate. Set up `X.Member` itself. |

## Recommended structure

One test project per production project (`X.Spec`); one folder per class under test; one abstract class per method under test (`WhenMethodName`) holding the `When` in its constructor; nested classes per precondition (`GivenSomething`) adding `Given` in their constructors; one test method per logical assertion (`Then...`). Prefer expression-bodied members and fluent chaining.

## Generating the specification

Opt in with one line in the spec project; a `_specification/` folder of markdown is written to the spec project root when the run ends.

```csharp
[assembly: AssemblyFixture(typeof(SpecificationDocument))]
```

- **The spec project must be named after the project it describes** plus one suffix (`MyHotel.Spec` → `MyHotel`) and reference it **directly**; otherwise `SetupFailed` before the first test.
- **Names are the document**: top-level folder → file, `When…` class → section, `Given…` class → subsection, `Then…` method → a claim, each read as words (`WhenListRooms` → "When list rooms"). Name a test method after the claim it makes. A top-level folder named `README` or as the project under test throws `SetupFailed`.
- **Written only when every non-skipped test in the assembly passed** — a filtered or failing run leaves the files untouched. Run the whole suite before expecting a diff.
- A `[Theory]` with `[InlineData]` renders as a table of its rows; more than 8 parameters throws `SetupFailed`.
- Output is deterministic, so CI can check it: `dotnet test && git diff --exit-code -- "**/_specification/*.md"` (a new file is untracked; `git status --porcelain` sees it).

## Complete examples

```csharp
using TSpec;
using TSpec.Assert;

// 1. Instance subject with mocked dependency + verification
public class WhenPlaceOrder : Spec<ShoppingService>   // result not asserted
{
    static Tag<Guid> cartId = new();

    public WhenPlaceOrder()
        => When(_ => _.PlaceOrder(The(cartId)))
           .Given<ICartRepository>().That(_ => _.GetCart(The(cartId))).Returns(A<Cart>);

    [Fact] public void ThenCreatesOrder() => Then<IOrderService>(_ => _.CreateOrder(The<Cart>()));
}

// 2. Static method, value result
public class WhenAdd : Spec<int>
{
    [Fact] public void ThenSumIs3() => When(_ => Calculator.Add(1, 2)).Then().Result.Is(3);
}

// 3. Async method under test (test method stays synchronous)
public class WhenSendReport : Spec<EmailService, SendResult>
{
    public WhenSendReport()
        => When(_ => _.SendAsync(A<Report>()))
           .Having(_ => _.SignIn(A<Credentials>()))
           .Until(_ => _.Disconnect());

    [Fact] public void ThenReturnsSent() => Then().Result.Is(SendResult.Sent);
}

// 4. Exception assertion
public class WhenGetCart : Spec<ShoppingService, Cart>
{
    public WhenGetCart()
        => When(_ => _.GetCart(An<int>()))
           .Given<ICartRepository>().That(_ => _.GetCart(The<int>())).Throws<KeyNotFoundException>();

    [Fact] public void ThenThrowsKeyNotFound() => Then().Throws<KeyNotFoundException>();
}
```
