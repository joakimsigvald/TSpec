# TSpec — Fluent, specification-style unit testing for .NET

TSpec is a fluent, specification-oriented testing framework for .NET that builds on xUnit.
It follows the Given–When–Then pattern, with built-in auto-mocking and test data generation.
Tests run on the standard xUnit runner and can live side by side with existing xUnit tests.

Whether you are new to unit testing or an experienced practitioner, TSpec helps you express test intent clearly by removing boilerplate, enforcing structure, and generating readable failure descriptions.

Install the package in your test project:
```shell
dotnet add package TSpec
```

Example: testing the `PlaceOrder` method on `ShoppingService`:
```csharp
public class WhenPlaceOrder : Spec<ShoppingService>
{
    static Tag<Guid> cartId = new(); // reference an auto-generated Guid

    public WhenPlaceOrder()
        => When(_ => _.PlaceOrder(The(cartId)))
           .Given<ICartRepository>()
           .That(_ => _.GetCart(The(cartId)))
           .Returns(A<Cart>);

    [Fact] public void ThenCreatesOrder()
        => Then<IOrderService>(_ => _.CreateOrder(The<Cart>()));
}
```

The example highlights how TSpec reduces boilerplate by handling test data, dependency mocking, and interaction verification declaratively.
In real-world usage, this typically yields substantially smaller and more readable tests than plain xUnit with Moq.

