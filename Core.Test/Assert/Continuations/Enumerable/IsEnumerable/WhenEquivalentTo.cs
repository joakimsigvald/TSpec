using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Enumerable.IsEnumerable;

public class WhenEquivalentTo : Spec
{
    [Fact]
    public void GivenAnExtraItem_ThenGetException()
    {
        int[] arr = [1, 2, 3];
        int[] secondArr = [2, 1];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Is().EquivalentTo(secondArr));
        ex.HasMessage("Expected arr to be equivalent to [2, 1] but found 3: [1, 2, 3]", "Arr is equivalent to secondArr");
    }
}
