using TSpec.Architecture;
using TSpec.Assert;

namespace TSpec.Architecture.CSharp.Test;

/// <summary>
/// The types a file declares, read from the build: the declarations are in Declared.
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
        => Declared("Kinds.cs").Types[0].FullName.Is("TSpec.Architecture.CSharp.Test.Declared.AClass");

    [Fact]
    public void GivenAGenericType_ThenItsFullNameHasItsTypeParameters()
        => Declared("Kinds.cs").Types.Single(type => type.Name == "AGeneric").FullName
            .Is("TSpec.Architecture.CSharp.Test.Declared.AGeneric<TKey, TValue>");

    [Fact]
    public void ThenTellItsNamespace()
        => Declared("Kinds.cs").Types[0].Namespace.Is("TSpec.Architecture.CSharp.Test.Declared");

    [Fact]
    public void GivenAPartialType_ThenListEachOfItsFiles()
        => Declared("APartial.cs").Types.Single().Files.Select(file => file.Path)
            .Is().EqualTo(["Declared/APartial.More.cs", "Declared/APartial.cs"]);

    [Fact]
    public void GivenAPartialType_ThenItIsTheSameTypeOnEachFile()
        => Declared("APartial.cs").Types.Single().Is().SameAs(Declared("APartial.More.cs").Types.Single());

    [Fact]
    public void GivenADeclarationTheBuildLeftOut_ThenLeaveItOut()
        => Declared("Conditional.cs").Types.Select(type => type.Name).Is().EqualTo([Compiled]);

    [Fact]
    public void GivenAFailure_ThenNameEachTypeByItsFullName()
        => Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => Declared("APartial.cs").Types.Is().Empty())
            .Message.Does().EndWith("but found 1: [TSpec.Architecture.CSharp.Test.Declared.APartial]");

#if DEBUG
    private const string Compiled = "ADebugType";
#else
    private const string Compiled = "AReleaseType";
#endif

    private static SourceFile Declared(string file)
        => Source.Projects["TSpec.Architecture.CSharp.Test"].Files.Single(source => source.Path == $"Declared/{file}");
}
