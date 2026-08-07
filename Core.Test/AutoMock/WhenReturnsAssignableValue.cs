using TSpec.Assert;

namespace TSpec.Test.AutoMock;

public class WhenReturnsAssignableValue : Spec<MyValueIntService, ICollection<int>>
{
    [Fact]
    public void GivenValueOfAssignableType_ThenMockReturnsIt()
    {
        When(_ => _.GetNumbers())
            .Given<IMyValueIntRepo>().Returns(Two<int>)
            .Then().Result.Is(Two<int>());
        Specification.Is(
            """
            Given IMyValueIntRepo returns two ints
            When GetNumbers()
            Then Result is two ints
            """);
    }

    [Fact]
    public void GivenNullOfAssignableType_ThenMockReturnsNull()
    {
        When(_ => _.GetNumbers())
            .Given<IMyValueIntRepo>().Returns(() => (int[]?)null)
            .Then().Result.Is().Null();
        Specification.Is(
            """
            Given IMyValueIntRepo returns (int[]?)null
            When GetNumbers()
            Then Result is null
            """);
    }

    [Fact]
    public void GivenTwoAssignableValuesAndNeitherIsMoreSpecific_ThenSetupFails()
        => Xunit.Assert.Throws<SetupFailed>(() =>
            When(_ => _.GetNumbers())
                .Given<IMyValueIntRepo>().Returns(Two<int>)
                .And<IMyValueIntRepo>().Returns(A<List<int>>)
                .Then().DoesNotThrow());
}

public class WhenReturnsMostSpecificValue : Spec<MyValueIntService, IEnumerable<int>>
{
    [Fact]
    public void GivenTwoAssignableValues_ThenMockReturnsTheMoreSpecific()
    {
        When(_ => _.GetAnyNumbers())
            .Given<IMyValueIntRepo>().Returns(() => (ICollection<int>)A<List<int>>())
            .And<IMyValueIntRepo>().Returns(Two<int>)
            .Then().Result.Is(Two<int>());
        Specification.Is(
            """
            Given IMyValueIntRepo returns (ICollection<int>)a List<int>
              and returns two ints
            When GetAnyNumbers()
            Then Result is two ints
            """);
    }
}
