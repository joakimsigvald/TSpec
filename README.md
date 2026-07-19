# TSpec — Fluent, specification-style unit testing for .NET

TSpec is a fluent, specification-oriented testing framework for .NET that builds on xUnit.
It follows the Given–When–Then pattern and integrates seamlessly with Moq.
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
6. [Guidelines](#6-guidelines)

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
* `Using`  — registers type conversions, defaults, and factories.
* `Having` — setup that runs *before* the action.
* `Until`  — teardown that runs *after* the action.

TSpec's strongly typed API guides the setup, preventing most invalid test configurations at compile time.
Chapter 2 describes the test pipeline in depth, and Chapter 3 how to prepare and reference test data.

### 1.2 Act

The *act* stage specifies the behavior under test by calling `When` with a lambda expression. 
The lambda takes the subject under test as argument and invokes the behavior to verify.

The subject under test is automatically created based on the arrangement, unless it is static or explicitly provided.

As with arrangement, the order in which `Given`, `Having`, `Until`, and `When`
are declared does not matter. Because execution is deferred until assertion, TSpec can deterministically reorder the pipeline before running it.
So the execution order of the steps is always: `Given` -> `Having` -> `When` -> `Until`.

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
Given three MyModel
  and IMyRepository.List() returns a MyModel[]
When _.List()
Then Result has count 4
```

In addition to verifying return values, exceptions can also be asserted using `Then().Throws`.

With basic familiarity with NuGet, unit testing and mocking, you are now ready to write your own tests using TSpec. 
The remainder of this README is a complete, practical guide to structuring specifications, managing test data, and verifying behavior with TSpec.

## 2. The Test Pipeline

At the core of TSpec are *deferred execution* and *lazy evaluation*: no production code is executed until the first assertion is made.
A test runs through four conceptual stages: preparation, execution, assertion, and teardown.

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

The chain reads as one sentence: *having signed in, having configured SmtpSettings, when sending a Report, until the outbox is flushed, until disconnected.*

### 2.1 Preparation

Before the first assertion, the pipeline is configured with test data, mocks, and lambdas to execute,
using the arrangement methods introduced in Section 1.1.

#### 2.1.1 Creating the Pipeline
You typically create the pipeline by subclassing `Spec` with two generic arguments. The first is the type of the *subject under test* and the second is the return type of the *method under test*.
Other overloads exist for cases without a subject under test or return value.

#### 2.1.2 Scope of Arrangement
When preparing the pipeline, values are provided either for the **Subject** or for the **Input**. 
* **Subject**: The subject under test or any of its components, provided as constructor arguments, properties, or through type cast.
* **Input**: Data supplied directly to the execution pipeline.

Arrangements are categorized into three types, using specific verbs to dictate their scope:
* **Values**: Configured using the verb **`Given`** and apply *only* to the **Input**.
* **Types**: Configured using the verb **`Using`**. An optional scope indicates whether they apply to the **Input**, the **Subject**, or both (the default).
* **Mocks**: Configured using the verb **`Given`** and apply to *both* **Input** and **Subject**.

#### 2.1.3 Preparing the Pipeline
The preparation steps are recorded and later applied in the following order:

1. Defaults, constraints, and test data, *in reverse order of declaration*.
1. Mocked behavior, *in order of declaration*.

#### 2.1.4 Creating the Subject Under Test
After preparation, the pipeline uses auto-mocking to create a new instance of your subject under test (unless you provided a value of that type explicitly).
If you haven't mocked a certain interface or method that the subject uses, a default mock will be auto-generated.

### 2.2 Execution

Execution is triggered by the first assertion (technically when `Result` is referenced or `Then()` is called). 
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
If a value is returned, it must match the declared return type and is exposed for assertion through the `Result` property.
If an exception is thrown, it becomes the captured outcome and can be asserted using the `Then().Throws` overloads.
`Throws<TError>()` asserts by type and `Throws<TError>(condition)` by content, while `Throws(expected)`
compares by reference — pass a mention (e.g. `The<TimeoutException>`) to verify that the exact
exception instance configured in the arrangement was propagated.

Accessing `Result` will implicitly execute the pipeline if it has not already been executed.

### 2.3 Assertion

Assertions consume the captured outcome or utilize the mocking framework for verifying execution paths. 
The pipeline executes at most once per test method, regardless of the number of references to `Result` or `Then()`.
Assertions are covered in depth in Chapter 5, and mock verification in Section 4.6.

### 2.4 Teardown

Teardown steps are provided with `Until()`, as lambdas that take the subject under test as argument.
Teardown is executed in order of declaration when the test class and pipeline are disposed, after the test method has run.

Example:
`When(A).Until(B).Until(C)` will result in the execution order: A -> B -> C.

After all `Until`-steps have run, TSpec disposes any disposable objects it created for the
subject-under-test graph: the subject itself and any concrete dependencies TSpec constructed
and injected into it, in reverse order of creation (subject first). This applies to both
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
Ownership is reflected in the generated specification: `Using owned CreateClient`.

### 2.5 Sync vs. Async Execution

TSpec supports testing synchronous and asynchronous code using the same test pipeline.

When the behavior under test is asynchronous (returns `Task` or `Task<T>`), TSpec waits for completion and captures the outcome in the same way as for synchronous code.
The only difference is the lambda signature provided to `When`, `Having`, `Until`, and mock setup methods.
Test methods themselves do not need to be `async`, but they may be — the test pipeline and
specification follow the test's async flow, so it is safe to await other work before asserting.

## 3. Using Test Data

TSpec provides helpers for referring to test data that can either be supplied explicitly or automatically generated (optionally with constraints).

Two complementary mechanisms are provided:
- Mentions, for quickly referring to generated values by position or quantity
- Tags, for assigning stable, meaningful identities to values of the same type

### 3.1 Mentions

Mentions are helper methods for generating and referring to up to five numbered values of a given type, as well as collections of up to five elements.
Mentions are resolved per type and per test and always refer to the same value within a specification.

**Single values**
For a single generated value:
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
`Any`, `Another`

**Uniqueness**
All values of the same or equivalent types are guaranteed to be unique within a test run.

### 3.2 Tags

Tags complement mentions by allowing values to be referred to by name rather than position.
They are primarily useful when working with multiple values of the same type.

A tag is an instance of `Tag<TValue>`.
Each tag represents exactly one value of the given type.

Example:
```csharp
protected static Tag<string> name = new(nameof(name));
protected static Tag<int> age = new(nameof(age)), shoeSize = new(nameof(shoeSize));
```
Providing a name for the tag improves diagnostic output.
The parameterless constructor `new()` may also be used.

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
3. **Customize**: any setup or transform lambdas provided for the type are applied, most recently provided first.

Built-in generation covers most types out of the box:
* **Primitives**: standard primitives like `int`, `string`, `Guid`, `DateTime`, `Uri`, and `TimeSpan`.
* **Enums, nullables and collections**: resolved from their underlying or element types.
* **Semantic types**: objects deriving from `Semantic<TPrimitive>` (such as `Email`, `PhoneNumber`, `Age`) are generated from their primitive values.
* **Interfaces and abstract classes**: mocked automatically, without boilerplate.
* **Concrete classes**: constructed with generated constructor arguments.
* If none of the above applies, the type's default value is used.

### 3.4 Type Registration and Conversion

If a specific custom mapping is required during data creation, you can override default generation by using the fluent type conversion pipeline.

* **Fluent Registration:** Configure the generator pipeline via `Using<TTarget>().From<TSource>()` — use values of the target type from the set described by the source type.
* **Smart Casting:** Once mapped, TSpec automatically probes the requested type for compatibility. It attempts to construct the target by finding implicit cast operators, single-parameter constructors, or matching static factory methods (e.g., `Create()`).
* **Conversion:** For explicit control, inject a conversion delegate directly, such as `Using<int>().From((byte b) => b + 1)` — the source type is inferred from the lambda parameter.
* **Chaining:** Register several conversions fluently with `And`, e.g. `Using<int>().From<byte>().And<long>().From<short>()`.
* **Scoping:** Like the value-level overloads, `Using<TTarget>()` accepts an optional `For` scope, e.g. `Using<int>(For.Input).From<byte>()` applies the conversion only when generating ambient test data, leaving subject construction unaffected. The default is `For.All`. Registrations for the same target type must have disjoint scopes: one for `Input` and another for `Subject` can coexist, but overlapping scopes throw `SetupFailed`.
* **Sequences:** For numeric and temporal source types (`DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`, `TimeSpan`), constrain the generated source values with `StartingAt` and `Spaced`, e.g. `Using<int>().From<int>().StartingAt(10).Spaced(5)`. `Spaced` accepts a fixed spacing (negative for descending) or a step function, e.g. `Spaced(i => i * 2)`. Generated values are guaranteed unique: a sequence that would repeat a value or leave the type's range throws `ValuesExhausted`.
* **Generator functions:** For arbitrary value spaces — non-numeric types or stateful series — pass a generator to `From`, e.g. `Using<Guid>().From(Guid.NewGuid)` or `Using<int>().From(NextFibonacci)` with a method or closure holding the state. Generated values are converted to the target type if necessary and used exactly as produced: the user defines the value space, so duplicates are allowed (1, 1, 2, 3, 5, ...).
* **Value lists:** To use exactly the given values, pass an explicit list to `From`, e.g. `Using<int>().From([10, 20, 30])`. The values are used in declaration order, duplicates allowed, and requesting more values than the list contains throws `ValuesExhausted`.
* **Safe Failures:** If incompatible types are relayed and no conversion path exists, generation throws `InvalidTypeConversion`.

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

This chapter assumes familiarity with Moq or a similar mocking framework, and shows how TSpec simplifies the mocking experience.

### 4.1 Auto-Mocking subject under test

The subject under test will be created automatically with mocks and default values.
Remember from Chapter 2 that mocks are configured after test data has been generated, 
so test data and constraints are available in the mocking stage regardless of where in the test they are provided.

You can supply your own constructor arguments by calling `Using`, or modify the generated ones by calling `Given` with a setup or transform lambda.
You can even provide the subject under test itself:
`Using(new MyClass(42, "Thursday"))`

### 4.2 Mocking

To mock the behavior of a dependency, call `Given<[TheService]>().That(_ => _.[TheMethod](...)).Returns/Throws(...)`. 
`That` accepts any lambda you would normally supply to `Setup` when creating a mock with Moq. 
You do not need to create and manage mocks manually, but can supply mocked behavior directly to the pipeline.
This allows most mocking scenarios to be expressed inline, close to the behavior under test.

### 4.3 Mocking with arguments

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

### 4.6 Verification

To verify a call to a mocked dependency, call `Then<[TheService]>([SomeLambdaExpression])`. 

Both mocking and verification are based on the Moq framework.
 
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

The built-in mocking capabilities of TSpec cover almost all scenarios that Moq covers. 
Should you need a feature that TSpec does not provide, create the mock explicitly with Moq and supply it to the pipeline using `Using(myMock.Object)`.

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
| Type | `Result.Has().Type<MyModel>()` |

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

Deferred sequences (e.g. LINQ queries) are lazily cached during assertion: each element is
produced at most once, so chained assertions and failure descriptions all see the same elements,
and short-circuiting assertions (such as `Does().Contain`) work even on infinite sequences.
Already-materialized collections are asserted as-is.

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
| Order — ascending or descending, optionally by a given property | `list.Has().Order().Ascending()` |
| | `list.Has().Order(it => it.Age).Descending()` |
| [One/Two/Three/Four/Five]Items — asserts the count and returns the items as an n-tuple | `numbers.Has().OneItem().that.Is(3)` |
| | `patients.Has().OneItem().that.Age.Is(3)` |
| | `patients.Has().OneItem(it => it.Age == 3).that.Gender.Is('F')` |
| All — every item matches the criteria, optionally with index, or applying an assertion | `list.Has().All(it => it.Age > 3)` |
| | `list.Has().All((it, i) => it.Age > i)` |
| | `list.Has().All(it => it.Age.Is().GreaterThan(3))` |
| Some — at least one item matches | `list.Has().Some(it => it.Age > 3)` |
| None — no item matches | `list.Has().None(it => it.Age > 3)` |

### 5.6 Justifying assertions with because

Each test method contains exactly one logical assertion. To document *why* the expected outcome
is the correct outcome, provide a rationale with the named argument `because` in `Then`:

```csharp
[Fact] public void ThenCircumferenceIsAroundSixPi()
    => Then(because: "the world is round").Result.Is().Around(Math.PI * 6, 0.001);
```

The reason is included in the generated specification, appended after the assertion
in natural reading order:

```csharp
When world.Circumference
Then Result is around 18.8496, because the world is round
```

Phrase the reason so it reads naturally after the word "because".
It should justify the expectation rather than restate it — the test name already says *what*
is expected; `because` explains *why*. The reason can only be provided once per test method,
in line with one logical assertion per test method, and covers all technical assertions
chained after it (with `and`, `either`/`or` etc.).

`Because(reason)` is syntactic sugar for `Then(because: reason)`:

```csharp
[Fact] public void ThenCircumferenceIsAroundSixPi()
    => Because("the world is round").Result.Is().Around(Math.PI * 6, 0.001);
```

## 6. Guidelines

### 6.1 Recommended test structure

This is an opinionated recommendation for structuring your tests, based on how xUnit and TSpec work and on experience from real projects.
The goal of these conventions is to keep specifications readable, navigable, and aligned with production structure as test suites grow.

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

### 6.2 Some final advice

Unit tests work best when they run *fast*. Write modular production code in line with best practices, 
so that each unit can be tested in isolation while mocking or ignoring the rest.

Remember that the entire test pipeline is built and disposed for each test method (xUnit creates a new test-class instance per test).
If a specification requires costly setup or execution, it can be reasonable to group *closely related* assertions into the same test method.

TSpec is designed to thrive in clean, well-structured codebases,
and its emphasis on explicit structure and readable specifications helps keep them that way as both code and test suites grow.

