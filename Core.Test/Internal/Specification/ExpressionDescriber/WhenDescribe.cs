using TSpec.Assert;
using TSpec.Internal.Specification;
using static TSpec.Internal.Specification.ExpressionDescriber;

namespace TSpec.Test.Internal.Specification.ExpressionDescriber;

public class WhenDescribe : Spec<string>
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("A<MyModel>", "a MyModel")]
    [InlineData("() => A<MyModel>()", "a MyModel")]
    // A count makes the type read as a plural; One is a count too, and reads singular
    [InlineData("Two<MyModel>()", "two MyModels")]
    [InlineData("Zero<MyModel>()", "zero MyModels")]
    [InlineData("Many<Query>()", "many Queries")]
    [InlineData("Some<Box>()", "some Boxes")]
    [InlineData("AnyNumberOf<MyModel>()", "any number of MyModels")]
    [InlineData("One<MyModel>()", "one MyModel")]
    [InlineData("() => A<MyModel>(_ => _.Name = A<string>())", "a MyModel { Name = a string }")]
    [InlineData("One(_theModel)", "one _theModel")]
    [InlineData("(_, i) => _.Name = $\"X{i + 1}\"", "Name = \"X{i + 1}\"")]
    [InlineData("(a, b) => a.Name = $\"X{b + 1}\"", "Name = \"X{b + 1}\"")]
    // A plural takes the bare apostrophe, not "MyModels's"
    [InlineData("Three<MyModel>().Last()", "three MyModels' Last()")]
    [InlineData("The<MyModel>().Last()", "the MyModel's Last()")]
    [InlineData("new MyComponent(An<IMyLogger>(), An<int>())", "new MyComponent(an IMyLogger, an int)")]
    [InlineData("new (An<IMyLogger>(), An<int>())", "new(an IMyLogger, an int)")]
    [InlineData("new(An<IMyLogger>(), An<int>())", "new(an IMyLogger, an int)")]
    [InlineData("new(An<IMyLogger>(), An<Action<int>>())", "new(an IMyLogger, an Action<int>)")]
    [InlineData("new(An<Action<int, string>>())", "new(an Action<int, string>)")]
    [InlineData("A<MyValue<int>>()", "a MyValue<int>")]
    [InlineData("A<MyValue<int, string>>()", "a MyValue<int, string>")]
    [InlineData("A<MyValue<int, Task<string>>>()", "a MyValue<int, Task<string>>")]
    [InlineData("A<(int, string, int, float)>", "a (int, string, int, float)")]
    [InlineData("i => $\"{2 * i}\"", "\"{2 * i}\"")]
    // An operand keeps the parentheses that group it, and gets none it does not need
    [InlineData("(a + b) * c", "(a + b) * c")]
    [InlineData("a + b * c", "a + b * c")]
    [InlineData("a * (b + c)", "a * (b + c)")]
    [InlineData("a - (b - c)", "a - (b - c)")]
    [InlineData("(a ? b : c) + 1", "(a ? b : c) + 1")]
    [InlineData("(a + b)", "a + b")]
    [InlineData("_ => _ with { Name = A<string>() }", "Name = a string")]
    [InlineData("_ => _ with { Name = A<string>(), Id = 1 }", "Name = a string, Id = 1")]
    [InlineData("The<MyModel>() with { Name = A<string>() }", "the MyModel with { Name = a string }")]
    [InlineData("_ => _.Inner with { Name = A<string>() }", "_.Inner with { Name = a string }")]
    // A trailing comma is legal C# and closes nothing — the list ends at its terminator
    [InlineData("The<MyModel>() with { Name = A<string>(), }", "the MyModel with { Name = a string }")]
    [InlineData("new MyModel { Name = A<string>(), Id = 1, }", "new MyModel { Name = a string, Id = 1 }")]
    // An untyped array creation renderes as a collection expression
    [InlineData("new[] { An<int>(), A<string>(), }", "[an int, a string]")]
    [InlineData("new[] { The(x) }", "[the X]")]
    [InlineData("_.Store(new[] { The(x) })", "_.Store([the X])")]
    // A typed one states its element type
    [InlineData("new int[] { 1, 2 }", "int[1, 2]")]
    [InlineData("new MyModel[] { theFirst, theSecond }", "MyModel[theFirst, theSecond]")]
    [InlineData("A<MyModel?>", "a MyModel?")]
    [InlineData("The<TimeSpan>() / 2", "the TimeSpan / 2")]
    [InlineData("The<int>() + TheSecond<int>()", "the int + the second int")]
    [InlineData("The<int>() + TheSecond<int>() - TheThird<int>()", "the int + the second int - the third int")]
    [InlineData("The<int>() & TheSecond<int>() & TheThird<int>()", "the int & the second int & the third int")]
    [InlineData("_ => _.Name = A<string>() + ASecond<string>()", "Name = a string + a second string")]
    [InlineData(
        """
        _ => _.Name = A<string>()
                + ASecond<string>()
        """, "Name = a string + a second string")]
    [InlineData("() => The(delay)", "the Delay")]
    // A drilldown after a tag reads possessively, as one after a mention does
    [InlineData("The(_updatedRoom).RoomNumber", "the UpdatedRoom's RoomNumber")]
    [InlineData("obj?.Name", "obj.Name")]
    [InlineData("obj?.Method()", "obj.Method()")]
    [InlineData("_ => _.Inner?.Value", "_.Inner.Value")]
    [InlineData("await Foo()", "Foo()")]
    [InlineData("(await Foo()).Bar", "Foo().Bar")]
    [InlineData("_ => await _.GetAsync()", "_.GetAsync()")]
    [InlineData("async _ => await _.GetAsync()", "_.GetAsync()")]
    [InlineData("async (_) => await _.GetAsync()", "_.GetAsync()")]
    [InlineData("async Task<int> (_) => await _.GetAsync()", "_.GetAsync()")]
    // A return type is only recognised after async, so this one stays unparsed
    [InlineData("Task<int> (_) => _.GetAsync()", "Task<int> (_) => _.GetAsync()")]
    // A 2+ parameter lambda has no prose rendering, so it prints as source —
    // rebuilt from the tree, which is what keeps the keywords out of it
    [InlineData("async ValueTask<int> (a, b) => await Add(a, b)", "(a, b) => Add(a, b)")]
    [InlineData("(a,b) => a.Combine( b )", "(a, b) => a.Combine(b)")]
    [InlineData("(a, b, c) => a ? b[0] : (int)c", "(a, b, c) => a ? b[0] : (int)c")]
    [InlineData("() => (ICollection<int>)[1, 2]", "(ICollection<int>)[1, 2]")]
    [InlineData("() => (IReadOnlyList<int>)[The(x)]", "(IReadOnlyList<int>)[the X]")]
    [InlineData("() => (ICollection<int>)Two<int>()", "(ICollection<int>)two ints")]
    // Read as that cast, and prints back either way
    [InlineData("(x)[0]", "(x)[0]")]
    [InlineData("await.Length", "await.Length")]
    [InlineData("await - 1", "await - 1")]
    // Interpolation holes are expressions, and describe like any other
    [InlineData("$\"/rooms/{The(_roomNumber)}\"", "\"/rooms/{the RoomNumber}\"")]
    [InlineData("$\"{A<MyModel>()} and {An<int>()}\"", "\"{a MyModel} and {an int}\"")]
    [InlineData("$\"no holes here\"", "\"no holes here\"")]
    [InlineData("$\"{{escaped}} {The(x)}\"", "\"{{escaped}} {the X}\"")]
    [InlineData("$\"{The(x),10:N2}\"", "\"{the X,10:N2}\"")]
    // A comma inside the expression is not the alignment separator
    [InlineData("$\"{_.Foo(a, b)}\"", "\"{_.Foo(a, b)}\"")]
    [InlineData("$@\"{The(x)}\"", "\"{the X}\"")]
    // A raw string is a string: the quote run delimits it, and the dollar run says how many braces
    // open a hole — one fewer than that stays literal
    [InlineData("\"\"\"plain\"\"\"", "\"plain\"")]
    [InlineData("$\"\"\"{The(x)}\"\"\"", "\"{the X}\"")]
    [InlineData("$$\"\"\"{{The(x)}}\"\"\"", "\"{the X}\"")]
    [InlineData("$$\"\"\"a {b} {{The(x)}}\"\"\"", "\"a {b} {the X}\"")]
    // With no dollar there are no holes: a raw string is literal throughout, braces and all
    [InlineData("\"\"\"{ \"a\": 1 }\"\"\"", "\"{ \"a\": 1 }\"")]
    // How the author delimited it is mechanism; the same text is the same claim
    [InlineData("\"\"\"He said \"hi\" to me\"\"\"", "\"He said \"hi\" to me\"")]
    [InlineData("$\"\"\"say \"{The(x)}\" now\"\"\"", "\"say \"{the X}\" now\"")]
    [InlineData("$$\"\"\"{ \"a\": {{The(x)}} }\"\"\"", "\"{ \"a\": {the X} }\"")]
    [InlineData("\"\"\"\"x\"\"\"\"", "\"x\"")]
    [InlineData("_.Foo($\"\"\"{The(x)}\"\"\", \"\"\"b\"\"\")", "_.Foo(\"{the X}\", \"b\")")]
    // Break-point markers are pinned separately in WhenPlaceBreakPoints; here the wording is.
    public void ThenReturnDescription(string? valueExpr, string expected)
    {
        When(_ => valueExpr.Describe().StripWrapMarkers())
            .Then().Result.Is(expected);
    }
}