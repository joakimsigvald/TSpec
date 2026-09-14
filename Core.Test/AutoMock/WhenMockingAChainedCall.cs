using TSpec.Assert;

namespace TSpec.Test.AutoMock;

public interface IParent
{
    IChild Child { get; }
    IChild GetChild(int id);
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
        => When(_ => _.GetFromChildOf(2, 1))
            .Given<IParent>().That(_ => _.GetChild(2).Get(1)).Returns(() => "chained")
            .Then().Result.Is("chained");

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
}

public class WhenAChainedCallIsVerified : Spec<ParentService, string>
{
    [Fact]
    public void GivenTheCallWasMade_ThenItIsCounted()
        => When(_ => _.GetFromChild(1)).Then<IParent>(_ => _.Child.Get(1));

    [Fact]
    public void GivenAnotherCallWasMade_ThenItIsNotCounted()
        => When(_ => _.GetFromChild(2)).Then<IParent>(_ => _.Child.Get(1), Times.Never);
}
