using TSpec.Assert;
using TSpec.Internal.Specification;

namespace TSpec.Test.Internal.Specification.ExpressionParser;

public class WhenParseActual : Spec<string>
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("Something().That", "")]
    [InlineData("Then().Result.Name", "Result.Name")]
    [InlineData("And(Result).Id", "Id")]
    [InlineData("The<int>()", "the int")]
    [InlineData("Then().Result?.Name", "Result?.Name")]
    [InlineData("Then().and.Result", "Result")]
    [InlineData("Then<IMyService>(_ => _.Call()).and.Result", "Result")]
    [InlineData("And<IMyService>(_ => _.Call()).and.Result", "Result")]
    public void ThenReturnDescription(string? returnsExpr, string expected)
        => When(_ => returnsExpr.ParseActual()).Then().Result.Is(expected);

    [Theory]
    [InlineData("And(Result).Id", "Result", "Result.Id")]
    [InlineData("Then().IsOpen", "the Checkout", "the Checkout's IsOpen")]
    public void GivenSubject_ThenPrefixDescription(string returnsExpr, string subject, string expected)
        => When(_ => returnsExpr.ParseActual(subject)).Then().Result.Is(expected);

}