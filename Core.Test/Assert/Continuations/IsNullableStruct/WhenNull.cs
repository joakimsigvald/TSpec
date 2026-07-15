using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.IsNullableStruct;

public class WhenNull : Spec
{
    [Fact] public void GivenNull_ThenDoesNotThrow() => ((Money?)null).Is().Null();

    [Fact]
    public void GivenNotNull_ThenGetException()
    {
        Money? actual = new();
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => actual.Is().Null());
        ex.HasMessage("Expected actual to be null but found Money { Amount = 0, Currency =  }", "Actual is null");
    }
}