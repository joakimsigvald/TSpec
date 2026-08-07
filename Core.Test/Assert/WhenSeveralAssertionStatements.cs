using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Assert;

public class WhenSeveralAssertionStatements : Spec<MyService, MyModel>
{
    public WhenSeveralAssertionStatements()
        => When(_ => _.GetModel()).Given<IMyRepository>().Returns(A<MyModel>);

    [Fact]
    public void GivenTwoVerifications_ThenEachClaimTakesItsOwnLine()
    {
        Then<IMyRepository>(_ => _.GetModel());
        Then<IMyRepository>(_ => _.GetModel());
        Specification.Is(
            """
            Given IMyRepository returns a MyModel
            When GetModel()
            Then IMyRepository.GetModel()
              and IMyRepository.GetModel()
            """);
    }

    [Fact]
    public void GivenAssertionAfterVerification_ThenEachClaimTakesItsOwnLine()
    {
        Then<IMyRepository>(_ => _.GetModel());
        Then().Result.Is(The<MyModel>());
        Specification.Is(
            """
            Given IMyRepository returns a MyModel
            When GetModel()
            Then IMyRepository.GetModel()
              and Result is the MyModel
            """);
    }

    [Fact]
    public void GivenAssertionAfterThrowsCheck_ThenEachClaimTakesItsOwnLine()
    {
        Then().DoesNotThrow();
        Then().Result.Is(The<MyModel>());
        Specification.Is(
            """
            Given IMyRepository returns a MyModel
            When GetModel()
            Then does not throw
              and Result is the MyModel
            """);
    }
}

public class WhenVerifyingAfterThrows : Spec<MyService, MyModel>
{
    [Fact]
    public void ThenEachClaimTakesItsOwnLine()
    {
        When(_ => _.GetModel())
            .Given<IMyRepository>().Throws<NotFound>()
            .Then().Throws<NotFound>();
        Then<IMyRepository>(_ => _.GetModel());
        Specification.Is(
            """
            Given IMyRepository throws NotFound
            When GetModel()
            Then throws NotFound
              and IMyRepository.GetModel()
            """);
    }
}
