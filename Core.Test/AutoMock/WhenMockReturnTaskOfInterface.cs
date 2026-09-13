using TSpec.Assert;

namespace TSpec.Test.AutoMock;

public class WhenMockReturnTaskOfInterface : Spec<MyValueIntService, IMyValueIntRepo>
{
    [Fact]
    public void ThenThrowSetupFailed()
    {
        var error = Xunit.Assert.Throws<SetupFailed>(
            () => When(_ => _.GetRepoAsync()).Then().Result.Get(1).Is().not.Null());
        error.Message.Does().Contain("Given<IMyValueIntRepo>().Returns(A<IMyValueIntRepo>)");
    }
}
/// A generic type is named in the refusal as C# writes it, so the suggested setup can be copied.
public class WhenMockReturnTaskOfGenericInterface : Spec<TestData.MyService, TestData.MyModel[]>
{
    [Fact]
    public void ThenThrowSetupFailedNamingTheTypeAsWritten()
    {
        var error = Xunit.Assert.Throws<SetupFailed>(() => When(_ => _.GetModelsAsync()).Then().Completes());
        error.Message.Is(
            """
            IMyRepository returns a Task<IEnumerable<MyModel>>.
            Interface types returned as task must be provided explicitly in the test setup.
            You can provide a default interface instance with 'Given<IMyRepository>().Returns(A<IEnumerable<MyModel>>)'.
            """);
    }
}
