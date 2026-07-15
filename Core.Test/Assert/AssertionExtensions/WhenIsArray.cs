using TSpec.Assert;

namespace TSpec.Test.Assert.AssertionExtensions;

public class WhenIsArray : Spec<int[]>
{
    public WhenIsArray() => Using(() => new int[] { 1, 2, 3 }, For.Subject);

    [Fact] public void GivenSame_ThenDoesNotThrow() => When(_ => _.Is(_)).Then();

    [Fact]
    public void GivenFail_ThenGetException()
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(()
            => When(_ => _).Then().Result.Is([1, 2, 3]));
        ex.HasMessage(
            "Expected Result to be [1, 2, 3] but found [1, 2, 3]",
            """
            Using new int[] { 1, 2, 3 } for Subject
            When _
            Then Result is [1, 2, 3]
            """);
    }
}