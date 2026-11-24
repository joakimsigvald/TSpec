using TSpec.Internal.Specification;
using static TSpec.Internal.Specification.ExpressionDescriber;

namespace TSpec.Test.Internal.Specification.ExpressionDescriber;

/// <summary>
/// Break points are placed while the structure is still known: « opens a group, » closes it, ¦ marks
/// a break point of the group it stands in, ¶ one that lines up with the line the group opened on —
/// stand-ins for the unprintable Wrap markers. A list of several items moves whole off its opening
/// bracket before it breaks one item per line; a lone item stays on its bracket. A brace block moves
/// whole before it breaks, with its braces on lines of their own.
/// </summary>
public class WhenPlaceBreakPoints
{
    [Theory]
    [InlineData("Foo()", "Foo()")]
    [InlineData("obj.Foo(An<int>())", "obj.Foo(«an int»)")]
    [InlineData("new MyComponent(An<IMyLogger>(), An<int>())",
        "new MyComponent(«¦«an IMyLogger, ¶an int»»)")]
    [InlineData("The<MyModel>() with { Name = A<string>() }",
        "the MyModel with« ¦{«¦ Name = a string ¶»}»")]
    [InlineData("new MyModel { Name = A<string>(), Id = An<int>() }",
        "new MyModel« ¦{«¦ Name = a string, ¦Id = an int ¶»}»")]
    [InlineData("(The<int>(), TheSecond<int>())", "(«¦«the int, ¶the second int»»)")]
    [InlineData("new[] { An<int>(), ASecond<int>() }", "[«¦«an int, ¶a second int»»]")]
    // The dot connecting two calls is a chain's joint; the dot of a path is not.
    [InlineData("\"abc\".ToUpper().ToLower()", "\"abc\".ToUpper()¦.ToLower()")]
    [InlineData("obj.Foo(An<int>()).Bar()", "obj.Foo(«an int»)¦.Bar()")]
    [InlineData("(await obj.GetAsync()).Bar", "obj.GetAsync()¦.Bar")]
    public void ThenMarkEachConstructAsAGroup(string expr, string expected)
        => Xunit.Assert.Equal(expected, MakeMarkersVisible(expr.Describe()));

    private static string MakeMarkersVisible(string text)
        => text
            .Replace(Wrap.Enter, '«')
            .Replace(Wrap.Exit, '»')
            .Replace(Wrap.Point, '¦')
            .Replace(Wrap.Align, '¶');
}
