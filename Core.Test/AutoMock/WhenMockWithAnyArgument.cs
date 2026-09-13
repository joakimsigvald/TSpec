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
            .When(_ => _.GetValue2(A<MyValueInt>(), ASecond<MyValueInt>()))
            .Then().Result.Is(The<string>());
        Specification.Is(
            """
            Given IMyValueIntRepo.Get2(any int, any int) returns a string
            When GetValue2(a MyValueInt, a second MyValueInt)
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
    public void ThenSetupMatchesArgumentSatisfyingConstraint()
    {
        Given<IMyValueIntRepo>().That(_ => _.Get(Any<int>(i => i == The<MyValueInt>().Primitive))).Returns(A<string>)
            .When(_ => _.GetValue(A<MyValueInt>()))
            .Then().Result.Is(The<string>());
        Specification.Is(
            """
            Given IMyValueIntRepo.Get(any int where i == the MyValueInt's Primitive)
                  returns a string
            When GetValue(a MyValueInt)
            Then Result is the string
            """);
    }

    [Fact]
    public void ThenVerifyMatchesArgumentSatisfyingConstraint()
    {
        When(_ => _.SetValue(A<MyValueInt>()))
            .Then<IMyValueIntRepo>(_ => _.Set(Any<int>(i => i == The<MyValueInt>().Primitive)))
            .And<IMyValueIntRepo>(_ => _.Set(Any<int>(i => i != The<MyValueInt>().Primitive)), Times.Never);
        Specification.Is(
            """
            When SetValue(a MyValueInt)
            Then IMyValueIntRepo.Set(any int where i == the MyValueInt's Primitive)
              and IMyValueIntRepo.Set(
                    any int where i != the MyValueInt's Primitive) was not invoked
            """);
    }

    [Fact]
    public void ThenSetupMatchesArgumentSatisfyingConstraintMethod()
    {
        Given<IMyValueIntRepo>().That(_ => _.Get(Any<int>(IsPositive))).Returns(A<string>)
            .When(_ => _.GetValue(A<MyValueInt>()))
            .Then().Result.Is(The<string>());
        Specification.Is(
            """
            Given IMyValueIntRepo.Get(any int where IsPositive) returns a string
            When GetValue(a MyValueInt)
            Then Result is the string
            """);
    }

    private static bool IsPositive(int value) => value > 0;

    [Fact]
    public void GivenConstraintOutsideMock_ThenThrowSetupFailed()
        => Xunit.Assert.Throws<SetupFailed>(() => Any<int>(i => i > 0)).Message.Is(
            "Any<int>(constraint) matches an argument in a mock setup or verification, and means nothing elsewhere. "
            + "To set up the value instead, write the lambda with braces: Any<int>(value => { ... })");

    /// Moq's matcher is not TSpec's: evaluated as a plain value it would match only the type's default.
    [Fact]
    public void GivenMoqsItIsAny_ThenThrowSetupFailed()
        => Xunit.Assert.Throws<SetupFailed>(() =>
            Given<IMyValueIntRepo>().That(_ => _.Get(It.IsAny<int>())).Returns(A<string>)
                .When(_ => _.GetValue(A<MyValueInt>()))
                .Then<IMyValueIntRepo>(_ => _.Get(It.IsAny<int>())))
            .Message.Is(
                "It.IsAny<int>() is Moq's, which TSpec does not use. Write Any<T>() for any value, "
                + "or Any<T>(constraint) for any value satisfying the constraint");
}
