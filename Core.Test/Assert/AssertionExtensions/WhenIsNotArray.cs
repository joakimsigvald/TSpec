using TSpec.Assert;

namespace TSpec.Test.Assert.AssertionExtensions;

public class WhenIsNotArray : Spec<int[]>
{
    public WhenIsNotArray() => Using(() => new int[] { 1, 2, 3 }, For.Input);

    [Fact] public void GivenNotSame_ThenDoesNotThrow() => When(_ => _.Is().Not(null)).Then();

    [Fact]
    public void GivenFail_ThenGetException()
    {
        int[] arr = [1, 2];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(()
            => When(_ => arr).Then().Result.Is().Not(arr));
        ex.HasMessage(
            "Expected Result to not be [1, 2] but found [1, 2]",
            """
            Using new int[] { 1, 2, 3 } for Input
            When arr
            Then Result is not arr
            """);
    }
}