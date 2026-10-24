using TSpec.Architecture;
using TSpec.Assert;

namespace TSpec.Architecture.CSharp.Test;

/// <summary>
/// The types declared inside a type, read from the build: the declarations are in Declared/Nesting.cs.
/// </summary>
public class WhenReadTheNestedTypes : Spec
{
    [Fact]
    public void ThenListTheTypesDeclaredInsideAType_InTheOrdinalOrderOfTheirFullNames()
        => NestedIn("Outer").Select(type => type.Name).Is().EqualTo(["Handler", "Hidden", "IInternal", "Public", "Size"]);

    [Fact]
    public void ThenTellWhatKindEachIs()
        => NestedIn("Outer").Select(type => (type.Name, type.Kind)).Is().EqualTo(
        [
            ("Handler", DeclaredKind.Delegate), ("Hidden", DeclaredKind.Class), ("IInternal", DeclaredKind.Interface),
            ("Public", DeclaredKind.Class), ("Size", DeclaredKind.Enum),
        ]);

    [Fact]
    public void ThenTellWhichArePublic()
        => NestedIn("Outer").Where(type => type.IsPublic).Select(type => type.Name).Is().EqualTo(["Handler", "Public", "Size"]);

    [Fact]
    public void GivenATypeNestedDeeper_ThenItIsNestedInItsOwner()
        => NestedIn("Outer").Single(type => type.Name == "Public").Nested.Select(type => type.Name).Is().EqualTo(["Deeper"]);

    [Fact]
    public void ThenGiveItsFullNameWithTheTypesItIsNestedIn()
        => NestedIn("Outer").Single(type => type.Name == "Public").Nested.Single().FullName
            .Is("TSpec.Architecture.CSharp.Test.Declared.Nesting.Outer.Public.Deeper");

    [Fact]
    public void GivenAnOwnerThatIsNotPublic_ThenNoTypeInItIs()
        => NestedIn("InternalOuter").Single().IsPublic.Is(false);

    [Fact]
    public void GivenAnInterface_ThenATypeInItIsPublicByDefault()
        => NestedIn("IOuter").Single().IsPublic.Is(true);

    [Fact]
    public void GivenAPartialNestedType_ThenListEachOfItsFiles()
        => NestedIn("Split").Single().Files.Select(file => file.Path)
            .Is().EqualTo(["Declared/Nesting.More.cs", "Declared/Nesting.cs"]);

    [Fact]
    public void ThenLeaveThemOutOfTheFilesTypes()
        => Source.Projects["Test"].Files.Single(file => file.Path == "Declared/Nesting.cs").Types.Select(type => type.Name)
            .Is().EqualTo(["IOuter", "InternalOuter", "Outer", "Split"]);

    [Fact]
    public void GivenAFile_ThenAllItsTypesAreThoseItDeclaresAtItsTopLevelAndThoseNestedInThem_InTheOrdinalOrderOfTheirFullNames()
        => NestingFile.AllTypes.Select(type => type.Name).Is().EqualTo(
        [
            "IOuter", "PublicByDefault", "InternalOuter", "PublicByName", "Outer", "Handler", "Hidden", "IInternal",
            "Public", "Deeper", "Size", "Split", "Part",
        ]);

    [Fact]
    public void GivenAProject_ThenAllItsTypesIncludeThoseNestedInThem()
        => Source.Projects["Test"].AllTypes.Where(type => type.Namespace.EndsWith(".Nesting")).Select(type => type.Name)
            .Is().EqualTo(NestingFile.AllTypes.Select(type => type.Name));

    [Fact]
    public void GivenAProject_ThenAllItsTypesListAPartialNestedTypeOnce()
        => Source.Projects["Test"].AllTypes.Count(type => type.Name == "Part").Is(1);

    private static SourceFile NestingFile => Source.Projects["Test"].Files.Single(file => file.Path == "Declared/Nesting.cs");

    private static IReadOnlyList<DeclaredType> NestedIn(string owner)
        => Source.Projects["Test"].Types.Single(type => type.Namespace.EndsWith(".Nesting") && type.Name == owner).Nested;
}
