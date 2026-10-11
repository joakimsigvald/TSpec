using TSpec.Assert;
using Xunit.Sdk;

namespace TSpec.Test.Assert.EitherOr;

public class WhenEitherOnCollection
{
    private readonly int[] _one = [1];

    [Fact]
    public void GivenIs_ThenTheSecondCanHold() => _one.Is().either.Empty().or.EqualTo([1]);

    [Fact]
    public void GivenHas_ThenTheSecondCanHold() => _one.Has().either.Count(2).or.OneItem();

    [Fact]
    public void GivenDoes_ThenTheSecondCanHold() => _one.Does().either.Contain(2).or.Contain(1);

    [Fact]
    public void GivenACount_ThenTheSecondCanHold() => _one.Has().either.Count().AtLeast(2).or.Count().AtMost(1);

    [Fact]
    public void GivenNoCountHolds_ThenEachIsNamed()
    {
        var ex = Xunit.Assert.Throws<XunitException>(
            () => _one.Has().either.Count().AtLeast(2)._.Count().AtLeast(3).or.Count().AtLeast(4));
        ex.HasMessage(
            "Expected _one to have at least 2 items, have at least 3 items or have at least 4 items but found 1: [1]");
    }

    [Fact]
    public void GivenNoneHolds_ThenEachIsNamed()
    {
        var ex = Xunit.Assert.Throws<XunitException>(() => _one.Is().either.Empty().or.EqualTo([2]));
        ex.HasMessage("Expected _one to be empty or be equal to [2] but found [1]");
    }
}
