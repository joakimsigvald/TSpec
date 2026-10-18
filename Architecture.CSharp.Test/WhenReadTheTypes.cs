using TSpec.Architecture;
using TSpec.Assert;
using static TSpec.Architecture.CSharp.Test.DeclaredFiles;

namespace TSpec.Architecture.CSharp.Test;

/// <summary>
/// The types a file or a project declares, read from the build: the declarations are in Declared.
/// </summary>
public class WhenReadTheTypes : Spec
{
    [Fact]
    public void ThenListTheTypesAFileDeclaresAtItsTopLevel()
        => DeclaredFile("Kinds.cs").Types.Select(type => type.Name).Is().EqualTo(
        [
            "AClass", "ADelegate", "AFileLocal", "AGeneric", "ARecord", "ARecordStruct", "AStruct",
            "AnEnum", "AnInterface", "AnInternalClass", "AnOwner",
        ]);

    [Fact]
    public void ThenTellWhatKindEachIs()
        => DeclaredFile("Kinds.cs").Types.Select(type => (type.Name, type.Kind)).Is().EqualTo(
        [
            ("AClass", DeclaredKind.Class), ("ADelegate", DeclaredKind.Delegate), ("AFileLocal", DeclaredKind.Class),
            ("AGeneric", DeclaredKind.Class), ("ARecord", DeclaredKind.Record),
            ("ARecordStruct", DeclaredKind.RecordStruct), ("AStruct", DeclaredKind.Struct), ("AnEnum", DeclaredKind.Enum),
            ("AnInterface", DeclaredKind.Interface), ("AnInternalClass", DeclaredKind.Class), ("AnOwner", DeclaredKind.Class),
        ]);

    [Fact]
    public void ThenTellWhichArePublic()
        => DeclaredFile("Kinds.cs").Types.Where(type => !type.IsPublic).Select(type => type.Name)
            .Is().EqualTo(["AFileLocal", "AnInternalClass"]);

    [Fact]
    public void ThenGiveItsFullNameWithItsNamespace()
        => DeclaredFile("Kinds.cs").Types[0].FullName.Is("TSpec.Architecture.CSharp.Test.Declared.AClass");

    [Fact]
    public void GivenAGenericType_ThenItsFullNameHasItsTypeParameters()
        => DeclaredFile("Kinds.cs").Types.Single(type => type.Name == "AGeneric").FullName
            .Is("TSpec.Architecture.CSharp.Test.Declared.AGeneric<TKey, TValue>");

    [Fact]
    public void ThenTellItsNamespace()
        => DeclaredFile("Kinds.cs").Types[0].Namespace.Is("TSpec.Architecture.CSharp.Test.Declared");

    [Fact]
    public void GivenAPartialType_ThenListEachOfItsFiles()
        => DeclaredFile("APartial.cs").Types.Single().Files.Select(file => file.Path)
            .Is().EqualTo(["Declared/APartial.More.cs", "Declared/APartial.cs"]);

    [Fact]
    public void GivenAPartialType_ThenItIsTheSameTypeOnEachFile()
        => DeclaredFile("APartial.cs").Types.Single().Is().SameAs(DeclaredFile("APartial.More.cs").Types.Single());

    [Fact]
    public void GivenAProject_ThenListTheTypesOfItsFilesInTheOrdinalOrderOfTheirFullNames()
        => Source.Projects["Test"].Types
            .Where(type => type.Namespace == "TSpec.Architecture.CSharp.Test.Declared" && type.Name != Compiled)
            .Select(type => type.Name).Is().EqualTo(
            [
                "AClass", "ADelegate", "AFileLocal", "AGeneric", "APartial", "ARecord", "ARecordStruct", "AStruct",
                "AnEnum", "AnInterface", "AnInternalClass", "AnOwner",
            ]);

    [Fact]
    public void GivenAProject_ThenListAPartialTypeOnce()
        => Source.Projects["Test"].Types.Count(type => type.Name == "APartial").Is(1);

    [Fact]
    public void GivenADeclarationTheBuildLeftOut_ThenLeaveItOut()
        => DeclaredFile("Conditional.cs").Types.Select(type => type.Name).Is().EqualTo([Compiled]);

    [Fact]
    public void GivenAFailure_ThenNameEachTypeByItsFullName()
        => Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => DeclaredFile("APartial.cs").Types.Is().Empty())
            .Message.Does().EndWith("but found 1: [TSpec.Architecture.CSharp.Test.Declared.APartial]");
}
