using TSpec.Assert;

namespace TSpec.Test.AutoMock;

public interface IParent
{
    IChild Child { get; }
    IChild GetChild(int id);
    string Name { get; }
}

public interface IChild
{
    IGrandChild GrandChild { get; }
    string Get(int id);
}

public interface IGrandChild
{
    string Get(int id);
}

public class ParentService(IParent parent)
{
    public string GetFromChild(int id) => parent.Child.Get(id);
    public string GetFromChildOf(int childId, int id) => parent.GetChild(childId).Get(id);
    public string GetFromGrandChild(int id) => parent.Child.GrandChild.Get(id);
    public string GetFromChildrenOf(int firstId, int secondId, int id)
        => $"{parent.GetChild(firstId).Get(id)},{parent.GetChild(secondId).Get(id)}";
    public IChild ChildOf(int id) => parent.GetChild(id);
}

/// A child is reached at an address — the member and the arguments it is called with — and answers
/// the chained setups that match that address.
public class WhenChainedCallsReachDifferentAddresses : Spec<ParentService, string>
{
    [Fact]
    public void GivenASetupForEachAddress_ThenEachChildAnswersItsOwn()
        => Given<IParent>().That(_ => _.GetChild(1).Get(1)).Returns(() => "one")
            .And<IParent>().That(_ => _.GetChild(2).Get(1)).Returns(() => "two")
            .When(_ => _.GetFromChildrenOf(1, 2, 1))
            .Then().Result.Is("one,two");

    [Fact]
    public void GivenASetupForOneAddress_ThenAnotherAddressDoesNotAnswerIt()
        => Given<IParent>().That(_ => _.GetChild(2).Get(1)).Returns(() => "two")
            .When(_ => _.GetFromChildOf(7, 1))
            .Then().Result.Is().Not("two");

    [Fact]
    public void GivenASetupForAnyAddress_ThenItAnswersWhereNoSpecificSetupDoes()
        => Given<IParent>().That(_ => _.GetChild(Any<int>()).Get(1)).Returns(() => "any")
            .And<IParent>().That(_ => _.GetChild(2).Get(1)).Returns(() => "two")
            .When(_ => _.GetFromChildrenOf(2, 7, 1))
            .Then().Result.Is("two,any");

    [Fact]
    public void GivenASetupForAnyAddressLast_ThenItAnswersEveryAddress()
        => Given<IParent>().That(_ => _.GetChild(2).Get(1)).Returns(() => "two")
            .And<IParent>().That(_ => _.GetChild(Any<int>()).Get(1)).Returns(() => "any")
            .When(_ => _.GetFromChildrenOf(2, 7, 1))
            .Then().Result.Is("any,any");

    [Fact]
    public void GivenTwoSetupsThroughTheSameAddress_ThenBothApply()
        => Given<IParent>().That(_ => _.GetChild(1).Get(1)).Returns(() => "first")
            .And<IParent>().That(_ => _.GetChild(1).Get(2)).Returns(() => "second")
            .When(_ => _.GetFromChildOf(1, 1))
            .Then().Result.Is("first");
}

public class WhenAChildIsReturned : Spec<ParentService, IChild>
{
    [Fact]
    public void GivenAChainedSetupMatchesItsAddress_ThenItIsNotTheSharedMock()
        => Given<IParent>().That(_ => _.GetChild(2).Get(1)).Returns(() => "two")
            .When(_ => _.ChildOf(2))
            .Then().Result.Is().Not(The<IChild>());

    [Fact]
    public void GivenNoChainedSetupMatchesItsAddress_ThenItIsTheSharedMock()
        => Given<IParent>().That(_ => _.GetChild(2).Get(1)).Returns(() => "two")
            .When(_ => _.ChildOf(7))
            .Then().Result.Is(The<IChild>());
}

/// A call reached through a member of the mocked service is a call on the mock of that member's type.
public class WhenAChainedCallIsSetUp : Spec<ParentService, string>
{
    [Fact]
    public void GivenAPropertyInTheChain_ThenTheCallAnswers()
        => When(_ => _.GetFromChild(1))
            .Given<IParent>().That(_ => _.Child.Get(1)).Returns(() => "chained")
            .Then().Result.Is("chained");

    [Fact]
    public void GivenAMethodInTheChain_ThenTheCallAnswers()
    {
        Given<IParent>().That(_ => _.GetChild(2).Get(1)).Returns(() => "chained")
            .When(_ => _.GetFromChildOf(2, 1))
            .Then().Result.Is("chained");
        Specification.Is(
            """
            Given IParent.GetChild(2).Get(1) returns "chained"
            When GetFromChildOf(2, 1)
            Then Result is "chained"
            """);
    }

    [Fact]
    public void GivenALongerChain_ThenTheCallAnswers()
        => When(_ => _.GetFromGrandChild(1))
            .Given<IParent>().That(_ => _.Child.GrandChild.Get(1)).Returns(() => "chained")
            .Then().Result.Is("chained");

    [Fact]
    public void ThenTheCallIsMadeOnTheMockOfTheChildsType()
        => When(_ => _.GetFromChild(1))
            .Given<IParent>().That(_ => _.Child.Get(1)).Returns(() => "chained")
            .Then<IChild>(_ => _.Get(1));

    [Fact]
    public void GivenAReceiverTSpecDoesNotMock_ThenThrowSetupFailed()
        => Xunit.Assert.Throws<SetupFailed>(() =>
            When(_ => _.GetFromChild(1))
                .Given<IParent>().That(_ => _.Name.Length).Returns(() => 3)
                .Then().Result.Is("chained"))
            .Message.Is(
                "IParent.Name returns a string, which TSpec does not mock, "
                + "so Length cannot be set up or verified through it");

    [Fact]
    public void GivenAReceiverTSpecDoesNotMockFurtherDown_ThenThrowSetupFailedNamingIt()
        => Xunit.Assert.Throws<SetupFailed>(() =>
            When(_ => _.GetFromChild(1))
                .Given<IParent>().That(_ => _.GetChild(2).Get(1).Trim()).Returns(() => "trimmed")
                .Then().Result.Is("chained"))
            .Message.Is(
                "IChild.Get returns a string, which TSpec does not mock, "
                + "so Trim cannot be set up or verified through it");
}

public class WhenAChainedCallIsVerified : Spec<ParentService, string>
{
    [Fact]
    public void GivenTheCallWasMade_ThenItIsCounted()
        => When(_ => _.GetFromChild(1)).Then<IParent>(_ => _.Child.Get(1));

    [Fact]
    public void GivenAMethodInTheChain_ThenTheCallIsCounted()
    {
        When(_ => _.GetFromChildOf(2, 1)).Then<IParent>(_ => _.GetChild(2).Get(1));
        Specification.Is(
            """
            When GetFromChildOf(2, 1)
            Then IParent.GetChild(2).Get(1)
            """);
    }

    [Fact]
    public void GivenAnotherCallWasMade_ThenItIsNotCounted()
        => When(_ => _.GetFromChild(2)).Then<IParent>(_ => _.Child.Get(1), Times.Never);

    [Fact]
    public void GivenAReceiverTSpecDoesNotMock_ThenThrowSetupFailed()
        => Xunit.Assert.Throws<SetupFailed>(() =>
            When(_ => _.GetFromChild(1)).Then<IParent>(_ => _.Name.Trim()))
            .Message.Is(
                "IParent.Name returns a string, which TSpec does not mock, "
                + "so Trim cannot be set up or verified through it");
}
