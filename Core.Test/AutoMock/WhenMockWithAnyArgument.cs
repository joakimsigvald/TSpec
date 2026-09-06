using Moq;
using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.AutoMock;

public class WhenMockWithAnyArgument : Spec<MyValueIntService, string>
{
    [Fact]
    public void ThenSetupMatchesAnyArgument()
    {
        Given<IMyValueIntRepo>().That(_ => _.Get(Any<int>())).Returns(A<string>)
            .When(_ => _.GetValue(A<MyValueInt>()))
            .Then().Result.Is(The<string>());
        Specification.Is(
            """
            Given IMyValueIntRepo.Get(any int) returns a string
            When GetValue(a MyValueInt)
            Then Result is the string
            """);
    }

    [Fact]
    public void ThenSetupMatchesAnyArguments()
    {
        Given<IMyValueIntRepo>().That(_ => _.Get2(Any<int>(), Any<int>())).Returns(A<string>)
            .When(_ => _.GetValue2(A<MyValueInt>(), Another<MyValueInt>()))
            .Then().Result.Is(The<string>());
        Specification.Is(
            """
            Given IMyValueIntRepo.Get2(any int, any int) returns a string
            When GetValue2(a MyValueInt, another MyValueInt)
            Then Result is the string
            """);
    }

    [Fact]
    public void ThenVerifyMatchesAnyArgument()
    {
        When(_ => _.SetValue(A<MyValueInt>()))
            .Then<IMyValueIntRepo>(_ => _.Set(Any<int>()));
        Specification.Is(
            """
            When SetValue(a MyValueInt)
            Then IMyValueIntRepo.Set(any int)
            """);
    }

    [Fact]
    public void ThenItIsAnyRendersAsAny()
    {
        Given<IMyValueIntRepo>().That(_ => _.Get(It.IsAny<int>())).Returns(A<string>)
            .When(_ => _.GetValue(A<MyValueInt>()))
            .Then<IMyValueIntRepo>(_ => _.Get(It.IsAny<int>()))
            .And(Result).Is(The<string>());
        Specification.Is(
            """
            Given IMyValueIntRepo.Get(any int) returns a string
            When GetValue(a MyValueInt)
            Then IMyValueIntRepo.Get(any int)
              and Result is the string
            """);
    }
}
