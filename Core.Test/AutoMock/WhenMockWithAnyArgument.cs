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

    [Fact]
    public void GivenAnyConvertedToTheParameterType_ThenThrowSetupFailed()
        => Xunit.Assert.Throws<SetupFailed>(() =>
            Given<IMyValueIntRepo>().That(_ => _.Get(Any<MyValueInt>())).Returns(A<string>)
                .When(_ => _.GetValue(A<MyValueInt>()))
                .Then().Result.Is(The<string>()))
            .Message.Is(
                "Any<MyValueInt>() is converted to int, so it can never match: "
                + "the call receives an int, not a MyValueInt. Write Any<int>() instead");

    [Fact]
    public void GivenConstrainedAnyConvertedToTheParameterTypeInVerification_ThenThrowSetupFailed()
        => Xunit.Assert.Throws<SetupFailed>(() =>
            When(_ => _.SetValue(A<MyValueInt>()))
                .Then<IMyValueIntRepo>(_ => _.Set(Any<MyValueInt>(v => v.Primitive > 0))))
            .Message.Is(
                "Any<MyValueInt>(...) is converted to int, so it can never match: "
                + "the call receives an int, not a MyValueInt. Write Any<int>(...) instead");

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

/// Any matches a whole argument; inside one it would be evaluated as a single generated value.
public class WhenAnyIsNestedInsideAnArgument : Spec<MemberKindsService, string>
{
    [Fact]
    public void GivenItIsAnElementOfAnArrayInASetup_ThenThrowSetupFailed()
        => Xunit.Assert.Throws<SetupFailed>(() =>
            When(_ => _.SumOf(1, 2))
                .Given<IMemberKinds>().That(_ => _.Sum(new[] { Any<int>(), 2 })).Returns(() => 3)
                .Then().Result.Is("3"))
            .Message.Is(
                "Any<int>() matches a whole argument, not a part of one, "
                + "so inside the values argument of IMemberKinds.Sum it can match nothing. "
                + "Write Any<int[]>() for any int[], or Any<int[]>(values => ...) for any int[] satisfying a condition");

    [Fact]
    public void GivenAConstrainedAnyIsAnElementOfAnArrayInASetup_ThenThrowSetupFailed()
        => Xunit.Assert.Throws<SetupFailed>(() =>
            When(_ => _.SumOf(1, 2))
                .Given<IMemberKinds>().That(_ => _.Sum(new[] { Any<int>(i => i > 0), 2 })).Returns(() => 3)
                .Then().Result.Is("3"))
            .Message.Is(
                "Any<int>(...) matches a whole argument, not a part of one, "
                + "so inside the values argument of IMemberKinds.Sum it can match nothing. "
                + "Write Any<int[]>() for any int[], or Any<int[]>(values => ...) for any int[] satisfying a condition");
}

public class WhenAnyIsNestedInsideAVerifiedArgument : Spec<Subjects.ShoppingService, object>
{
    [Fact]
    public void GivenItInitializesAMember_ThenThrowSetupFailed()
        => Xunit.Assert.Throws<SetupFailed>(() =>
            When(_ => _.PlaceOrder(A<Subjects.ShoppingCart>()))
                .Then<Subjects.IOrderService>(_ => _.CreateOrder(new Subjects.ShoppingCart { Id = Any<int>() })))
            .Message.Is(
                "Any<int>() matches a whole argument, not a part of one, "
                + "so inside the cart argument of IOrderService.CreateOrder it can match nothing. "
                + "Write Any<ShoppingCart>() for any ShoppingCart, "
                + "or Any<ShoppingCart>(cart => ...) for any ShoppingCart satisfying a condition");
}
