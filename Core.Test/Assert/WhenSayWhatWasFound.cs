using TSpec.Assert;

namespace TSpec.Test.Assert;

public class WhenSayWhatWasFound : Spec
{
    [Fact]
    public void GivenTheActualsOwnValue_ThenTheFailureSaysWhatItWas()
    {
        int n = 1;
        Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => n.Is(2)).HasMessage("Expected n to be 2 but it was 1");
    }

    [Fact]
    public void GivenAnActualWrittenAsItsValue_ThenTheFailureDoesNotRepeatIt()
        => Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => 3.Is().LessThan(2)).HasMessage("Expected 3 to be less than 2");

    [Fact]
    public void GivenWhatTheAssertionFoundInTheActual_ThenTheFailureSaysWhatItFound()
    {
        int[] arr = [1, 3];
        Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Has().None(it => it > 2))
            .HasMessage("Expected arr to have none it > 2 but found 1 that does: [3]");
    }

    [Fact]
    public void GivenACountedCollection_ThenTheFailureSaysWhatItFound()
    {
        int[] arr = [1, 3];
        Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Is().Empty())
            .HasMessage("Expected arr to be empty but found 2: [1, 3]");
    }
}
