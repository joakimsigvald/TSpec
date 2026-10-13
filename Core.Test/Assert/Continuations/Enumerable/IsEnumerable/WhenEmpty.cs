using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Enumerable.IsEnumerable;

public class WhenEmpty : Spec
{
    [Fact]
    public void GivenEmpty_ThenCompletes() => Zero<int>().Is().Empty().and.Empty();

    [Fact]
    public void GivenNotEmpty_ThenGetException()
    {
        int[] arr = [1];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Is().Empty());
        ex.HasMessage($"Expected arr to be empty but found 1: [1]", "Arr is empty");
    }

    [Fact]
    public void GivenMoreThanFiveItems_ThenTheFailureSaysHowMany()
    {
        int[] arr = [1, 2, 3, 4, 5, 6, 7];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Is().Empty());
        ex.HasMessage("Expected arr to be empty but found 7: [1, 2, 3, 4, 5, ...]", "Arr is empty");
    }
}