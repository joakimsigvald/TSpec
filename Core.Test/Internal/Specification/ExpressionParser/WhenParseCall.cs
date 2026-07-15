using TSpec.Assert;
using TSpec.Internal.Specification;

namespace TSpec.Test.Internal.Specification.ExpressionParser;

public class WhenParseCall : Spec<string>
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
    public void ThenReturnDescription(string? callExpr, string? expected)
        => When(_ => callExpr.ParseCall())
        .Then().Result.Is(expected);
}