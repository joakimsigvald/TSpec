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
        ex.HasMessage("""
            Expected arr to be equivalent to secondArr
              extra: 2: 3
            """, "Arr is equivalent to secondArr");
    }

    [Fact]
    public void GivenNotAndTheSameItems_ThenGetException()
    {
        int[] arr = [1, 2];
        int[] secondArr = [2, 1];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Is().not.EquivalentTo(secondArr));
        ex.HasMessage("Expected arr to not be equivalent to secondArr but found 2: [1, 2]");
    }

    [Fact]
    public void GivenTwoSequencesThatAreNotCollections_ThenSetupFails()
    {
        var arr = new[] { 1, 2 }.Where(i => i > 0);
        var secondArr = new[] { 2, 1 }.Where(i => i > 0);
        var ex = Xunit.Assert.Throws<SetupFailed>(() => arr.Is().Like(secondArr));
        ex.Message.Is("Neither arr nor secondArr is a collection, so either may never end: make one of them a list or an array");
    }
}
