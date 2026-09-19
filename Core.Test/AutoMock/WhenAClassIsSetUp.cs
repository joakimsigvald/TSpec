using TSpec.Assert;
using static TSpec.Times;

namespace TSpec.Test.AutoMock;

/// Built to be mocked, as the Azure SDK clients are: virtual members and a protected constructor for the mock.
public class VirtualClient
{
    protected VirtualClient() { }

    public VirtualClient(Uri endpoint) => Endpoint = endpoint;

    public Uri? Endpoint { get; }

    public virtual string Fetch() => "real";

    public string Describe() => $"own {Fetch()}";
}

public class VirtualClientService(VirtualClient client)
{
    public string Fetch() => client.Fetch();
    public string Describe() => client.Describe();
}

public sealed class SealedClient
{
    public string Fetch() => "real";
}

public class SealedClientService(SealedClient client)
{
    public string Fetch() => client.Fetch();
}

/// A class the test sets up is mocked, as an interface is; one it does not set up is built as any input is.
public class WhenAClassIsSetUp : Spec<VirtualClientService, string>
{
    public WhenAClassIsSetUp() => When(_ => _.Fetch());

    [Fact]
    public void GivenACall_ThenTheSubjectReceivesTheMock()
        => Given<VirtualClient>().That(_ => _.Fetch()).Returns(() => "mocked")
            .Then().Result.Is("mocked");

    [Fact]
    public void GivenADefault_ThenTheSubjectReceivesTheMock()
        => Given<VirtualClient>().Returns(() => "mocked")
            .Then().Result.Is("mocked");

    [Fact]
    public void GivenACall_ThenItsCallsCanBeVerified()
        => Given<VirtualClient>().That(_ => _.Fetch()).Returns(() => "mocked")
            .Then<VirtualClient>(_ => _.Fetch(), Once);

    [Fact]
    public void GivenNothing_ThenTheSubjectReceivesARealOne()
        => Then().Result.Is("real");
}

/// A member a mock cannot override runs its own code on the mock, and setting it up is refused.
public class WhenANonVirtualMemberOfAMockedClassIsCalled : Spec<VirtualClientService, string>
{
    public WhenANonVirtualMemberOfAMockedClassIsCalled() => When(_ => _.Describe());

    [Fact]
    public void GivenAVirtualMemberIsSetUp_ThenItRunsItsOwnCodeOnTheMock()
        => Given<VirtualClient>().That(_ => _.Fetch()).Returns(() => "mocked")
            .Then().Result.Is("own mocked");

    [Fact]
    public void GivenItIsSetUp_ThenSetupFailsSayingWhy()
        => Xunit.Assert.Throws<SetupFailed>(() =>
            Given<VirtualClient>().That(_ => _.Describe()).Returns(() => "mocked").Then())
            .Message.Is(
                "VirtualClient.Describe is not virtual or abstract, so nothing can intercept it. "
                + "Only a member the mock can override may be set up or verified");
}

/// A sealed class cannot be mocked, so setting it up is refused however it is set up.
public class WhenASealedClassIsSetUp : Spec<SealedClientService, string>
{
    private const string Refusal = "SealedClient is sealed, so it cannot be mocked. Provide one with Using instead";

    public WhenASealedClassIsSetUp() => When(_ => _.Fetch());

    [Fact]
    public void GivenACall_ThenSetupFailsSayingWhy()
        => Xunit.Assert.Throws<SetupFailed>(() =>
            Given<SealedClient>().That(_ => _.Fetch()).Returns(() => "mocked").Then())
            .Message.Is(Refusal);

    [Fact]
    public void GivenADefault_ThenSetupFailsSayingWhy()
        => Xunit.Assert.Throws<SetupFailed>(() =>
            Given<SealedClient>().Returns(() => "mocked").Then())
            .Message.Is(Refusal);

    [Fact]
    public void GivenNothing_ThenTheSubjectReceivesARealOne()
        => Then().Result.Is("real");
}
