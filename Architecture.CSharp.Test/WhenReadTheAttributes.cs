using TSpec.Assert;

namespace TSpec.Architecture.CSharp.Test;

/// <summary>
/// The attributes written in Attributed.cs, asked for by name: with or without the Attribute suffix,
/// however they are qualified. OwnAttributed carries a Serializable of this project, not System's.
/// </summary>
public class WhenReadTheAttributes : Spec
{
    [Fact]
    public void GivenAnAttributesName_ThenTheMemberHasIt()
        => MemberNamed("Marked").HasAttribute("Obsolete").Is(true);

    [Fact]
    public void GivenTheNameWithTheSuffix_ThenTheMemberHasIt()
        => MemberNamed("Marked").HasAttribute("ObsoleteAttribute").Is(true);

    [Fact]
    public void GivenAnAttributeWrittenWithTheSuffix_ThenTheNameWithoutItMatches()
        => MemberNamed("MarkedWithSuffix").HasAttribute("Obsolete").Is(true);

    [Fact]
    public void GivenAnAttributeWrittenQualified_ThenItsNameMatches()
        => MemberNamed("MarkedQualified").HasAttribute("Obsolete").Is(true);

    [Fact]
    public void GivenTheFullName_ThenTheMemberHasIt()
        => MemberNamed("Marked").HasAttribute("System.Obsolete").Is(true);

    [Fact]
    public void GivenTheFullNameWithTheSuffix_ThenTheMemberHasIt()
        => MemberNamed("Marked").HasAttribute("System.ObsoleteAttribute").Is(true);

    [Fact]
    public void GivenTheFullNameFromTheGlobalNamespace_ThenTheMemberHasIt()
        => MemberNamed("MarkedWithSuffix").HasAttribute("global::System.Obsolete").Is(true);

    [Fact]
    public void GivenAPlainName_ThenAnAttributeOfThatNameInAnyNamespaceMatches()
        => TypeNamed("OwnAttributed").HasAttribute("Serializable").Is(true);

    [Fact]
    public void GivenItsOwnFullName_ThenTheTypeHasIt()
        => TypeNamed("OwnAttributed").HasAttribute("TSpec.Architecture.CSharp.Test.Own.Serializable").Is(true);

    [Fact]
    public void GivenTheFullNameOfAnotherNamespace_ThenTheTypeDoesNotHaveIt()
        => TypeNamed("OwnAttributed").HasAttribute("System.Serializable").Is(false);

    [Fact]
    public void GivenTheFullNameOfAnotherNamespace_ThenTheFrameworksAttributeDoesNotMatch()
        => TypeNamed("Attributed").HasAttribute("TSpec.Architecture.CSharp.Test.Own.Serializable").Is(false);

    [Fact]
    public void GivenAnotherName_ThenTheMemberDoesNotHaveIt()
        => MemberNamed("Plain").HasAttribute("Obsolete").Is(false);

    [Fact]
    public void ThenATypeHasTheAttributesWrittenOnIt()
        => TypeNamed("Attributed").HasAttribute("Serializable").Is(true);

    [Fact]
    public void GivenAPartialType_ThenItHasAnAttributeWrittenOnAnyOfItsParts()
        => TypeNamed("AttributedPart").HasAttribute("Serializable").Is(true);

    [Fact]
    public void ThenListTheAttributesWrittenOnAType()
        => TypeNamed("Attributed").Attributes.Select(attribute => attribute.Name.ToString())
            .Is().EqualTo(["Serializable"]);

    private static DeclaredMember MemberNamed(string name)
        => Source.Projects["Test"].Members.Single(member => member.File.Path == "Attributed.cs" && member.Name == name);

    private static DeclaredType TypeNamed(string name)
        => Source.Projects["Test"].Files.Single(file => file.Path == "Attributed.cs")
            .Types.Single(type => type.Name == name);
}
