using TSpec.Internal.Specification;
using static TSpec.Internal.Specification.ExpressionDescriber;

namespace TSpec.Test.Internal.Specification.ExpressionDescriber;

/// <summary>
/// Break points are placed while the structure is still known, ranked by nesting: « enters a
/// level, » exits it, ¦ marks a break point — stand-ins for the unprintable Wrap markers.
/// An argument list may break after its opening paren and after each comma; a brace block
/// prefers moving whole (the point before the brace) over breaking inside.
/// </summary>
public class WhenPlaceBreakPoints
{
    [Theory]
    [InlineData("Foo()", "Foo()")]
    [InlineData("obj.Foo(An<int>())", "obj.Foo(«¦an int»)")]
    [InlineData("new MyComponent(An<IMyLogger>(), An<int>())",
        "new MyComponent(«¦an IMyLogger, ¦an int»)")]
    [InlineData("The<MyModel>() with { Name = A<string>() }",
        "the MyModel with« ¦{ «Name = a string» }»")]
    [InlineData("new MyModel { Name = A<string>(), Id = An<int>() }",
        "new MyModel« ¦{ «Name = a string, ¦Id = an int» }»")]
    [InlineData("(The<int>(), TheSecond<int>())", "(«the int, ¦the second int»)")]
    // The dot connecting two calls is a chain's joint; the dot of a path is not.
    [InlineData("\"abc\".ToUpper().ToLower()", "\"abc\".ToUpper()¦.ToLower()")]
    [InlineData("obj.Foo(An<int>()).Bar()", "obj.Foo(«¦an int»)¦.Bar()")]
    [InlineData("(await obj.GetAsync()).Bar", "obj.GetAsync()¦.Bar")]
    public void ThenRankBreakPointsByNesting(string expr, string expected)
        => Xunit.Assert.Equal(expected, MakeMarkersVisible(expr.Describe()));

    private static string MakeMarkersVisible(string text)
        => text.Replace(Wrap.Enter, '«').Replace(Wrap.Exit, '»').Replace(Wrap.Point, '¦');
}
