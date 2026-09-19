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

/// A mock keeps nothing set on it, so a setup lambda setting its property is refused, not ignored.
public class WhenASetupSetsAPropertyOnAMock : Spec<Labeler, string>
{
    public WhenASetupSetsAPropertyOnAMock() => When(_ => _.Label());

    [Fact]
    public void ThenSetupFailsPointingToReturns()
        => RefusalOf(() => Given().A<IIdSource>(s => s.Name = "arranged").Then())
            .Is("IIdSource is a mock and ignores Name = \"arranged\". "
                + "Arrange it with Given<IIdSource>().That(_ => _.Name).Returns(() => \"arranged\")");

    [Fact]
    public void GivenAValueThatIsNotALiteral_ThenTheMessageLeavesItOut()
        => RefusalOf(() => Given().A<IIdSource>(s => s.Created = new DateTime(2026, 9, 19)).Then())
            .Is("IIdSource is a mock and ignores Created = …. "
                + "Arrange it with Given<IIdSource>().That(_ => _.Created).Returns(() => …)");

    [Fact]
    public void GivenAnIndexer_ThenTheMessageNamesTheIndex()
        => RefusalOf(() => Given().A<IIdSource>(s => s[1] = "arranged").Then())
            .Is("IIdSource is a mock and ignores [1] = \"arranged\". "
                + "Arrange it with Given<IIdSource>().That(_ => _[1]).Returns(() => \"arranged\")");

    [Fact]
    public void GivenTheMockIsHeldByAnotherValue_ThenItIsRefusedToo()
        => RefusalOf(() => Given().A<IdHolder>(h => h.Source.Name = "arranged").Then())
            .Is("IIdSource is a mock and ignores Name = \"arranged\". "
                + "Arrange it with Given<IIdSource>().That(_ => _.Name).Returns(() => \"arranged\")");

    [Fact]
    public void GivenAnyWithASetup_ThenItIsRefusedToo()
        => RefusalOf(() => Any<IIdSource>(s => s.Name = "arranged"))
            .Is("IIdSource is a mock and ignores Name = \"arranged\". "
                + "Arrange it with Given<IIdSource>().That(_ => _.Name).Returns(() => \"arranged\")");

    [Fact]
    public void GivenUsingOnTheMockedType_ThenItIsRefusedToo()
        => RefusalOf(() => Using<IIdSource>(s => s.Name = "arranged").Then())
            .Is("IIdSource is a mock and ignores Name = \"arranged\". "
                + "Arrange it with Given<IIdSource>().That(_ => _.Name).Returns(() => \"arranged\")");

    private static string RefusalOf(Action arrangement)
        => Xunit.Assert.Throws<SetupFailed>(arrangement).Message;
}

/// A setup lambda given with Using runs each time its type is generated, and is refused as any other.
public class WhenAUsingSetupSetsAPropertyOnAMock : Spec<HolderLabeler, string>
{
    public WhenAUsingSetupSetsAPropertyOnAMock() => When(_ => _.Label());

    [Fact]
    public void ThenSetupFailsPointingToReturns()
        => RefusalOf(() => Using<IdHolder>(h => h.Source.Name = "arranged").Then())
            .Is("IIdSource is a mock and ignores Name = \"arranged\". "
                + "Arrange it with Given<IIdSource>().That(_ => _.Name).Returns(() => \"arranged\")");

    [Fact]
    public void GivenATransform_ThenItIsRefusedToo()
        => RefusalOf(() => Using<IdHolder>(h =>
            {
                h.Source.Name = "arranged";
                return h;
            }).Then())
            .Is("IIdSource is a mock and ignores Name = \"arranged\". "
                + "Arrange it with Given<IIdSource>().That(_ => _.Name).Returns(() => \"arranged\")");

    private static string RefusalOf(Action arrangement)
        => Xunit.Assert.Throws<SetupFailed>(arrangement).Message;
}

/// The setup a refusal suggests is one that answers with the value the refused set would have stated.
public class WhenTheSuggestedSetupIsFollowed : Spec<Labeler, string>
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

/// A mock held by another value is the mock of its type, so the suggested setup reaches it there too.
public class WhenTheSuggestedSetupIsFollowedForAHeldMock : Spec<HolderLabeler, string>
{
    [Fact]
    public void ThenTheHeldMockAnswersTheArrangedValue()
        => When(_ => _.Label())
            .Given<IIdSource>().That(_ => _.Name).Returns(() => "arranged")
            .Then().Result.Is("arranged");
}
