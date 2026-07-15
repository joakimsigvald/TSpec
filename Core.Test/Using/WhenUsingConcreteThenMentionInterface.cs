using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Using;

public class WhenUsingConcreteThenMentionInterface : Spec<MyService, int>
{
    public WhenUsingConcreteThenMentionInterface() => When(_ => _.GetNextId());

    [Fact]
    public void ThenMentionOfInterfaceReturnsRegisteredInstance()
    {
        const int fakeId = 42;
        Using(new FakeRepository(fakeId));
        A<IMyRepository>().GetNextId().Is(fakeId);
    }
}