using TSpec.Assert;
using static TSpec.Times;

namespace TSpec.Test.AutoMock;

public interface IClientFactory
{
    VirtualClient Get(string name);
    SealedClient GetSealed();
}

public class ClientFactoryService(IClientFactory factory)
{
    public string Fetch(string name) => factory.Get(name).Fetch();
    public string FetchSealed() => factory.GetSealed().Fetch();
}

/// A class can be mocked, so a chain goes through one as through an interface, as the Azure clients need.
public class WhenAChainGoesThroughAClass : Spec<ClientFactoryService, string>
{
    [Fact]
    public void GivenACallThroughItIsSetUp_ThenTheMockAnswers()
        => When(_ => _.Fetch("a"))
            .Given<IClientFactory>().That(_ => _.Get("a").Fetch()).Returns(() => "mocked")
            .Then().Result.Is("mocked");

    [Fact]
    public void GivenACallThroughItIsSetUp_ThenItIsVerifiedThroughIt()
        => When(_ => _.Fetch("a"))
            .Given<IClientFactory>().That(_ => _.Get("a").Fetch()).Returns(() => "mocked")
            .Then<IClientFactory>(_ => _.Get("a").Fetch(), Once);

    [Fact]
    public void GivenASealedClass_ThenTheChainIsRefused()
        => Xunit.Assert.Throws<SetupFailed>(() =>
            When(_ => _.FetchSealed())
                .Given<IClientFactory>().That(_ => _.GetSealed().Fetch()).Returns(() => "mocked")
                .Then())
            .Message.Is(
                "IClientFactory.GetSealed returns a SealedClient, which TSpec does not mock, "
                + "so Fetch cannot be set up or verified through it");
}
