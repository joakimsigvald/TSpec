using TSpec.Assert;
using TSpec.Internal.Specification;
using Xunit.Sdk;

namespace TSpec.Test.Assert.Continuations.Enumerable.HasEnumerable;

public class WhenCount : Spec
{
    [Fact]
    public void GivenCorrectCount_ThenDoesNotThrow() => Two<int>().Has().Count(2).and.Count(2);

    [Fact]
    public void GivenWrongCount_ThenGetException()
    {
        int[] arr = [1];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Has().Count(2));
        ex.HasMessage($"Expected arr to have count 2 but found 1: [1]", "Arr has count 2");
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 1, 3)]
    public void GivenCount(int count, params int[] numbers) => numbers.Has().Count(count);

    [Theory]
    [InlineData(2, "Expected numbers to have count 2 but found 1: [1]", 1)]
    [InlineData(1, "Expected numbers to have count 1 but found 2: [1, 3]", 1, 3)]
    [InlineData(3, "Expected numbers to have count 3 but found 6: [1, 2, 3, 4, ...]", 1, 2, 3, 4, 5, 6)]
    public void GivenCountFail(int count, string errorMessage, params int[] numbers)
    {
        var ex = Xunit.Assert.Throws<XunitException>(() => numbers.Has().Count(count));
        ex.Message.Is(errorMessage);
    }

    [Theory]
    [InlineData(2, 1)]
    [InlineData(1, 1, 3)]
    public void GivenNotCount(int count, params int[] numbers) => numbers.Has().not.Count(count);

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 1, 3)]
    public void GivenAtLeast(int count, params int[] numbers)
    {
        numbers.Has().Count().AtLeast(count);
        Specification.Is($"Numbers has at least 'count' = {count} items");
    }

    [Fact]
    public void GivenAtLeastFail()
    {
        var ex = Xunit.Assert.Throws<XunitException>(() => Two<int>().Has().Count().AtLeast(3));
        ex.Message.Is($"Expected two int to have at least 3 items but found {Two<int>().ParseValue()}");
    }

    [Theory]
    [InlineData(2, 1)]
    [InlineData(2, 1, 3)]
    public void GivenAtMost(int count, params int[] numbers)
    {
        numbers.Has().Count().AtMost(count);
        Specification.Is($"Numbers has at most 'count' = {count} items");
    }

    [Theory]
    [InlineData(1, 2, 1)]
    [InlineData(1, 2, 1, 3)]
    public void GivenInRange(int from, int to, params int[] numbers)
    {
        numbers.Has().Count().InRange(from, to);
        Specification.Is($"Numbers has between 'from' = {from} and 'to' = {to} items");
    }

    [Theory]
    [InlineData(11, 12, 1)]
    [InlineData(11, 12, 1, 3)]
    public void GivenNotInRange(int from, int to, params int[] numbers)
    {
        numbers.Has().not.Count().InRange(from, to);
        Specification.Is($"Numbers has not between 'from' = {from} and 'to' = {to} items");
    }

    [Fact]
    public void GivenInvalidRange()
    {
        var ex = Xunit.Assert.Throws<SetupFailed>(() => Two<int>().Has().Count().InRange(3, 2));
        ex.Message.Is("Given range must be in ascending order");
    }

    [Theory]
    [InlineData(0, 1, 1)]
    [InlineData(1, 2, 1, 3)]
    public void GivenConditionalCount(int count, int greaterThan, params int[] numbers)
    {
        numbers.Has().Count(it => it > greaterThan).At(count);
        Specification.Is($"Numbers has 'count' = {count} items where it => it > greaterThan");
    }

    [Theory]
    [InlineData(0, 1, 1)]
    [InlineData(1, 2, 1, 3)]
    public void GivenConditionalCountAtLeast(int count, int greaterThan, params int[] numbers)
    {
        numbers.Has().Count(it => it > greaterThan).AtLeast(count);
        Specification.Is($"Numbers has at least 'count' = {count} items where it => it > greaterThan");
    }

    [Theory]
    [InlineData(0, 1, 1)]
    [InlineData(1, 2, 1, 3)]
    public void GivenConditionalCountAtMost(int count, int greaterThan, params int[] numbers)
    {
        numbers.Has().Count(it => it > greaterThan).AtMost(count);
        Specification.Is($"Numbers has at most 'count' = {count} items where it => it > greaterThan");
    }

    [Theory]
    [InlineData(1, 2, 0, 1)]
    [InlineData(1, 2, 2, 1, 3)]
    public void GivenConditionalInRange(int from, int to, int greaterThan, params int[] numbers)
    {
        numbers.Has().Count(it => it > greaterThan).InRange(from, to);
        Specification.Is($"Numbers has between 'from' = {from} and 'to' = {to} items where it => it > greaterThan");
    }

    [Fact]
    public void GivenConditionalInRangeFail()
    {
        int[] oneAndTwo = [1, 2];
        var ex = Xunit.Assert.Throws<XunitException>(() => oneAndTwo.Has().Count(it => it < 0).InRange(1, 2));
        ex.Message.Is("Expected oneAndTwo to have between 1 and 2 items where it => it < 0 but found [1, 2]");
    }
}