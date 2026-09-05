using TSpec.Assert;
using TSpec.Internal.Specification;

namespace TSpec.Test.Internal.Specification.ExpressionDescriber;

public class WhenDescribeActual : Spec<string>
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("Something().That", "")]
    [InlineData("Then().Result.Name", "Result.Name")]
    [InlineData("And(Result).Id", "Id")]
    [InlineData("The<int>()", "the int")]
    [InlineData("Then().Result?.Name", "Result.Name")]
    [InlineData("Then().Result[0]", "Result[0]")]
    [InlineData("Because(\"it is so\").Result.Rows[0]", "Result.Rows[0]")]
    [InlineData("Then().Result.Rows[0][1]", "Result.Rows[0][1]")]
    [InlineData("Then().Result.Rows[0].Cells[1]", "Result.Rows[0].Cells[1]")]
    [InlineData("Then().Result[0].Name", "Result[0].Name")]
    [InlineData("Then().Result.Read<Room>()", "Result.Read<Room>()")]
    [InlineData("Because(\"it is so\").Result.Read<Room>().Name", "Result.Read<Room>().Name")]
    [InlineData("Then().Result.Read<Room>()[0]", "Result.Read<Room>()[0]")]
    [InlineData("Then().and.Result", "Result")]
    [InlineData("Then<IMyService>(_ => _.Call()).and.Result", "Result")]
    [InlineData("And<IMyService>(_ => _.Call()).and.Result", "Result")]
    [InlineData("await Result.Read<Room>()", "Result.Read<Room>()")]
    // The await sits mid-chain here, so peeling has to happen as the chain is walked
    [InlineData("(await Result.Read<VersionInfo>()).Version", "Result.Read<VersionInfo>().Version")]
    // A parenthesized root keeps both operands, and the parentheses that make it one
    [InlineData("(Result.Text ?? \"\").Contains(\"x\")", "(Result.Text ?? \"\").Contains(\"x\")")]
    public void ThenReturnDescription(string? returnsExpr, string expected)
        => When(_ => returnsExpr.DescribeActual()).Then().Result.Is(expected);

    [Theory]
    [InlineData("And(Result).Id", "Result", "Result.Id")]
    [InlineData("Then().IsOpen", "the Checkout", "the Checkout's IsOpen")]
    public void GivenSubject_ThenPrefixDescription(string returnsExpr, string subject, string expected)
        => When(_ => returnsExpr.DescribeActual(subject)).Then().Result.Is(expected);

}