using TSpec.Assert;

namespace TSpec.Test.Assert.AssertionExtensionsNumerical;

public class WhenIsInt : Spec<int>
{
    [Fact] public void GivenSame_ThenDoesNotThrow() => When(_ => _.Is(_)).Then();

    [Fact] public void GivenFail_ThenGetException() 
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => Using(1).When(_ => _).Then().Result.Is(2));
        ex.HasMessage("Expected Result to be 2 but found 1");
    }
}
