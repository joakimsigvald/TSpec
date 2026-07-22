# TSpec — Agent Reference

Condensed reference for AI coding agents writing tests with TSpec (covers TSpec 1.1).
TSpec is a fluent Given–When–Then specification framework for .NET on top of xUnit v3 + Moq.
Full human documentation: [README.md](https://github.com/joakimsigvald/TSpec#readme).

## Core model — read this first

- A test class subclasses `Spec<TSUT, TResult>` (subject type, return type of the method under test). Variants: `Spec<T>` = `Spec<T, T>`; non-generic `Spec` = no subject, no result (static/void).
- **Execution is deferred**: nothing runs until the first `Then()` call or `Result` access. The pipeline runs **at most once** per test method.
- **Declaration order of arrange steps does not matter.** Effective execution order is always: `Given` → `Having` → `When` → `Until`.
  - `Having` steps run in **reverse** declaration order (last declared runs first), just before `When`.
  - `Until` steps run in declaration order, **after the test method returns** (on dispose).
  - `Given` data is applied in reverse declaration order; mocked behavior in declaration order.
- **Exactly one `When` per spec class.** Put shared `When`/`Given` in an abstract base constructor; specialize with nested subclasses (see Structure below).
- xUnit creates a new test-class instance per test method, so the whole pipeline is built and torn down per test.
- The subject under test (SUT) is auto-constructed: interfaces/abstract dependencies become Moq mocks automatically; concrete constructor args are generated. Provide your own with `Using(instance)`.
- Test methods do **not** need to be `async`, even for async code under test. `When(_ => _.DoAsync())` is awaited internally. Async test methods are supported when needed (e.g. awaiting external setup before asserting).

## Pipeline verbs

| Verb | Purpose | Notes |
|---|---|---|
| `When(_ => _.Method(args))` | The single action under test | Lambda gets the SUT; overloads for `Action`/`Func`/async |
| `Given().A(value)` / `Given(tag).Is(v)` | Provide input test data | Applies to Input only; also `Given().ASecond(value)`…, `Given().One(v)`/`Some(values)`/`Two<T>()` for collections |
| `Given<TService>().That(_ => _.Call(...)).Returns(...)` | Mock dependency behavior | See Mocking |
| `Given<TModel>(m => m.X = 1)` / `Given((int i) => i + 1)` | Setup/transform generated values of a type | Applied most-recent-first |
| `Using(value)` / `Using(() => value)` / `Using(tag)` | Register default value/factory for a type | Optional scope: `For.Input`, `For.Subject`, `For.All` (default); `owned: true` = pipeline disposes it on teardown |
| `Using<TTarget>().From<TSource>()` | Type conversion for generation | See Conversions |
| `Having(_ => _.Setup())` | Setup on the SUT before `When` | Reverse declaration order |
| `Until(_ => _.Cleanup())` | Teardown after the test | Declaration order |
| `Then()` | Run pipeline, start assertion | `Then(because: "reason")` adds rationale (once per test) |
| `Because(reason)` | Sugar for `Then(because: reason)` | `Because("...").Result.Is(...)` |
| `Then().Result` | The captured return value | Assert with `.Is(...)` etc. |
| `Then().Throws<TEx>()` / `Throws()` | Assert thrown exception | Exposes the caught exception via `.that`: `Throws<TEx>().that.Message.Is("...")`, `.that.Message.Does().Match(p)`, `.that.ParamName.Is(...)` — full assert vocabulary, renders "throws TEx that Message is ...". Also `Throws<TEx>(e => condition)`, `DoesNotThrow()`, `DoesNotThrow<TEx>()`. `Throws(func)` compares by **reference** — pass a mention (`The<TEx>`) to verify the arranged instance propagated; use the type/condition overloads to assert by content |
| `Then<TService>(_ => _.Call(...))` | Verify a mock was called | Optional `Times` argument |

## Test data: mentions and tags

Mentions generate-and-remember values **per type, per test** — the same mention always returns the same value within a test. Generated values of a type are unique within a test.

- Single values: `A<T>()`, `An<T>()`, `The<T>()`, `AFirst<T>()`, `TheFirst<T>()`, `ASecond<T>()`, `TheSecond<T>()` … up to `Fifth` (5 numbered slots per type).
- With inline setup: `A<Cart>(_ => _.Id = 3)`.
- Collections: `Zero<T>()`, `One<T>()`, `Two<T>()`, `Three<T>()`, `Four<T>()`, `Five<T>()`, `Some<T>()` (≥1), `Many<T>()` (≥2), `AnyNumberOf<T>()`.
- Throwaway values (never referenced again): `Any<T>()`, `Another<T>()`.
- Tags name values of the same type: `static Tag<string> name = new(nameof(name));` then `Given(name).Is("Ada")`, reference with `The(name)`, or register as default with `Using(name, For.Subject)`.

Value resolution order when a value is requested: (1) already-mentioned value; (2) registered conversion/value source, then explicit `Using` value/factory; (3) built-in generation (primitives, enums, collections, mocks for interfaces/abstract types, constructed concrete classes); then any `Given` setup/transform lambdas are applied.

## Conversions and sequences (`Using<TTarget>()`)

- `Using<int>().From<byte>()` — generate targets from source-type values (via cast operators, single-arg constructors, or static factories).
- `Using<int>().From((byte b) => b + 1)` — explicit conversion lambda.
- `Using<int>().From<int>().StartingAt(10).Spaced(5)` — numeric/temporal sequences; `Spaced` takes a constant (negative = descending) or step function. Exhausting the type's range throws `ValuesExhausted`.
- `Using<Guid>().From(Guid.NewGuid)` — generator function (duplicates allowed).
- `Using<int>().From([10, 20, 30])` — explicit value list, in order; requesting more throws `ValuesExhausted`.
- Registrations for the same target type must have disjoint scopes (`For.Input` vs `For.Subject`); overlap throws `SetupFailed`.

## Mocking

```csharp
// Return value
Given<ICartRepository>().That(_ => _.GetCart(The<int>())).Returns(A<Cart>)
// Compute from call arguments (up to 5, signature must match the call)
Given<ICalculator>().That(_ => _.Add(TheFirst<int>(), TheSecond<int>())).Returns((a, b) => a + b)
// Throw
Given<IService>().That(_ => _.Get()).Throws<TimeoutException>()
// Different behavior per successive call
Given<IMyService>().That(_ => _.GetValueAsync())
    .First().Returns(() => 1)
    .AndNext().Throws(An<ArgumentException>)
    .AndNext().Returns();
// Observe arguments without changing behavior
Given<IMyInterface>().That(_ => _.Get(An<int>())).Tap<int>(i => _captured = i).Returns(() => 42)
```

Unmocked interface methods return auto-generated defaults (no strict-mock failures). For Moq features TSpec lacks, build a `Mock<T>` manually and supply `Using(myMock.Object)`.

Verification (in test methods): `Then<IOrderService>(_ => _.CreateOrder(The<Cart>()));` — optionally with `Times`: `Then<IOrderService>(_ => _.CreateOrder(The<Cart>()), Times.Once())`. Note: mentions in verify expressions match by value — a fresh `Any<T>()` matches nothing; use `The<T>()`/`The(tag)` to match arguments used in the test.
Aggregate invocation count (any method/property): parameterless `Then<TService>()`/`And<TService>()` + `WasInvoked()` (≥1), `WasInvoked(Times)`, `WasInvoked(Func<Times>)`. With `using static Moq.Times;`: `Then<IEmailSender>().WasInvoked(Never)`, `.And<IOrderService>().WasInvoked(Once)`. Counts all invocations incl. property gets/sets; composes with a specific verification to mean "this call and no other".

## Assertions (`TSpec.Assert`)

Called directly on values; every assertion returns a chainable continuation.
Combinators (lowercase): `.and.`, `.not.`, `.either. ... .or.`, `.that.`, `.but.`.
Also works standalone in plain xUnit tests (no `Spec` base class required), as a replacement for FluentAssertions.

- Any value: `Is(x)`, `Is().EqualTo(x)`, `Is().Not(x)`, `Is().Null()`, `Is().Like(obj)`/`EquivalentTo(obj)` (structural), `Has(_ => _.Id == 3)`, `Is().A<T>().that`/`Is().An<T>().that` (asserts type and returns the value strongly typed as `T`; `A`/`An` are synonyms; subtypes accepted; `that` after `not.A<T>()` throws `SetupFailed`). `Has().Type<T>()` is deprecated — use `Is().A<T>()`
- Numeric: `Is().GreaterThan(x)`, `LessThan(x)`, `Around(x, tolerance)`, `Even()`, `OneOf(values)`, `True()`, `False()`
- Strings: `Is().Like("abc")` (case/whitespace-insensitive), `Empty()`, `NullOrEmpty()`, `NullOrWhitespace()`; `Does().Contain/StartWith/EndWith(s)` (each with an optional `StringComparison` overload), `Does().Match(pattern)` (regex; also takes a `Regex`), `Has().Length(n)`, `Has().Length().AtLeast/AtMost/InRange(...)`
- Time: `Is().Before/After(t)`, `CloseTo(t, tolerance)`; TimeSpan: `Positive()/Negative()`
- Collections (deferred sequences are lazily cached at `Is()`/`Has()`/`Does()` — each element produced at most once): `Is().EqualTo(list)` (same order), `Like(list)` (any order), `SameAs(list)`, `Empty()`, `Distinct()`; `Does().Contain(x)`; `Has().Count(n)`, `Count().AtLeast/AtMost/InRange(...)`, `Count(predicate).At(n)`, `Order().Ascending()/Descending()` (comparable items) or `Order(it => it.Key)` (any IComparable key), `OneItem()`…`FiveItems()` (assert count, returns items: `list.Has().OneItem().that.Age.Is(3)`), `All(predicate)`, `Some(predicate)`, `None(predicate)`. Accessing `that` after an inverted assertion (`not.OneItem().that`) throws `SetupFailed`.
- Dictionaries (binds to `IReadOnlyDictionary`; `IDictionary`-declared variables fall back to enumerable-of-pairs): `dict.Has().Key(k)`, `Has().Value(v)`, inverted with `no` (`Has().no.Key(k)` — `no` replaces `not` here), `dict.Has(key).that.Is(v)` (asserts key exists, `that` is the value). Key lookups respect the dictionary's key comparer; failures list the key-value pairs.
- **No trainwrecks in `Then`/`And` subject expressions** — `Then(_.A.B.C)` throws `SetupFailed`; chain properties after the assertion continuation instead.

## Lifecycle and ownership

- Teardown order after the test method: `Until` steps (declaration order), then TSpec disposes disposable objects **it created** for the SUT graph (SUT first, then its constructed dependencies), supporting `IDisposable` and `IAsyncDisposable`.
- Not disposed by TSpec: instances provided via `Using` (value, factory, or tag), mocks, and generated input data. To manage the SUT's lifetime yourself, provide it with `Using(new MySut(...))`.
- Exception: `Using(..., owned: true)` transfers ownership to the pipeline — the provided/created object is disposed with the TSpec-created graph. Idiom for integration tests: `Using(CreateClient, owned: true)` in the base spec constructor gives every test a fresh `HttpClient`, disposed after the test (replaces per-test `using` statements).

## Common errors → causes

| Error message | Cause / fix |
|---|---|
| `Cannot provide setup after pipeline is set up` / `...after test pipeline was run` | Arrange call (`Given`/`Using`/`Having`/`Until`) after `Then()`/`Result` executed the pipeline. Move all arrangement before the first assertion. |
| `When must be called before Then or Result` | Missing `When` — every spec needs exactly one. |
| `Cannot call When twice in the same pipeline` | Two `When` calls; use nested given-classes to vary preconditions instead. |
| `Tried to use Result, but an action ... was provided` | `When` got an `Action` but the test reads `Result`. Pass a `Func` matching the declared `TResult`. |
| `No trainwrecks in Then/And!` | Subject expression like `Then(x.A.B)`; assert on `x.A` and chain `.B` after the continuation. |
| `AndNext must be preceded by First` | Sequential mock setup must start with `.First()`. |
| `Because can only be provided once per test method` | One logical assertion (and one `because`) per test method. |
| `ValuesExhausted` | A `From` sequence/list ran out of unique values; widen the sequence or provide more values. |
| `InvalidTypeConversion` | No conversion path between registered source and requested target; register `Using<TTarget>().From(...)` with a lambda. |
| `{Mock} returns a Task<T>. Interface types returned as task must be provided explicitly` | Auto-mock can't fabricate an interface inside a `Task`; set it up: `Given<TheMock>().That(...).Returns(A<TheInterface>)`. |

## Recommended structure

One test project per production project (`X.Spec`); one folder per class under test; one abstract class per method under test (`WhenMethodName`) holding the `When` in its constructor; nested concrete/abstract classes per precondition (`GivenSomething`) adding `Given` in their constructors; one test method per logical assertion (`Then...`). Prefer expression-bodied members and fluent chaining.

## Complete examples

```csharp
using TSpec;
using TSpec.Assert;

// 1. Instance SUT with mocked dependency + verification
public class WhenPlaceOrder : Spec<ShoppingService>   // Spec<T> : SUT and result both T
{
    static Tag<Guid> cartId = new(nameof(cartId));

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
public class WhenGetCartNotFound : Spec<ShoppingService, Cart>
{
    public WhenGetCartNotFound()
        => When(_ => _.GetCart(An<int>()))
           .Given<ICartRepository>().That(_ => _.GetCart(The<int>())).Throws<KeyNotFoundException>();

    [Fact] public void ThenThrows() => Then().Throws<KeyNotFoundException>();
}
```
