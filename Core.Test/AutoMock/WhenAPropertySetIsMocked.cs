using TSpec.Assert;
using TSpec.Internal.Specification;
using static TSpec.Times;

namespace TSpec.Test.AutoMock;

public class Renamer(IIdSource source)
{
    public void Rename(string name) => source.Name = name;

    public void RenameAt(int slot, string name) => source[slot] = name;

    public void RenameIfGiven(string? name)
    {
        if (name is not null)
            source.Name = name;
    }
}

/// A set is named with Set(property, value), and set up or verified as any call is.
public class WhenAPropertySetIsMocked : Spec<Renamer>
{
    [Fact]
    public void GivenASetUpToThrow_ThenTheSetThrows()
    {
        When(_ => _.Rename(""))
            .Given<IIdSource>().That(_ => Set(_.Name, "")).Throws<ArgumentException>()
            .Then().Throws<ArgumentException>();
        Specification.Is(
            """
            Given IIdSource.Name = "" throws ArgumentException
            When Rename("")
            Then throws ArgumentException
            """);
    }

    [Fact]
    public void GivenNoRename_ThenNameIsNeverSet()
    {
        When(_ => _.RenameIfGiven(null))
            .Then<IIdSource>(_ => Set(_.Name, Any<string>()), Never);
        Specification.Is(
            """
            When RenameIfGiven(null)
            Then IIdSource.Name = any string was not invoked
            """);
    }

    [Fact]
    public void GivenARename_ThenNameIsSetToIt()
    {
        When(_ => _.Rename("x"))
            .Then<IIdSource>(_ => Set(_.Name, "x"), Once);
        Specification.Is(
            """
            When Rename("x")
            Then IIdSource.Name = "x" was invoked once
            """);
    }

    [Fact]
    public void GivenTheSetIsTapped_ThenTheTapGetsTheValue()
        => When(_ => _.Rename("x"))
            .Given<IIdSource>().That(_ => Set(_.Name, Any<string>())).Tap((string name) => _names.Add(name)).Returns()
            .Then(_names).Is().EqualTo(["x"]);

    [Fact]
    public void GivenAnIndexer_ThenItsSetIsVerifiedByIndexAndValue()
    {
        When(_ => _.RenameAt(1, "x"))
            .Then<IIdSource>(_ => Set(_[1], "x"), Once);
        Specification.Is(
            """
            When RenameAt(1, "x")
            Then IIdSource[1] = "x" was invoked once
            """);
    }

    [Fact]
    public void GivenAnotherValueWasSet_ThenTheFailureListsIt()
        => Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() =>
            When(_ => _.Rename("y")).Then<IIdSource>(_ => Set(_.Name, "x"), Once))
            .Message.NormalizeLineEndings().Is(
                """
                Expected IIdSource.Name = "x" to be invoked once but was never invoked
                IIdSource received:
                  IIdSource.Name = "y"
                """.NormalizeLineEndings());

    [Fact]
    public void GivenAPropertyWithNoSetter_ThenItIsRefused()
        => Xunit.Assert.Throws<SetupFailed>(() =>
            When(_ => _.Rename("x")).Then<IIdSource>(_ => Set(_.Version, 2), Never))
            .Message.Is("IIdSource.Version has no setter, so it cannot be set");

    [Fact]
    public void GivenSetIsCalled_ThenItIsRefused()
        => Xunit.Assert.Throws<SetupFailed>(() => Set("a", "b"))
            .Message.Is("Set names a property set only inside That(…) or Then<T>(…)");

    private readonly List<string> _names = [];
}
