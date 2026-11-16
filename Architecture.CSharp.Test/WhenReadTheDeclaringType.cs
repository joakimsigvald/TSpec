using TSpec.Assert;

namespace TSpec.Architecture.CSharp.Test;

/// <summary>
/// The types the members in Declared and Signatures are declared in, read from the build.
/// </summary>
public class WhenReadTheDeclaringType : Spec
{
    [Fact]
    public void ThenAMembersDeclaringTypeIsTheTypeItIsDeclaredIn()
        => MemberIn("Signatures/Implementing.cs", "Book").DeclaringType.Is()
            .SameAs(TypeIn("Signatures/Implementing.cs", "Room"));

    [Fact]
    public void GivenANestedTypesMember_ThenItIsDeclaredInTheNestedType()
        => MemberIn("Declared/Nesting.cs", "Small").DeclaringType.Is()
            .SameAs(TypeIn("Declared/Nesting.cs", "Size"));

    [Fact]
    public void GivenADelegateInAType_ThenItIsDeclaredInThatType()
        => MemberIn("Declared/Nesting.cs", "Handler").DeclaringType.Is()
            .SameAs(TypeIn("Declared/Nesting.cs", "Outer"));

    [Fact]
    public void GivenADelegateAtTheTopLevel_ThenItIsDeclaredInNoType()
        => MemberIn("Declared/Kinds.cs", "ADelegate").DeclaringType.Is().Null();

    [Fact]
    public void GivenAPrimaryConstructor_ThenItIsDeclaredInItsType()
        => MemberIn("Signatures/Implementing.cs", "Suite").DeclaringType.Is()
            .SameAs(TypeIn("Signatures/Implementing.cs", "Suite"));

    [Fact]
    public void GivenAPositionalProperty_ThenItIsDeclaredInItsRecord()
        => Source.Projects["Test"].Members
            .Where(member => member.File.Path == "Signatures/Implementing.cs" && member.Name == "Floors")
            .Select(member => member.DeclaringType?.Name).Is().EqualTo(["IStoried", "Suite"]);

    [Fact]
    public void GivenAPartialTypesMember_ThenItIsDeclaredInTheOneType()
        => MemberIn("Declared/APartial.More.cs", "Second").DeclaringType.Is()
            .SameAs(TypeIn("Declared/APartial.cs", "APartial"));

    [Fact]
    public void GivenAnExtensionBlocksMember_ThenItIsDeclaredInTheBlocksClass()
        => MemberIn("Signatures/Extending.cs", "IsMoving").DeclaringType.Is()
            .SameAs(TypeIn("Signatures/Extending.cs", "Extending"));

    private static DeclaredMember MemberIn(string path, string name)
        => Source.Projects["Test"].Members.Single(member => member.File.Path == path && member.Name == name);

    private static DeclaredType TypeIn(string path, string name)
        => Source.Projects["Test"].Files.Single(file => file.Path == path)
            .AllTypes.Single(type => type.Name == name);
}
