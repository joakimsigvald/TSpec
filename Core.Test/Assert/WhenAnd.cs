using TSpec.Assert;
using Xunit.Sdk;

namespace TSpec.Test.Assert;

public class WhenAnd
{
    [Fact]
    public void GivenTheFirstPasses_ThenTheSecondIsAsserted()
    {
        var ex = Xunit.Assert.Throws<XunitException>(() => 3.Is().GreaterThan(1).and.LessThan(2));
        ex.HasMessage("Expected 3 to be less than 2 but found 3");
    }

    [Fact]
    public void GivenTheFirstIsNegated_ThenTheSecondIsNot()
    {
        var ex = Xunit.Assert.Throws<XunitException>(() => 3.Is().not.GreaterThan(5).and.LessThan(2));
        ex.HasMessage("Expected 3 to be less than 2 but found 3");
    }
}
