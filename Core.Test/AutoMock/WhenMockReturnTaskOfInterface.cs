using TSpec.Assert;

namespace TSpec.Test.AutoMock;

/// An unset member returning a task answers with what the same member returning the value would.
public class WhenMockReturnTaskOfInterface : Spec<MyValueIntService, IMyValueIntRepo>
{
    [Fact]
    public void ThenTheTaskHoldsTheMockItself()
        => When(_ => _.GetRepoAsync()).Then().Result.Is(The<IMyValueIntRepo>());
}
