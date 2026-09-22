using TSpec.Assert;
using static TSpec.Times;

namespace TSpec.Test.AutoMock;

/// A property keeps its value: a setup answers every read, else the last set, else the first read.
public class WhenAPropertyKeepsItsValue : Spec<IdHolder, IIdSource>
{
    public WhenAPropertyKeepsItsValue() => When(_ => _.Source);

    [Fact]
    public void GivenNoSet_ThenEveryReadAnswersTheSame()
        => Result.Name.Is(Result.Name);

    [Fact]
    public void GivenASet_ThenAReadAnswersWhatWasSet()
        => (Result.Name = "x").Is(Result.Name);

    [Fact]
    public void GivenASetAfterARead_ThenItOverwritesWhatWasRead()
    {
        var read = Result.Name;
        (Result.Name = "x").Is(Result.Name).and.Not(read);
    }

    [Fact]
    public void GivenTheGetterIsSetUp_ThenASetDoesNotChangeWhatItAnswers()
    {
        Given<IIdSource>().That(_ => _.Name).Returns(() => "arranged");
        Result.Name = "x";
        Result.Name.Is("arranged");
    }

    [Fact]
    public void GivenAnIndexerWasSet_ThenEachIndexKeepsItsOwn()
    {
        Result[1] = "x";
        Result[1].Is("x").And(Result[2]).Is().Not("x");
    }

    [Fact]
    public void GivenTheTestReadItBeforeTheAct_ThenItKeepsThatValue()
        => Then(The<IIdSource>().Name).Is(Result.Name);
}

/// What the subject sets, it reads back.
public class WhenTheSubjectSetsAPropertyItReads : Spec<Renewer, string>
{
    [Fact]
    public void ThenItReadsBackWhatItSet()
        => When(_ => _.Renew("x")).Then().Result.Is("x");

    [Fact]
    public void GivenTheGetterIsSetUp_ThenTheSetIsStillVerified()
        => When(_ => _.Renew("x"))
            .Given<IIdSource>().That(_ => _.Name).Returns(() => "arranged")
            .Then<IIdSource>(_ => Set(_.Name, "x"), Once).and.Result.Is("arranged");
}

/// A class mock keeps its virtual properties as an interface mock does.
public class WhenAClassMockKeepsItsPropertyValue : Spec<VirtualClientService, string>
{
    public WhenAClassMockKeepsItsPropertyValue()
        => When(_ => _.Fetch()).Given<VirtualClient>().That(_ => _.Fetch()).Returns(() => "mocked");

    [Fact]
    public void ThenAVirtualPropertyKeepsWhatIsSet()
    {
        var client = Then(The<VirtualClient>());
        client.Token = "x";
        client.Token.Is("x");
    }
}

/// Where a chain gives a child per address, each child keeps its own values.
public class WhenAChainChildKeepsItsPropertyValue : Spec<ParentService, IChild>
{
    public WhenAChainChildKeepsItsPropertyValue()
        => When(_ => _.ChildOf(1))
            .Given<IParent>().That(_ => _.GetChild(Any<int>()).Get(9)).Returns(() => "child");

    [Fact]
    public void ThenTheChildAtThatAddressKeepsIt()
    {
        Result.Name = "x";
        The<IParent>().GetChild(1).Name.Is("x");
    }

    [Fact]
    public void ThenAnotherChildKeepsItsOwn()
    {
        Result.Name = "x";
        The<IParent>().GetChild(2).Name.Is().Not("x");
    }
}

public class Renewer(IIdSource source)
{
    public string Renew(string name)
    {
        source.Name = name;
        return source.Name;
    }
}
