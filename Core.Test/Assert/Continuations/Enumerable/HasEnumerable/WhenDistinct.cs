using TSpec.Assert;
using TSpec.Internal.Specification;
using Xunit.Sdk;

namespace TSpec.Test.Assert.Continuations.Enumerable.HasEnumerable;

public class WhenDistinct : Spec
{
    [Theory]
    [InlineData(1, 2)]
    public void GivenDistinct(params int[] numbers)
    {
        numbers.Is().Distinct();
        Specification.Is("Numbers is distinct");
    }

    [Theory]
    [InlineData(1, 1)]
    public void GivenNotDistinct(params int[] numbers)
    {
        numbers.Is().not.Distinct();
        Specification.Is("Numbers is not distinct");
    }

    [Theory]
    [InlineData(1, 1)]
    public void GivenDistinctFail(params int[] numbers)
    {
        var ex = Xunit.Assert.Throws<XunitException>(() => numbers.Is().Distinct());
        ex.Message.Is($"Expected numbers to be distinct but found {numbers.ParseValue()}");
    }

    [Theory]
    [InlineData(1, 2)]
    public void GivenNotDistinctFail(params int[] numbers)
    {
        var ex = Xunit.Assert.Throws<XunitException>(() => numbers.Is().not.Distinct());
        ex.Message.Is($"Expected numbers to not be distinct but found {numbers.ParseValue()}");
    }

    [Theory]
    [InlineData(1, 5, 3)]
    public void GivenDistinctProperty(params int[] numbers)
    {
        numbers.Is().Distinct(it => it % 3);
        Specification.Is("Numbers is distinct by it => it % 3");
    }

    [Theory]
    [InlineData(1, 4)]
    public void GivenDistinctPropertyFail(params int[] numbers)
    {
        var ex = Xunit.Assert.Throws<XunitException>(() => numbers.Is().Distinct(it => it % 3));
        ex.Message.Is($"Expected numbers to be distinct by it => it % 3 but found {numbers.ParseValue()}");
    }

    [Theory]
    [InlineData(1, 1, 1)]
    public void GivenDistinctPropertyIndex(params int[] numbers)
    {
        numbers.Is().Distinct((it, i) => it + i);
        Specification.Is("Numbers is distinct by (it, i) => it + i");
    }

    [Theory]
    [InlineData(3, 2, 1)]
    public void GivenDistinctPropertyIndexFail(params int[] numbers)
    {
        var ex = Xunit.Assert.Throws<XunitException>(() => numbers.Is().Distinct((it, i) => it + i));
        ex.Message.Is($"Expected numbers to be distinct by (it, i) => it + i but found {numbers.ParseValue()}");
    }

    [Theory]
    [InlineData(1, 5, 3)]
    public void GivenDistinctPropertyAndProperty(params int[] numbers)
    {
        numbers.Is().Distinct(it => it % 3).and.Distinct(it => it % 5);
        Specification.Is(
            """
            Numbers is distinct by it => it % 3
                and distinct by it => it % 5
            """);
            
    }
}