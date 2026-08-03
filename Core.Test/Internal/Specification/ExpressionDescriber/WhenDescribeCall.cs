using TSpec.Assert;
using TSpec.Internal.Specification;

namespace TSpec.Test.Internal.Specification.ExpressionDescriber;

public class WhenDescribeCall : Spec<string>
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("_ => _.Get(The<int>())", "_.Get(the int)")]
    [InlineData("Get(The<int>())", "Get(the int)")]
    [InlineData("_ => MyService.Echo(The<MyEnum>())", "MyService.Echo(the MyEnum)")]
    [InlineData("_ => [1, 2, 3]", "[1, 2, 3]")]
    [InlineData("_ => A<List<int>>()", "a List<int>")]
    [InlineData("_ => new MyModel { Id = An<int>() }", "new MyModel { Id = an int }")]
    [InlineData("""
        x

        y
        """, "x y")]
    // Break-point markers are pinned separately in WhenPlaceBreakPoints; here the wording is.
    public void ThenReturnDescription(string? callExpr, string? expected)
        => When(_ => callExpr.DescribeCall()?.StripWrapMarkers())
        .Then().Result.Is(expected);
}