using TSpec.Architecture;
using TSpec.Assert;

namespace TSpec.Test.Architecture;

/// <summary>
/// The types a file declares, read from the build: the declarations are in Architecture/Declared.
/// </summary>
public class WhenReadTheTypes : Spec
{
    [Fact]
    public void ThenListTheTypesAFileDeclaresAtItsTopLevel()
        => Declared("Kinds.cs").Types.Select(type => type.Name).Is().EqualTo(
        [
            "AClass", "ADelegate", "AFileLocal", "AGeneric", "ARecord", "ARecordStruct", "AStruct",
            "AnEnum", "AnInterface", "AnInternalClass", "AnOwner",
        ]);

    [Fact]
    public void ThenTellWhatKindEachIs()
        => Declared("Kinds.cs").Types.Select(type => (type.Name, type.Kind)).Is().EqualTo(
        [
            ("AClass", DeclaredKind.Class), ("ADelegate", DeclaredKind.Delegate), ("AFileLocal", DeclaredKind.Class),
            ("AGeneric", DeclaredKind.Class), ("ARecord", DeclaredKind.Record),
            ("ARecordStruct", DeclaredKind.RecordStruct), ("AStruct", DeclaredKind.Struct), ("AnEnum", DeclaredKind.Enum),
            ("AnInterface", DeclaredKind.Interface), ("AnInternalClass", DeclaredKind.Class), ("AnOwner", DeclaredKind.Class),
        ]);

    [Fact]
    public void ThenTellWhichArePublic()
        => Declared("Kinds.cs").Types.Where(type => !type.IsPublic).Select(type => type.Name)
            .Is().EqualTo(["AFileLocal", "AnInternalClass"]);

    [Fact]
    public void ThenGiveItsFullNameWithItsNamespace()
        => Declared("Kinds.cs").Types[0].FullName.Is("TSpec.Test.Architecture.Declared.AClass");

    [Fact]
    public void GivenAGenericType_ThenItsFullNameHasItsTypeParameters()
        => Declared("Kinds.cs").Types.Single(type => type.Name == "AGeneric").FullName
            .Is("TSpec.Test.Architecture.Declared.AGeneric<TKey, TValue>");

    [Fact]
    public void ThenTellItsNamespace()
        => Declared("Kinds.cs").Types[0].Namespace.Is("TSpec.Test.Architecture.Declared");

    [Fact]
    public void GivenAPartialType_ThenListEachOfItsFiles()
        => Declared("APartial.cs").Types.Single().Files.Select(file => file.Path)
            .Is().EqualTo(["Architecture/Declared/APartial.More.cs", "Architecture/Declared/APartial.cs"]);

    [Fact]
    public void GivenAPartialType_ThenItIsTheSameTypeOnEachFile()
        => Declared("APartial.cs").Types.Single().Is().SameAs(Declared("APartial.More.cs").Types.Single());

    [Fact]
    public void GivenAFailure_ThenNameEachTypeByItsFullName()
        => Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => Declared("APartial.cs").Types.Is().Empty())
            .Message.Does().EndWith("but found 1: [TSpec.Test.Architecture.Declared.APartial]");

    private static SourceFile Declared(string file)
        => Source.Projects["TSpec.Test"].Files.Single(source => source.Path == $"Architecture/Declared/{file}");
}
