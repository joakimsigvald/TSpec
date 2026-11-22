using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.IsNullableStruct;

public class WhenNotNull : Spec
{
    [Fact] public void GivenNotNull_ThenCompletes() => ((Money?)new Money()).Is().not.Null().and.not.Null();

    [Fact]
    public void GivenNull_ThenGetException()
    {
        Money? actual = null;
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => actual.Is().not.Null());
        ex.HasMessage("Expected actual to not be null but it was null", "Actual is not null");
    }
}