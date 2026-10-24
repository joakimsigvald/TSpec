using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture;
using TSpec.Assert;

namespace TSpec.Architecture.CSharp.Test;

/// <summary>
/// The members in Signatures/Implementing.cs, read from the build.
/// </summary>
public class WhenReadWhetherAMemberIsOriginal : Spec
{
    [Fact]
    public void GivenAnOverride_ThenItIsNotOriginal()
        => new[] { "Clean", "ToString" }.Has().None(name => MemberOf("Room", name).IsOriginal);

    [Fact]
    public void GivenAnExplicitInterfaceImplementation_ThenItIsNotOriginal()
        => MemberOf("Room", "Close").IsOriginal.Is(false);

    [Fact]
    public void GivenAnImplicitInterfaceImplementation_ThenItIsNotOriginal()
        => new[] { "Number", "Open", "Opened" }.Has().None(name => MemberOf("Room", name).IsOriginal);

    [Fact]
    public void GivenARecordsPositionalPropertyAnInterfaceNames_ThenItIsNotOriginal()
        => MemberOf("Suite", "Floors").IsOriginal.Is(false);

    [Fact]
    public void GivenAMemberThatNamesItself_ThenItIsOriginal()
        => new[] { "Book", "Guests", "_guests", "Room" }.Has().All(name => MemberOf("Room", name).IsOriginal);

    [Fact]
    public void GivenAVirtualOrAbstractMember_ThenItIsOriginal()
        => new[] { "Clean", "Describe" }.Has().All(name => MemberOf("RoomBase", name).IsOriginal);

    [Fact]
    public void GivenAnInterfacesOwnMember_ThenItIsOriginal()
        => MemberOf("IRoom", "Open").IsOriginal.Is(true);

    private static DeclaredMember MemberOf(string type, string name)
        => Source.Projects["Test"].Members.Single(member => member.File.Path == "Signatures/Implementing.cs"
            && member.Name == name
            && member.Syntax.AncestorsAndSelf().OfType<BaseTypeDeclarationSyntax>().First().Identifier.Text == type);
}