**Using an AI coding agent?** A condensed reference optimized for agents is shipped with the package
and available as [TSpec-agent-reference.md](https://github.com/joakimsigvald/TSpec/blob/main/TSpec-agent-reference.md).
Point your agent to it by adding a line to your repository's agent instructions (e.g. `CLAUDE.md` or `AGENTS.md`):

```markdown
Tests use TSpec — reference: `~/.nuget/packages/tspec/<version>/TSpec-agent-reference.md` (highest version),
or https://raw.githubusercontent.com/joakimsigvald/TSpec/main/TSpec-agent-reference.md
```

## Table of Contents

1. [Introduction](#1-introduction)  
2. [The Test Pipeline](#2-the-test-pipeline)  
3. [Using Test Data](#3-using-test-data)  
4. [Mocking & Auto-Mocking](#4-mocking--auto-mocking)  
5. [Asserting Results](#5-asserting-results)  
6. [Tests as specification](#6-tests-as-specification)

## 1. Introduction

To write a test with TSpec, start by subclassing `Spec`.
Each test is expressed as a specification and executed as a pipeline consisting of three phases:
*arrange*, *act*, and *assert*.

The following is a complete TSpec test class (a *specification*) containing a single test method (a *requirement*):

```csharp
using TSpec;
using TSpec.Assert;
using static App.Calculator;

namespace App.Test;

public class CalculatorSpec : Spec<int>
{
    [Fact] public void WhenAdd_1_and_2_ThenSumIs_3() => When(_ => Add(1, 2)).Then().Result.Is(3);
}
```

### 1.1 Arrange

The *arrange* stage defines the setup of the test pipeline, by calling methods on `Spec`, either directly or fluently chained:

* `Given`  — provides test data and mocked behavior.
* `Using`  — registers type conversions, defaults, factories, and setup applied to every value of a type.
* `Having` — setup that runs *before* the action.
* `Until`  — teardown that runs *after* the action.

TSpec's strongly typed API guides the setup, preventing most invalid test configurations at compile time.
Chapter 2 describes the test pipeline in depth, and Chapter 3 how to prepare and reference test data.

### 1.2 Act

The *act* stage specifies the behavior under test by calling `When` with a lambda expression. 
The lambda takes the subject under test as argument and invokes the behavior to verify.

The subject under test is automatically created based on the arrangement, unless it is static or explicitly provided.

The order in which `Given`, `Having`, `Until`, and `When` are declared does not matter.
Because execution is deferred until assertion, TSpec always runs the steps in the same order:
`Given` -> `Having` -> `When` -> `Until`.

Each specification defines exactly one action under test and therefore contains a single `When` stage.

### 1.3 Assert

TSpec includes a fluent assertion library, `TSpec.Assert`, conceptually similar to FluentAssertions,
but with a more compact syntax based on the verbs `Is`, `Has`, and `Does`.

The *assert* stage is specified by calling `Then` (or accessing `Result`), followed by one or more assertions.
It is only when one of these methods is called that the test pipeline is executed and the result evaluated.

If a test fails, this is either due to an invalid test setup or because an assertion was not satisfied.
In the latter case, TSpec provides detailed assertion failures together with an automatically
generated description of the specification, making it easier to understand the intended behavior.

Example:

**Specification:**
```csharp
=> When(_ => _.List())
   .Given<IMyRepository>()
   .That(_ => _.List()).Returns(A<MyModel[]>)
   .Given().Three<MyModel>()
   .Then().Result.Has().Count(4)
```

**Output:**
```csharp
Expected Result to have count 4 but found 3...
---- 
Given three MyModels
  and IMyRepository.List() returns a MyModel[]
When List()
Then Result has count 4
```

In addition to verifying return values, exceptions can also be asserted using `Then().Throws`.

With basic familiarity with NuGet, unit testing and mocking, you are now ready to write your own tests using TSpec. 
The remainder of this README is a complete, practical guide to structuring specifications, managing test data, and verifying behavior with TSpec.

## 2. The Test Pipeline

At the core of TSpec are *deferred execution* and *lazy evaluation*: no production code is executed until the first assertion is made.
A test runs through four stages: preparation, execution, assertion, and teardown.

The stages are visible in the shape of a specification: `Having` and `Until` compose fluently in chain form,
with the action at the center and each clause extending outward in time.

```csharp
public class WhenSendReport : Spec<EmailService, SendResult>
{
    public WhenSendReport()
        => When(_ => _.Send(A<Report>()))
           .Having(_ => _.SignIn(A<Credentials>()))
           .Having(_ => _.Configure(A<SmtpSettings>()))
           .Until(_ => _.FlushOutbox())
           .Until(_ => _.Disconnect());

    [Fact] public void ThenReturnsSent() => Then().Result.Is(SendResult.Sent);
}
```

Setups run outward from the action, so `Configure` runs before `SignIn`;
teardowns run in the order they are written, so `FlushOutbox` runs before `Disconnect`.

### 2.1 Preparation

Before the first assertion, the pipeline is configured with test data, mocks, and lambdas to execute,
using the arrangement methods introduced in Section 1.1.

#### 2.1.1 Creating the Pipeline
You create the pipeline by subclassing `Spec<TSUT, TResult>`, where `TSUT` is the type of the *subject under test* and `TResult` the return type of the *method under test*.
`Spec<T>` is short for `Spec<T, T>` and also fits when the result is not asserted; the non-generic `Spec` has neither subject nor result.

#### 2.1.2 Scope of Arrangement
Arrangements apply to the **Subject** — the subject under test or any of its components, provided as constructor arguments, properties, or through type cast —
or to the **Input**, the data supplied directly to the execution pipeline.

* **Values** are provided with **`Given`** and apply *only* to the Input.
* **Types** are configured with **`Using`**, with an optional scope: `For.Input`, `For.Subject`, or `For.All` (the default).
* **Mocks** are provided with **`Given`** and apply to *both* Input and Subject.

#### 2.1.3 Preparing the Pipeline
The preparation steps are recorded and later applied in the following order:

1. Defaults, setups, transforms, and test data, *in reverse order of declaration*.
1. Mocked behavior, *in order of declaration*.

#### 2.1.4 Creating the Subject Under Test
After preparation, the pipeline uses auto-mocking to create a new instance of your subject under test (unless you provided a value of that type explicitly).
If you haven't mocked a certain interface or method that the subject uses, a default mock will be auto-generated.

### 2.2 Execution

Execution is triggered by the first assertion — when `Result` is referenced or `Then()` is called. 
The pipeline then runs and captures the outcome.

#### 2.2.1 Running Setup
Setup steps are provided with `Having()`, as lambdas that take the subject under test as argument.
Setup is executed in reverse order of declaration, right after the subject under test is created.

Example:
`When(A).Having(B).Having(C)` will result in the execution order: C -> B -> A.

#### 2.2.2 Executing the Behavior Under Test
The lambda provided with `When()` will be executed right after setup.

#### 2.2.3 Collecting the outcome
The outcome of a pipeline execution is either a return value or a thrown exception.
A returned value must match the declared return type and is exposed for assertion through the `Result` property.
A thrown exception becomes the captured outcome and is asserted with `Then().Throws` (see [5.7](#57-asserting-exceptions)).

### 2.3 Assertion

Assertions consume the captured outcome or utilize the mocking framework for verifying execution paths. 
The pipeline executes at most once per test method, regardless of the number of references to `Result` or `Then()`.
Assertions are covered in depth in Chapter 5, and mock verification in Section 4.6.

### 2.4 Teardown

Teardown steps are provided with `Until()`, as lambdas that take the subject under test as argument.
Teardown is executed in order of declaration when the test class and pipeline are disposed, after the test method has run.

Example:
`When(A).Until(B).Until(C)` will result in the execution order: A -> B -> C.

After all `Until`-steps have run, TSpec disposes the disposable objects it created for the
subject-under-test graph — the subject itself and the concrete dependencies it constructed
and injected — in reverse order of creation (subject first), supporting both
`IDisposable` and `IAsyncDisposable`. Objects you provide with `Using` (as value, factory, or tag),
mocks, and generated input data are never disposed by TSpec — so to manage the subject's
lifetime yourself, provide your own instance with `Using`.

To instead transfer ownership of a provided object to the pipeline, pass `owned: true` —
TSpec then disposes it together with the objects it created itself.
A typical use is an `HttpClient` factory in integration tests, replacing a `using` statement in every test
with a single line in the shared base spec:

```csharp
public abstract class ApiSpec<TResult> : Spec<MyApiClient, TResult>
{
    protected ApiSpec() => Using(CreateClient, owned: true);

    private static HttpClient CreateClient() => _webAppFactory.CreateClient();
}
```

The factory is invoked at most once per test (each test builds its own pipeline),
and the created client is disposed when the test is torn down.

### 2.5 Sync vs. Async Execution

TSpec supports testing synchronous and asynchronous code using the same test pipeline.

When the behavior under test is asynchronous (returns `Task`, `Task<T>`, `ValueTask` or `ValueTask<T>`), TSpec waits for completion and captures the outcome in the same way as for synchronous code.
The only difference is the lambda signature provided to `When`, `Having`, `Until`, and mock setup methods.
Test methods themselves do not need to be `async`, but they may be — for instance to await other work before asserting.

A lambda that needs its own `async` body — or consists of a `throw` — may have to state its return type to be unambiguous,
e.g. `When(async ValueTask (_) => ...)` or `Until(void (_) => throw ...)`. `Task` is resolved before `ValueTask`.

## 3. Using Test Data

TSpec provides helpers for referring to test data that can either be supplied explicitly or automatically generated (optionally with setups or transforms).

Two complementary mechanisms are provided:
- Mentions, for quickly referring to generated values by position or quantity
- Tags, for assigning stable, meaningful identities to values of the same type

### 3.1 Mentions

Mentions are helper methods for generating and referring to up to five numbered values of a given type, as well as collections of up to five elements.
Mentions are resolved per type and per test and always refer to the same value within a specification.

**Single values**
For a single generated value — all of these refer to the same one:
`A`, `An`, `The`, `AFirst`, `TheFirst`

For additional values of the same type:
`ASecond`, `TheSecond`
`AThird`, `TheThird`
`AFourth`, `TheFourth`
`AFifth`, `TheFifth`

**Collections**
For collections of generated values:
`Zero`, `One`, `Two`, `Three`, `Four`, `Five`
`Some` (at least one), `Many` (at least two), `AnyNumberOf`

**Unreferenced values**
For auto-generated values that are not intended to be referenced again:
`Any`, and `SomeOther` for an array of them — two by default, unlike every value already mentioned.
A collection is a mention too, so after `Three<int>()`, `Some<int>()` returns those three, while
`SomeOther<int>()` returns two other ints.

As an argument in a mock setup or verification, `Any<T>()` means any value of the type (see [4.3](#43-mocking-with-arguments)).

**Variation**
Distinct mentions get distinct values where the type has room for them, deterministically. Small value spaces may repeat.

### 3.2 Tags

Tags complement mentions by allowing values to be referred to by name rather than position.
They are primarily useful when working with multiple values of the same type.

A tag is an instance of `Tag<TValue>` and represents exactly one value of the given type.
A tag assigned to a field takes the field's name, which shows in diagnostic output;
a tag declared anywhere else is given its name: `new("name")`.

Example:
```csharp
protected static Tag<string> name = new();
protected static Tag<int> age = new(), shoeSize = new();
```

#### 3.2.1 Set and reference tagged values
Tags can be used to set or reference values during pipeline configuration and execution.

Example:
```csharp
protected static Tag<string> firstname = new(), lastname = new();
...
=> Given(firstname).Is("Ada").And(lastname).Is("Lovelace")
   .When(_ => _.CreateUser(The(firstname), The(lastname)))
   .Then().Result.FullName.Is("Ada Lovelace");
```

#### 3.2.2 Use tagged values as default or for auto-generation
Tagged values may also be used as:

- the default value when generating test data (`For.Input`)
- input when auto-generating the subject under test (`For.Subject`)

Example:
```csharp
Using(name, For.Input).And(age, For.Subject);
```

### 3.3 Data Generation

TSpec supplies test data through an internal generation pipeline. When a value is requested — for instance with `An<int>()` — it is resolved in three steps:

1. **Reuse**: if the value has already been mentioned in the test, the same value is returned.
2. **Resolve**: otherwise the value is resolved from the arrangement — the first matching source wins:
   * a registered type conversion or value source (see [3.4](#34-type-registration-and-conversion))
   * an explicitly provided value or factory, e.g. `Using(42)` or `Using(() => new MyModel())`
   * the built-in generation strategy for the type (see below)
3. **Customize**: any setup or transform lambdas provided for the type with `Using` are applied, most recently provided first.

Built-in generation covers most types out of the box:
* **Primitives**: standard primitives like `int`, `string`, `Guid`, `DateTime`, `Uri`, and `TimeSpan`.
* **Enums, nullables and collections**: resolved from their underlying or element types.
* **Semantic types**: objects deriving from `Semantic<TPrimitive>` (such as `Email`, `PhoneNumber`, `Age`) are generated from their primitive values.
* **Interfaces and abstract classes**: mocked automatically, without boilerplate.
* **Concrete classes**: constructed with generated constructor arguments.
* If none of the above applies, the type's default value is used.

### 3.4 Type Registration and Conversion

To override how values of a type are generated, register a conversion with `Using<TTarget>()`.

* **From a source type:** `Using<TTarget>().From<TSource>()` generates source values and converts them to the target, through an implicit cast operator, a single-parameter constructor, or a matching static factory method (e.g., `Create()`).
* **Conversion lambda:** `Using<int>().From((byte b) => b + 1)` — the source type is inferred from the lambda parameter.
* **Chaining:** register several conversions with `And`, e.g. `Using<int>().From<byte>().And<long>().From<short>()`.
* **Scoping:** `Using<int>(For.Input).From<byte>()` applies the conversion only to input data, leaving subject construction unaffected. The default is `For.All`. Registrations for the same target type must have disjoint scopes — one for `Input` and another for `Subject` can coexist, but overlapping scopes throw `SetupFailed`.
* **Sequences:** for numeric and temporal source types (`DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`, `TimeSpan`), constrain the values with `StartingAt` and `Spaced`, e.g. `Using<int>().From<int>().StartingAt(10).Spaced(5)`. `Spaced` accepts a fixed spacing (negative for descending) or a step function, e.g. `Spaced(i => i * 2)`. Values are unique: a sequence that would repeat a value or leave the type's range throws `ValuesExhausted`.
* **Generator functions:** for arbitrary value spaces, pass a generator, e.g. `Using<Guid>().From(Guid.NewGuid)` or `Using<int>().From(NextFibonacci)` with a method or closure holding the state. Values are converted to the target type if needed and used as produced, so duplicates are allowed (1, 1, 2, 3, 5, ...).
* **Value lists:** `Using<int>().From([10, 20, 30])` uses exactly the given values, in order; requesting more values than the list contains throws `ValuesExhausted`.
* **No conversion path:** generation throws `InvalidTypeConversion`.

```csharp
// Generates an Email instance automatically by creating a primitive source and looking for constructors or static factories
public class WhenConvertByConstructor : Spec<MyEmailConstr>
{
    public WhenConvertByConstructor() => Using<MyEmailConstr>().From<Email>();
}

// Employs a specific conversion lambda to translate generated data
public class WhenRelayIntToByteWithConverter : Spec<int>
{
    public WhenRelayIntToByteWithConverter() => Using<int>().From((byte b) => b + 1);

    [Fact] public void ThenGenerateByteAsInt() => Three<int>().Is().EqualTo([2, 3, 4]);
}
```

## 4. Mocking & Auto-Mocking

This chapter assumes familiarity with mocking, and shows how TSpec simplifies the mocking experience.

### 4.1 Auto-Mocking subject under test

The subject under test will be created automatically with mocks and default values.
Remember from Chapter 2 that mocks are configured after test data has been generated, 
so test data, setups and transforms are available in the mocking stage regardless of where in the test they are provided.

You can supply your own constructor arguments by calling `Using`, or modify the generated ones by calling `Using` with a setup or transform lambda.
You can even provide the subject under test itself:
`Using(new MyClass(42, "Thursday"))`

**Constructor defaults are honoured.** A parameter that declares a default gets what the test arranged — a value from `Using`, a registered conversion, or a mock the test has already set up — and keeps its default otherwise.

### 4.2 Mocking

To mock the behavior of a dependency, call `Given<[TheService]>().That(_ => _.[TheMethod](...)).Returns/Throws(...)`. 
You do not need to create and manage mocks manually, but can supply mocked behavior directly to the pipeline.
This allows most mocking scenarios to be expressed inline, close to the behavior under test.

Naming no method, `Given<[TheService]>().Returns(...)` sets a default that applies to every method of the interface returning a type assignable from that type.

An awaited call is set up with the value inside its task. To answer with a task that completes later, state the task as the call's return type:

```csharp
=> Given<IQuoteService>().That<Task<Quote>>(_ => _.GetAsync(The<int>())).Returns(() => _pending.Task)
```

To set up another call on the same service, continue with `AndThat`; `And<[TheOtherService]>()` moves on to the next service:

```csharp
=> Given<IRoomStore>().That(_ => _.Find(The<int>())).Returns(A<Room>)
   .AndThat(_ => _.IsBooked(The<int>())).Returns(() => false)
   .And<IClock>().That(_ => _.Today).Returns(() => The<DateOnly>())
```

A call can be set up, or verified, through the members that lead to it. An awaited member is written with `.Result`, since an expression cannot `await`:

```csharp
=> Given<IUnitOfWork>().That(_ => _.Orders(The<int>()).Find(Any<int>())).Returns(A<Order>)
   .AndThat(_ => _.GetCustomerAsync(The<int>()).Result.Name).Returns(() => "Ada")
```

Each step of a chain reaches a mock of its own for the arguments it is called with: `Orders(1)` and `Orders(2)` are set up and counted apart, while calling `Orders(1)` twice reaches the same mock.
A call on that mock which no chain set up is answered as `Given<IOrderStore>()` set it up, and a step no chain matches returns the shared `IOrderStore` mock itself.
Verifying on the type, `Then<IOrderStore>(…)`, counts the calls on every `IOrderStore` mock, including those reached through a chain; verify through the chain to count the calls on one.

A delegate starts a chain as a member does, so a factory the subject depends on hands it a mock per argument:

```csharp
=> Given<Func<int, IOrderStore>>().That(_ => _(2).Find(Any<int>())).Returns(A<Order>)
```

A **protected** member can only be mocked by name:

```csharp
=> Given<HttpMessageHandler>()
   .ThatProtected<HttpResponseMessage>("SendAsync")
   .Returns(A<HttpResponseMessage>)
```

### 4.3 Mocking with arguments

Arguments in the mocked call match by value, so `The<int>()` matches the value the test passes.
When the argument does not matter, write `Any<T>()` — in a mock setup or verification it means any value of the type:

```csharp
=> Given<IBookingStore>().That(_ => _.Save(Any<Booking>(), Any<CancellationToken>())).Throws<IOException>()
```

To match only the values satisfying a constraint, give it to `Any`:

```csharp
=> Given<IBookingStore>().That(_ => _.Save(Any<Booking>(b => b.Nights > 7))).Throws<IOException>()
```

A constraint only has a meaning in a mock setup or verification; anywhere else it throws `SetupFailed`.

To vary mocked behavior based on arguments, supply a lambda with arguments to `Returns`. The lambda signature must match the mocked call.
Up to five arguments are supported.

Example with two arguments:
```csharp
=> Given<IMyCalculator>()
   .That(_ => _.Add(TheFirst<int>(), TheSecond<int>()))
   .Returns((a, b) => a + b) //The mock adds the two arguments passed to the function and returns the sum
```

### 4.4 Mocking sequence of calls

When a mock is called several times in succession, it can be set up to behave differently on each call.
Describe the sequence using `First` and `AndNext`.

Example mocking three successive calls:
```csharp
=> Given<IMyService>().That(_ => _.GetValueAsync())
    .First().Returns(() => 1) // returns 1 on first call
    .AndNext().Throws(An<ArgumentException>) //throw exception on second call
    .AndNext().Returns(); //return on third call
```

### 4.5 Observing calls with Tap

`Tap` observes the arguments passed to a mocked call without affecting its behavior.
Methods with up to five arguments can be tapped.

Example:
```csharp
int _tappedValue = -1;

=> Given<IMyInterface>()
   .That(_ => _.Get(An<int>()))
   .Tap<int>(i => _tappedValue = i)
   .Returns(() => _retVal)
```

A sequence can be tapped too. A tap after `First` or `AndNext` taps the call of that step:

```csharp
List<int> _asked = [];

=> Given<IMyInterface>()
   .That(_ => _.Get(Any<int>()))
   .First().Tap<int>(_asked.Add).Returns(() => 1)
   .AndNext().Tap<int>(_asked.Add).Returns(() => 2)
```

A tap before `First` taps every call:

```csharp
=> Given<IMyInterface>()
   .That(_ => _.Get(Any<int>()))
   .Tap<int>(_asked.Add)
   .First().Returns(() => 1)
   .AndNext().Returns(() => 2)
```

### 4.6 Verification

To verify a call to a mocked dependency, call `Then<[TheService]>([SomeLambdaExpression])`. 

Example:
```csharp
namespace MyProject.Spec.ShoppingService;

public class WhenPlaceOrder : Spec<MyProject.ShoppingService>
{
    public WhenPlaceOrder() 
        => When(_ => _.PlaceOrder(An<int>()))
        .Given<ICartRepository>().That(_ => _.GetCart(The<int>()))
        .Returns(() => A<Cart>(_ => _.Id = The<int>()));

    [Fact] public void ThenOrderIsCreated() => Then<IOrderService>(_ => _.CreateOrder(The<Cart>()));

    [Fact] public void ThenLogsOrderCreated()
        => Then<ILogger>(_ => _.Information($"OrderCreated from Cart {The<int>()}"));
}
```

#### 4.6.1 Invocation counts — `wasInvoked:`

To assert *how many times* something was invoked, add `wasInvoked:` — a `Times`: `Once`, `Never`,
`AtLeastOnce`, `AtMostOnce`, `Exactly(n)`, `AtLeast(n)`, `AtMost(n)` or `Between(from, to)`, both bounds included.

```csharp
using static TSpec.Times;

Then<IEventQueue>(q => q.MarkRejected(42, Any<string>(), Any<CancellationToken>()), Once)
    .And<IEventQueue>(nameof(IEventQueue.MarkFailed), Never)   // named method, any args
    .And<IEntityWriter>(wasInvoked: Never);                    // whole service, any interaction
```

* **Expression** — matches arguments. Without `wasInvoked:`, `Then<TService>(expr)` verifies the call was made at least once.
* **Named method** — matches any invocation of the method regardless of arguments; `nameof` keeps it refactor-safe.
  On an overloaded method the count aggregates across all overloads, so use the expression form when a specific overload matters.
* **Whole service** — counts every interaction, including property gets/sets and indexer access,
  so `wasInvoked: Never` asserts the service was not touched at all. Here `wasInvoked:` must be named.

Without `using static TSpec.Times;`, write `wasInvoked: Times.Once`.

### 4.7 Mocks from another library

When a test needs something TSpec's mocking does not offer, make that mock with the mocking library of your choice and hand it to the subject with `Using`, as any other constructor argument:

```csharp
=> When(_ => _.Refresh())
   .Using<INotifier>(notifierFromYourLibrary)
```

TSpec treats it as a plain value, so arrange and verify it with its own library.

## 5. Asserting Results

TSpec comes with its own fluent assertion framework under the `TSpec.Assert` namespace. 
It can be used on its own as an alternative to `FluentAssertions` or `AwesomeAssertions`,
but it really shines in combination with the TSpec pipeline.

What follows is a short guide to the fluent structure of assertions, followed by a feature reference.

### 5.1 Fluent assertions

Assertions are made directly on the value to be verified.
Every assertion returns a continuation, allowing chaining of assertions.
The continuation is context-aware and allows different assertions depending on what was asserted previously.

#### 5.1.1 And
When you want to combine more than one assertion, all of which must pass
```csharp
3.Is().GreaterThan(2).and.LessThan(4);
```

#### 5.1.2 Either - Or
When you want to combine two assertions, one of which must pass
```csharp
3.Is().either.GreaterThan(4).or.LessThan(4);
```

#### 5.1.3 Not
Any assertion can be negated by placing `not` before it (note the lowercase)
```csharp
3.Is().not.GreaterThan(4);
```

### 5.2 Values
Values of any type can be verified with the extension methods `Is` and `Has`

#### 5.2.1 Is

| Assertion | Example |
|---|---|
| Equal | `Result.Is(3)` |
| | `Result.Is().EqualTo(3)` |
| Equivalent — structural equality, for objects | `Result.Is().Like(new MyObject {Id = 3})` |
| | `Result.Is().EquivalentTo(new MyObject {Id = 3})` |
| Not equal | `Result.Is().Not(3)` |
| Null | `Result.Is().Null()` |
| A / An — asserts the type (subtypes accepted) and exposes the value, strongly typed, through `that` | `var enc = one.Is().A<EncounterComposition>().that;` |
| | `var order = one.Is().An<Order>().that;` |
| Greater / less than | `3.Is().GreaterThan(2)` |
| | `2.Is().LessThan(3)` |
| Around — approximately equal, with tolerance | `Result.Is().Around(3, 0.1)` |
| Even — divisible by 2 | `Result.Is().Even()` |
| OneOf | `Result.Is().OneOf(Three<int>())` |
| True / False — for booleans | `Result.Is().True()` |
| | `Result.Is().False()` |

#### 5.2.2 Has

| Assertion | Example |
|---|---|
| Condition | `Result.Has(_ => _.Id == 3)` |

### 5.3 Strings

#### 5.3.1 Is

| Assertion | Example |
|---|---|
| Like / EquivalentTo — ignoring casing and surrounding whitespace | `" ABC ".Is().Like("abc")` |
| | `" ABC ".Is().EquivalentTo("abc")` |
| Empty | `"".Is().Empty()` |
| NullOrEmpty | `((string)null).Is().NullOrEmpty()` |
| NullOrWhitespace | `" ".Is().NullOrWhitespace()` |

#### 5.3.2 Does

| Assertion | Example |
|---|---|
| Contain | `"ABC".Does().Contain("AB")` |
| StartWith | `"ABC".Does().StartWith("AB")` |
| EndWith | `"ABC".Does().EndWith("BC")` |
| Contain / StartWith / EndWith with a `StringComparison` | `"ABC".Does().Contain("bc", StringComparison.OrdinalIgnoreCase)` |
| Match — regular expression (string pattern or `Regex` for custom options) | `"abc123".Does().Match(@"[a-c]+\d+")` |
| | `"abc".Does().Match(new Regex("^ABC$", RegexOptions.IgnoreCase))` |

#### 5.3.3 Has

| Assertion | Example |
|---|---|
| Length | `"ABC".Has().Length(3)` |
| Length at least / at most / in range | `"ABC".Has().Length().AtLeast(2)` |
| | `"ABC".Has().Length().AtMost(5)` |
| | `"ABC".Has().Length().InRange(2, 4)` |

`Has()` on a string also offers the char-collection assertions (`Count`, `All`, ...) from
[section 5.5.3](#553-has).

### 5.4 Time

| Assertion | Example |
|---|---|
| Before / After | `DateTime.Now.Is().Before(DateTime.Now.AddDays(1))` |
| | `DateTime.Now.Is().After(DateTime.Now.AddDays(-1))` |
| CloseTo — within a given tolerance | `DateTime.Now.Is().CloseTo(DateTime.Now.AddDays(1), TimeSpan.FromDays(2))` |
| | `TimeSpan.FromDays(4).Is().CloseTo(TimeSpan.FromDays(3), TimeSpan.FromDays(2))` |
| Positive / Negative — for TimeSpan | `TimeSpan.FromDays(1).Is().Positive()` |
| | `TimeSpan.FromDays(-1).Is().Negative()` |

### 5.5 Collections

Deferred sequences (e.g. LINQ queries) are cached as they are asserted: each element is
produced at most once, so chained assertions see the same elements, and short-circuiting
assertions (such as `Does().Contain`) work even on infinite sequences.

#### 5.5.1 Is

| Assertion | Example |
|---|---|
| EqualTo — equal elements in the same order | `list.Is().EqualTo(otherList)` |
| Like / EquivalentTo — equal elements, order may differ | `list.Is().Like(otherList)` |
| | `list.Is().EquivalentTo(otherList)` |
| SameAs — same reference | `list.Is().SameAs(otherList)` |
| Empty | `list.Is().Empty()` |
| Distinct — all elements are different, optionally by a given property | `list.Is().Distinct()` |
| | `list.Is().Distinct(it => it.Id)` |

#### 5.5.2 Does

| Assertion | Example |
|---|---|
| Contain | `list.Does().Contain(3)` |

#### 5.5.3 Has

| Assertion | Example |
|---|---|
| Count | `list.Has().Count(3)` |
| Count at least / at most / in range | `list.Has().Count().AtLeast(2)` |
| | `list.Has().Count().AtMost(2)` |
| | `list.Has().Count().InRange(2, 4)` |
| Count with condition — counts only matching items | `list.Has().Count(it => it > 3).At(2)` |
| | `list.Has().Count(it => it > 3).AtLeast(2)` |
| Order — ascending or descending; `Order()` for comparable items, `Order(by)` for any comparable key | `list.Has().Order().Ascending()` |
| | `patients.Has().Order(p => p.Name).Descending()` |
| [One/Two/Three/Four/Five]Items — asserts the count and returns the items as an n-tuple | `numbers.Has().OneItem().that.Is(3)` |
| | `patients.Has().OneItem().that.Age.Is(3)` |
| | `patients.Has().OneItem(it => it.Age == 3).that.Gender.Is('F')` |
| All — every item matches the criteria, optionally with index, or applying an assertion | `list.Has().All(it => it.Age > 3)` |
| | `list.Has().All((it, i) => it.Age > i)` |
| | `list.Has().All(it => it.Age.Is().GreaterThan(3))` |
| Some — at least one item matches | `list.Has().Some(it => it.Age > 3)` |
| None — no item matches | `list.Has().None(it => it.Age > 3)` |

Accessing `that` after an inverted assertion (e.g. `not.OneItem().that`) throws `SetupFailed` —
there is no item to expose when the assertion states its absence.

#### 5.5.4 Dictionaries

Dictionary assertions bind to `IReadOnlyDictionary<TKey, TValue>` (covers `Dictionary`,
`FrozenDictionary`, `ImmutableDictionary`, ...). Key lookups respect the dictionary's own key comparer.

| Assertion | Example |
|---|---|
| Key — the dictionary contains the given key | `dict.Has().Key("a")` |
| Value — the dictionary contains the given value | `dict.Has().Value(3)` |
| no — inverts the following assertion (the possession-flavored `not`) | `dict.Has().no.Key("c")` |
| | `dict.Has().no.Value(3)` |
| Has(key) — asserts the key exists and exposes its value through `that` | `dict.Has("a").that.Is(3)` |

- The collection assertions (`Count`, `OneItem`, `All`, ...) remain available, and chaining
  through one of them keeps the dictionary vocabulary: `dict.Has().Count(2).and.Key("a")`.
- Variables *declared* as `IDictionary<TKey, TValue>` get only the collection assertions.

### 5.6 Justifying assertions with because

Each test method contains exactly one logical assertion. To document *why* the expected outcome
is the correct outcome, provide a rationale with the named argument `because` in `Then`:

```csharp
[Fact] public void ThenCircumferenceIsAroundSixPi()
    => Then(because: "the world is round").Result.Is().Around(Math.PI * 6, 0.001);
```

The reason is appended after the assertion in the generated specification:

```csharp
When Circumference
Then Result is around 18.8496, because the world is round
```

Phrase the reason so it reads naturally after the word "because", and let it justify the expectation
rather than restate it — the test name already says *what* is expected; `because` explains *why*.
A reason can be provided once per test method and covers every assertion chained after it.

`Because("the world is round")` is shorthand for `Then(because: "the world is round")`.

### 5.7 Asserting exceptions

When the behavior under test is expected to throw, assert the thrown exception through the
`Then().Throws` overloads rather than `Result` (accessing `Result` after a throw fails the test).

| Assertion | Meaning |
|---|---|
| `Then().Throws<TError>()` | threw an exception assignable to `TError` |
| `Then().Throws()` | threw any exception |
| `Then().Throws<TError>(e => e.Code == 42)` | threw `TError` satisfying a predicate |
| `Then().Throws<TError>(e => e.Message.Is("Nope"))` | threw `TError` satisfying inline assertions |
| `Then().Throws(The<TError>)` | threw *this exact instance* (by reference — pass a mention of the arranged exception) |
| `Then().Completes()` | ran to the end — returned rather than threw |

```csharp
[Fact] public void ThenRejectsEmptyCart()
    => Then().Throws<InvalidOperationException>();
```

#### Asserting on the thrown exception with `that`

`Then().Throws<TError>()` (and the untyped `Then().Throws()`) expose the caught exception through
`that`, so the full assertion vocabulary applies to it, its `Message`, or any other property:

```csharp
Then().Throws<ArgumentException>().that.Message.Is("Invalid cart");
Then().Throws<ArgumentException>().that.Message.Does().Match(@"cart \d+");
Then().Throws<ArgumentException>().that.ParamName.Is("cartId");
```

Prefer `that` to the predicate form `Throws<TError>(e => e.Message.Contains(...))`: a failure shows the
actual message, and the specification reads as prose rather than as a lambda. The predicate and
inline-assertion overloads remain for conditions that don't decompose into a single property.

## 6. Tests as specification

Every TSpec test carries a readable specification of what it claims, shown when the test fails.
Opt in, and a `_specification/` folder of markdown files is generated per test project from those claims,
regenerated by running all its tests.

### 6.1 Recommended test structure

This opinionated structure keeps specifications readable, navigable, and aligned with production code as test suites grow —
and since the generated specification follows it, the names become its headings.

1. Mimic the folder structure of your production code, with one test project per production project, called *[ProductionProject].Spec*
1. Create one folder per class under test, called *[NameOfClass]*
1. Create one abstract test class per method under test, called *When[NameOfMethod]*
1. Nest one concrete class per given-case inside it, called *Given[SomePrecondition]* — given-classes can be nested in several levels
1. Write one test method per logical assertion — it may contain several actual assertions
1. Prefer arrow syntax and fluent chaining over statement blocks

Example:
```csharp
public abstract class WhenPlaceOrder : Spec<ShoppingService> 
{
    static Tag<Guid> cartId = new();
  
    protected WhenPlaceOrder() => When(_ => _.PlaceOrder(The(cartId)));

    public abstract class GivenCartExists : WhenPlaceOrder 
    { 
        protected GivenCartExists()
            => Given<ICartRepository>().That(_ => _.GetCart(The(cartId))).Returns(A<Cart>);

        public class WithItems : GivenCartExists 
        {
        ...
        }

        public class WithoutItems : GivenCartExists 
        {
        ...
        }
    }

    public class GivenCartNotExists : WhenPlaceOrder 
    {
        public GivenCartNotExists()
            => Given<ICartRepository>().That(_ => _.GetCart(The(cartId))).Returns(() => Cart.NoCart);

        ...
    }
}
```

Unit tests work best when they run *fast*. Write modular production code in line with best practices,
so that each unit can be tested in isolation while mocking or ignoring the rest. Remember that the
entire test pipeline is built and disposed for each test method — if a specification requires costly
setup or execution, it can be reasonable to group *closely related* assertions into one test method.

### 6.2 Generating the specification

Add one line to the spec project:

```csharp
[assembly: AssemblyFixture(typeof(SpecificationDocument))]
```

A `_specification/` folder is then written to the spec project root when the run ends. Without that
line nothing is collected and nothing changes. The root is found through the build's debug information,
wherever the binaries are written; a build without debug information falls back to the binaries' folder.

**The spec project must be named after the project it describes** — `MyHotel.Spec` describes
`MyHotel` — and must reference that project **directly**; a transitive reference is not enough. Any
suffix works, `.Spec` preferred and `.Test` fine. This is checked before the first test runs, so a
project that fails it throws `SetupFailed` immediately rather than after the suite has finished.

The folder holds one markdown file per top-level folder of the spec project, named as the folder —
`Rooms.md`, `Bookings.md` — plus one named as the project under test for the classes tested directly
at its root (`MyHotel.md` for `MyHotel.Spec`, `Core.md` for `MyHotel.Core.Spec`). A top-level folder
named `README` or as the project under test therefore throws `SetupFailed`.

Each file reads as its own document: what every requirement in it declares or arranges is stated once
at the top, and the headings below follow the test structure. A `[Fact]` renders as a bullet point with
the method name in words, followed by the assertion in a code block. A `[Theory]` fed by `[InlineData]`
renders as a table of its rows, headed by the parameter names; a theory of more than **eight**
parameters cannot be tabled and fails generation.

A `README.md` in the folder is the entry point: the project's title, the `<Description>` of its project
file when it has one, what holds throughout the specification, and a table linking each file with how
many `When`, `Given` and `Then` it holds.

**The specification is written only when every non-skipped test in the assembly passed.** A filtered or failed run
leaves the existing files untouched and the gaps are named:

```
TSpec: _specification/ left unchanged — 1 requirement(s) did not report a pass, so the specification
would be incomplete. Run the whole suite green to regenerate it.
  - MyHotel.Spec.WhenGetVersion.ThenReturnTheApplicationVersion
```

A full green run replaces the folder's contents, so a file for a folder that was renamed or removed does not linger.

#### Keeping it fresh

The files are deterministic — sorted and deduplicated — so a stale one shows up as a diff.
Let the build catch it:

```bash
dotnet test && git diff --exit-code -- "**/_specification/*.md"
```

If someone changed the tests without regenerating, that fails, and the diff is the error message.
A file for a new folder is untracked and `git diff` does not see it; `git status --porcelain` does.
