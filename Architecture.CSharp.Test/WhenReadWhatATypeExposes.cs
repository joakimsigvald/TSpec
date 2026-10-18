using TSpec.Architecture;
using TSpec.Assert;

namespace TSpec.Architecture.CSharp.Test;

/// <summary>
/// The types in Exposes: each names Exposed, or itself, in a signature of its own.
/// </summary>
public class WhenReadWhatATypeExposes : Spec
{
    [Fact]
    public void GivenAPublicMember_ThenItExposesTheTypeItReturns()
        => ExposedBy("Returning").Is().EqualTo(["Exposed"]);

    [Fact]
    public void GivenABaseType_ThenItExposesIt()
        => ExposedBy("Deriving").Is().EqualTo(["Exposed"]);

    [Fact]
    public void GivenItsOwnMemberNamesIt_ThenItExposesItself()
        => ExposedBy("SelfNamed").Is().EqualTo(["SelfNamed"]);

    [Fact]
    public void GivenAPrivateMember_ThenItExposesNothingOfIt()
        => ExposedBy("Hiding").Is().Empty();

    [Fact]
    public void GivenATypeOtherAssembliesDoNotSee_ThenItExposesNothing()
        => ExposedBy("Unseen").Is().Empty();

    [Fact]
    public void GivenANestedType_ThenItExposesWhatItsOwnSignatureNames()
        => Exposing("Owning").Nested.Single().Exposes.Select(type => type.Name).Is().EqualTo(["Exposed"]);

    [Fact]
    public void GivenANestedType_ThenItsSignatureIsNotItsOwners()
        => ExposedBy("Owning").Is().Empty();

    private static IEnumerable<string> ExposedBy(string name) => Exposing(name).Exposes.Select(type => type.Name);

    private static DeclaredType Exposing(string name)
        => Source.Projects["Test"].Types.Single(type => type.Namespace.EndsWith(".Exposes") && type.Name == name);
}
