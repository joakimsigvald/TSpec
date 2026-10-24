# TSpec — Agent Reference

Condensed reference for AI coding agents writing tests with TSpec (covers TSpec 5.4).
TSpec is a fluent Given–When–Then specification framework for .NET on top of xUnit v3.
Full documentation: [README.md](https://github.com/joakimsigvald/TSpec#readme).

## Core model — read this first

- A test class subclasses `Spec<TSUT, TResult>` (subject type, return type of the method under test). `Spec<T>` is `Spec<T, T>`, and also the spelling when the result is not asserted; non-generic `Spec` has neither subject nor result.
- **Execution is deferred**: nothing runs until the first `Then()` or `Result`, and the pipeline runs **at most once**. All arrangement must come before it.
- **The Facts of a class share one run** when each reads the outcome before anything else. A Fact that sets up or mentions a value first, and each Theory row, runs alone. A Fact that changes what a later one reads (consumes a stream, changes the subject) affects it — give such a Fact a class of its own.
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
// By name, whatever the arguments; That(name) where it returns nothing. A default: a specific setup wins over it
Given<HttpMessageHandler>().That<HttpResponseMessage>("SendAsync").Returns(A<HttpResponseMessage>)
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

Called directly on values; every assertion returns a continuation. Combinators are lowercase: `.and.`, `.not.`, `.either. … ._. … .or.` (alternatives, `or` before the last; equality is `EqualTo(x)` there), `.that.`, `.but.`. `.or.Fix("what to do", because: "why")` ends an either with a remedy the failure states.
Works standalone in plain xUnit tests too.

- Any value: `Is(x)`, `Is().Not(x)`, `Is().SameAs(obj)` (same reference), `Is().Null()`, `Is().Like(obj)` (structural; the actual may have more properties), `Has(_ => _.Id == 3)`, `Is().A<T>().that` (asserts the type, exposes the value as `T`; `An<T>()` is a synonym).
- Numeric: `Is().GreaterThan(x)`, `LessThan(x)`, `Around(x, tolerance)`, `Even()`, `OneOf(values)`, `True()`, `False()`.
- Strings: `Is().Like("abc")` (case/whitespace-insensitive), `Empty()`, `NullOrEmpty()`, `NullOrWhitespace()`; `Does().Contain/StartWith/EndWith(s)` (optional `StringComparison`), `Does().Match(pattern)`; `Has().Length(n)`, `Has().Length().AtLeast/AtMost/InRange(...)`.
- Time: `Is().Before/After(t)`, `CloseTo(t, tolerance)`; TimeSpan `Positive()`/`Negative()`.
- Collections: `Is().EqualTo(list)` (same order), `Are(list)` or `Is().Like(list)` (any order, no extra item), `SameAs(list)`, `Empty()`, `Distinct()`; `Does().Contain(x)`; `Has().Count(n)`, `Count().AtLeast/AtMost/InRange(...)`, `Count(predicate).At(n)`, `Order().Ascending()/Descending()`, `Order(it => it.Key)`, `All/Some/None(predicate)`, `OneItem()` … `FiveItems()` (asserts the count and returns the items: `list.Has().OneItem().that.Age.Is(3)`).
- Dictionaries: `Has().Key(k)`, `Has().Value(v)`, `Has().no.Key(k)`, `Has(key).that.Is(v)`. These bind to `IReadOnlyDictionary`; a variable declared `IDictionary` gets only the collection assertions.
- `that` after a negated assertion (`not.OneItem().that`) throws `SetupFailed`.
- **No trainwrecks in `Then` subjects**: `Then(x.A.B)` throws `SetupFailed`; write `Then(x).A.B`.

## Lifecycle and ownership

- After the test method, or a shared run's last Fact: `Until` steps, then TSpec disposes the `IDisposable`/`IAsyncDisposable` objects **it created** for the subject graph (subject first).
- Instances provided with `Using`, mocks and generated input are never disposed — unless provided with `owned: true`. Integration-test idiom: `Using(CreateClient, owned: true)` in the base constructor gives each run a fresh `HttpClient`, disposed after it.

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
- **Written only when every test in the assembly that is neither skipped nor explicit passed** — a filtered or failing run leaves the files untouched. Run the whole suite before expecting a diff.
- A `[Theory]` with `[InlineData]` renders as a table of its rows; more than 8 parameters throws `SetupFailed`.
- Output is deterministic, so CI can check it: `dotnet test && git diff --exit-code -- "**/_specification/*.md"` (a new file is untracked; `git status --porcelain` sees it).

## Architecture (`TSpec.Architecture`)

Put the tests in a spec project of their own that references every project: `<ProjectReference Include="..\**\*.csproj" Exclude="..\Architecture.Spec\**" />`.

`Source.Projects` lists every project its build reaches, with their direct project and package references. The spec project itself is `Source.Projects.CurrentSpec`; `Within` and `NonRedundant` leave it out.

```csharp
[Fact]
public void AreOnlyTheseAndNotRedundant()
    => Source.Projects.Is().Within(p => p switch
    {
        "." => ["Entry", "Infra"],
        "Entry" or "Core" => ["Contract"],
        "Infra" => ["Core"],
        "Spec" => ["."],
        "Core.Spec" => ["Core"],
        _ => [],
    }, p => p.Segment(^1) == "Spec" ? ["P:*"] : ["P:Microsoft.*"]).and.NonRedundant();
```

- Names are assembly names under a common root within the solution. `root.X` → `X`, the root → `.`, other projects → `/Name`. Packages are always `P:Name`. A reference that produces no assembly (an `.esproj`) is not in the graph.
- Rules add up: a dependency passes if any rule allows it; `_ => [...]` applies to every project. A trailing `*` matches the rest (`P:Moq.*` does not match `P:Moq`). `Within` allows, never requires.
- `NonRedundant()`: no project references a project it already reaches through another; packages, and `A.B -> A` (`Core.Spec -> Core`), are exempt. `not.Redundant()` is obsolete.
- Layer rules: switch on `p.Segment(0)` (also `Segment(^1)`, `Segment(..2)`).
- Custom rules: `Source.Projects["X"]` (an unknown name throws `SetupFailed`) is a `SourceProject` with `Name`, `Dependencies` and `Reaches` (directly or through other projects, packages included), named as the rules see them. `IsSpecProject`: it reaches TSpec, whatever its name. `OpensInternalsTo`: the projects its `InternalsVisibleTo` names.
- The architecture page: `[assembly: AssemblyFixture(typeof(ArchitectureDocument))]` writes `_specification.md`, a diagram of the projects and the tests below it, on a clean full run. Only `Spec` classes count, so derive the test classes from `Spec`.
- A graph of your own: `new DependencyGraph("Namespace", dependencies)`, each node's name and the names it depends on; the first argument heads the table. `Is().Within(...)` takes the same rules.
- A `DependencyGraph`'s test draws what the rules allow above its table in the generated specification. A target ending in `*` draws no arrow.
- `graph.Is().Acyclic()`: no nodes depend on each other around a loop; a failure names the first (`A -> B -> A`).
- `Project.Dependencies` is obsolete: use `Source.Projects["X"].Dependencies`.

## C# code (package `TSpec.Architecture.CSharp`)

- A package of its own: `using TSpec.Architecture.CSharp;`.
- `Uses`, `Exposes`, `DependsOn`, `Gives` and `Takes` compile the project once a run; reflection is not seen.

### Project

- `Files` are the source files it compiles from its own folder, leaving out what the build generates under `obj/` and files linked in from elsewhere.
- `Types` are those of all its files, a partial type once; `Members` and `Nodes<T>()` are all its files'.
- `AllTypes` (on a file too): its types and those nested in them, however deep. `Types` are the top-level ones only, so a rule on every public type reads `AllTypes`.
- `Folders`: a `DependencyGraph` of the folders directly in its own, each with the folders whose types its files use. A file at the project's root is in no folder. Assert with `Folders.Is().Within(...)`.

### File

- `Path` in the project (written with `/`), `Lines`, `Raw` text and the `Types` declared at its top level. A failure names it `X/Path`.
- `Folder`, `Name` and `Extension` are parts of `Path`: for `Stores/Cache/UserStore.cs`, `Stores` (the top-level folder, as `Folders` names it; empty at the project's root), `UserStore` and `cs`.
- `Root` is its Roslyn syntax as the build parsed it, so code under an `#if` the build left out is not in it. `Model` is Roslyn's `SemanticModel` of that syntax; a type the spec project does not run with is unresolved. `file.SpanOf(node)` is where a node or token starts and ends: a `Start` and an `End`, each a `Line` and `Column` counted from 1.
- `Nodes<T>()`: its syntax nodes of one kind in written order, directives included, nothing inside a documentation comment. Each has `File`, `Syntax` and `Span` (with `Lines`), and a failure names it `X/Path:line`.
- `Members`: those it declares, nested types' and local functions too.
- `Uses`: the build's types its code uses, through a member too (an extension call, a constant). A nested type is one of its own.
- `DependsOn()`: the projects and packages whose types its code uses (`Contract`, `P:Dapper`), not its own project nor the framework; one reached only through another project counts. `DependsOn(name)` asks for one, a trailing `*` matching as in `Within`.
- To add an analysis, cache it per file in a `ConditionalWeakTable<SourceFile, T>` behind an extension member; a file is the same object every time.

### Type

- `Name` (no type parameters), `FullName`, `Namespace`, `Kind`, `IsPublic`, `Files` and `Nested` (the types declared inside it). A nested type's `IsPublic` is in effect. A failure names it by its full name.
- `Exposes`: the build's types named in its signature that other assemblies see, itself included.

### Member

- `Accessibility` is in effect: no more than the types around it allow. `Type` and `Parameters` are syntax, as written. A record's positional parameters are its constructor's and its properties.
- `IsOriginal`: it neither implements an interface's member (explicitly or implicitly) nor overrides a base type's, so it sets its own name and signature; a naming rule reads only original members.
- `Gives` is the types it returns, including those inside generic types, tuples and arrays; `Takes` is the same for its parameters. With no argument they list the types (`ITypeSymbol`). Ask if one is included with one argument that is a `TypeKind`, a `typeof(List<>)` (type arguments ignored) or a condition. A type it only implements doesn't count: `List<int>` gives no `IEnumerable<>`.
- A failure names it `X/Path:line Name`.

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
