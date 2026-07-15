using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.AutoMock;

public class WhenGivingMultipleSetupsForMethod : Spec<MyValueIntService, string>
{
    [Fact]
    public void ThenUseTheLatestIfThrows()
        => When(_ => _.GetValue(A<MyValueInt>()))
        .Given<IMyValueIntRepo>().That(_ => _.Get(The<MyValueInt>())).Returns(A<string>)
        .Given<IMyValueIntRepo>().That(_ => _.Get(The<MyValueInt>())).Throws(An<ApplicationException>)
        .Then().Throws<ApplicationException>();

    [Fact]
    public void ThenUseTheLatestIfReturns()
        => When(_ => _.GetValue(A<MyValueInt>()))
        .Given<IMyValueIntRepo>().That(_ => _.Get(The<MyValueInt>())).Throws(An<ApplicationException>)
        .Given<IMyValueIntRepo>().That(_ => _.Get(The<MyValueInt>())).Returns(A<string>)
        .Then().Result.Is(The<string>());
}