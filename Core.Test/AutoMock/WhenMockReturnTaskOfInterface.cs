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