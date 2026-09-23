using TSpec.Assert;

namespace TSpec.Test.AutoMock;

public interface IIdSource
{
    string Name { get; set; }
    DateTime Created { get; set; }
    string this[int slot] { get; set; }
    int Version { get; }
}

public class IdHolder
{
    public IIdSource Source { get; set; } = null!;
}

public class Labeler(IIdSource source)
{
    public string Label() => source.Name;
    public string LabelAt(int slot) => source[slot];
}

public class HolderLabeler(IdHolder holder)
{
    public string Label() => holder.Source.Name;
}

/// A mock keeps what a setup lambda sets on it, and states it as the lambda is written.
public class WhenASetupSetsAPropertyOnAMock : Spec<Labeler, string>
{
    public WhenASetupSetsAPropertyOnAMock() => When(_ => _.Label());

    [Fact]
    public void ThenTheMockAnswersWhatWasSet()
    {
        Given().A<IIdSource>(s => s.Name = "arranged").Using(The<IIdSource>).Then().Result.Is("arranged");
        Specification.Is(
            """
            Using the IIdSource
            Given a IIdSource with Name = "arranged"
            When Label()
            Then Result is "arranged"
            """);
    }

    [Fact]
    public void GivenTheMockIsHeldByAnotherValue_ThenItIsReachedThere()
        => Given().A<IdHolder>(h => h.Source.Name = "arranged").Using(() => The<IdHolder>().Source)
            .Then().Result.Is("arranged");

    [Fact]
    public void GivenAnyWithASetup_ThenItIsSetToo()
        => Using(Any<IIdSource>(s => s.Name = "arranged")).Then().Result.Is("arranged");

    [Fact]
    public void GivenUsingOnTheMockedType_ThenItIsSetToo()
        => Using<IIdSource>(s => s.Name = "arranged").Then().Result.Is("arranged");

    [Fact]
    public void GivenTheSamePropertyIsSetUp_ThenTheSetupAnswers()
        => Given().A<IIdSource>(s => s.Name = "arranged")
            .Given<IIdSource>().That(_ => _.Name).Returns(() => "set up")
            .Then().Result.Is("set up");
}

/// A setup lambda given with Using runs each time its type is generated, and sets the mock as any other.
public class WhenAUsingSetupSetsAPropertyOnAMock : Spec<HolderLabeler, string>
{
    public WhenAUsingSetupSetsAPropertyOnAMock() => When(_ => _.Label());

    [Fact]
    public void ThenTheMockAnswersWhatWasSet()
        => Using<IdHolder>(h => h.Source.Name = "arranged").Then().Result.Is("arranged");

    [Fact]
    public void GivenATransform_ThenItSetsTheMockToo()
        => Using<IdHolder>(h =>
            {
                h.Source.Name = "arranged";
                return h;
            }).Then().Result.Is("arranged");
}

/// A property set up to answer answers every read, whatever is set on it.
public class WhenAPropertyIsSetUpToAnswer : Spec<Labeler, string>
{
    [Fact]
    public void GivenAProperty_ThenItAnswersTheArrangedValue()
        => When(_ => _.Label())
            .Given<IIdSource>().That(_ => _.Name).Returns(() => "arranged")
            .Then().Result.Is("arranged");

    [Fact]
    public void GivenAnIndexer_ThenItAnswersTheArrangedValue()
    {
        When(_ => _.LabelAt(1))
            .Given<IIdSource>().That(_ => _[1]).Returns(() => "arranged")
            .Then().Result.Is("arranged");
        Specification.Is(
            """
            Given IIdSource[1] returns "arranged"
            When LabelAt(1)
            Then Result is "arranged"
            """);
    }
}

/// A mock held by another value is the mock of its type, so a setup on the type reaches it there too.
public class WhenAHeldMockIsSetUpToAnswer : Spec<HolderLabeler, string>
{
    [Fact]
    public void ThenTheHeldMockAnswersTheArrangedValue()
        => When(_ => _.Label())
            .Given<IIdSource>().That(_ => _.Name).Returns(() => "arranged")
            .Then().Result.Is("arranged");
}
