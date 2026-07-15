using TSpec.Assert;
using Xunit.Sdk;

namespace TSpec.Test.Assert.Continuations.Enumerable.HasEnumerable;

public class WhenOrder : Spec
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(4, 2, 1, -1)]
    public void GivenDescending(params int[] numbers)
    {
        numbers.Has().Order<int>().Descending();
        Specification.Is($"Numbers has descending order");
    }

    [Theory]
    [InlineData("Expected numbers to be descending but found [1, 3]", 1, 3)]
    public void GivenDescendingFail(string errorMessage, params int[] numbers)
    {
        var ex = Xunit.Assert.Throws<XunitException>(() => numbers.Has().Order<int>().Descending());
        ex.Message.Is(errorMessage);
    }

    [Theory]
    [InlineData(1, 2)]
    public void GivenAsceding(params int[] numbers)
    {
        numbers.Has().Order<int>().Ascending();
        Specification.Is($"Numbers has ascending order");
    }

    [Theory]
    [InlineData("Expected numbers to be ascending but found [2, 1]", 2, 1)]
    public void GivenAscedingFail(string errorMessage, params int[] numbers)
    {
        var ex = Xunit.Assert.Throws<XunitException>(() => numbers.Has().Order<int>().Ascending());
        ex.Message.Is(errorMessage);
    }

    [Fact]
    public void GivenDescendingBy()
    {
        int[] numbers = [2, 1, 3];
        numbers.Has().Order<int>(it => it % 3).Descending();
        Specification.Is($"Numbers has descending order by it => it % 3");
    }

    [Fact]
    public void GivenDescendingByFail()
    {
        int[] numbers = [3, 1, 2];
        var ex = Xunit.Assert.Throws<XunitException>(() => numbers.Has().Order<int>(it => it % 3).Descending());
        ex.Message.Is("Expected numbers to be descending by it => it % 3 but found [3, 1, 2]");
    }

    [Fact]
    public void GivenAscendingBy()
    {
        int[] numbers = [3, 1, 2];
        numbers.Has().Order<int>(it => it % 3).Ascending();
        Specification.Is($"Numbers has ascending order by it => it % 3");
    }

    [Fact]
    public void GivenAscendingByFail()
    {
        int[] numbers = [2, 1, 3];
        var ex = Xunit.Assert.Throws<XunitException>(() => numbers.Has().Order<int>(it => it % 3).Ascending());
        ex.Message.Is("Expected numbers to be ascending by it => it % 3 but found [2, 1, 3]");
    }
}