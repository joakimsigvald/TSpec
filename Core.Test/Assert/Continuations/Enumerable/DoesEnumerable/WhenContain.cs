using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Enumerable.DoesEnumerable;

public class WhenContain : EnumerableSpec
{
    [Theory]
    [InlineData("abc", 'a')]
    public void GivenListContainItem_ThenDoesNotThrow(string actual, char expected)
        => actual.ToList().Does().Contain(expected).and.Is().not.Null();

    [Theory]
    [InlineData("", 'a')]
    [InlineData("abc", 'd')]
    [InlineData("abc", 'A')]
    public void GivenNotContainList_ThenGetException(string actual, char expected)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => actual.ToList().Does().Contain(expected));
        ex.HasMessage($"Expected actual.ToList() to contain {expected} but found {Describe(actual)}",
             "Actual.ToList() contains expected");
    }
}