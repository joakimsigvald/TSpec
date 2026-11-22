using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Assert;

namespace TSpec.Architecture.CSharp.Test;

/// <summary>
/// The compiler's symbols for the types and members in Declared and Signatures, read from the build.
/// </summary>
public class WhenReadTheSymbols : Spec
{
    [Theory]
    [InlineData("Signatures/Implementing.cs", "Room", "TSpec.Architecture.CSharp.Test.Signatures.Room")]
    [InlineData("Declared/Nesting.cs", "Size", "TSpec.Architecture.CSharp.Test.Declared.Nesting.Outer.Size")]
    [InlineData("Declared/Kinds.cs", "AGeneric", "TSpec.Architecture.CSharp.Test.Declared.AGeneric<TKey, TValue>")]
    public void ThenATypesSymbolIsTheCompilersForIt(string path, string name, string symbol)
        => TypeIn(path, name).Symbol.ToDisplayString().Is(symbol);

    [Theory]
    [InlineData("Book", "TSpec.Architecture.CSharp.Test.Signatures.Room.Book()")]
    [InlineData("Room", "TSpec.Architecture.CSharp.Test.Signatures.Room.Room()")]
    [InlineData("Guests", "TSpec.Architecture.CSharp.Test.Signatures.Room.Guests")]
    [InlineData("_guests", "TSpec.Architecture.CSharp.Test.Signatures.Room._guests")]
    [InlineData("Opened", "TSpec.Architecture.CSharp.Test.Signatures.Room.Opened")]
    public void ThenAMembersSymbolIsTheCompilersForIt(string name, string symbol)
        => MemberOf("Room", name).Symbol.ToDisplayString().Is(symbol);

    [Fact]
    public void GivenAPositionalProperty_ThenItsSymbolIsTheProperty()
        => MemberOf("Suite", "Floors").Symbol.Kind.Is(SymbolKind.Property);

    [Theory]
    [InlineData("Suite", "TSpec.Architecture.CSharp.Test.Signatures.Suite.Suite(int)")]
    [InlineData("Lobby", "TSpec.Architecture.CSharp.Test.Signatures.Lobby.Lobby(int)")]
    public void GivenAPrimaryConstructor_ThenItsSymbolIsTheConstructor(string type, string symbol)
        => MemberOf(type, type).Symbol.ToDisplayString().Is(symbol);

    [Fact]
    public void GivenADelegate_ThenItsSymbolIsTheDelegateType()
        => Source.Projects["Test"].Members.Single(member => member.Name == "ADelegate").Symbol.Kind.Is(SymbolKind.NamedType);

    [Fact]
    public void GivenATypeTheProjectDeclares_ThenFindIt()
        => Found("Test", Given("Plain")).Is().SameAs(TypeIn("Signatures/Signing.cs", "Status"));

    [Fact]
    public void GivenATypeWithTypeArguments_ThenFindItsDefinition()
        => Found("Test", TypeIn("Signatures/Inheriting.cs", "Wrapped").Symbol.BaseType!).Is()
            .SameAs(TypeIn("Signatures/Inheriting.cs", "Wrapper"));

    [Fact]
    public void GivenANestedType_ThenFindIt()
        => Found("Test", TypeIn("Declared/Nesting.cs", "Size").Symbol).Is().SameAs(TypeIn("Declared/Nesting.cs", "Size"));

    [Fact]
    public void GivenATypeAnotherProjectDeclares_ThenFindNone()
        => Found("Test", SourceFileAsTestSeesIt).Is().Null();

    [Fact]
    public void GivenTheProjectThatDeclaresIt_ThenFindIt()
        => (Found(".", SourceFileAsTestSeesIt)?.FullName).Is("TSpec.Architecture.CSharp.SourceFile");

    [Fact]
    public void GivenAFrameworkType_ThenFindNone()
        => Found("Test", Given("Listed")).Is().Null();

    [Fact]
    public void GivenAnArray_ThenFindNone()
        => Found("Test", Given("Arrayed")).Is().Null();

    private static DeclaredType? Found(string project, ITypeSymbol type)
        => Source.Projects[project].TryFind(type, out var found) ? found : null;

    /// <summary>The package's SourceFile as the Test project's compilation resolves it, from metadata.</summary>
    private static ITypeSymbol SourceFileAsTestSeesIt
        => Source.Projects["Test"].Members.Single(member => member.Name == "DeclaredFile").Gives()[0];

    private static ITypeSymbol Given(string member)
        => Source.Projects["Test"].Members
            .Single(declared => declared.File.Path == "Signatures/Signing.cs" && declared.Name == member).Gives()[0];

    private static DeclaredMember MemberOf(string type, string name)
        => Source.Projects["Test"].Members.Single(member => member.File.Path == "Signatures/Implementing.cs"
            && member.Name == name
            && member.Syntax.AncestorsAndSelf().OfType<BaseTypeDeclarationSyntax>().First().Identifier.Text == type);

    private static DeclaredType TypeIn(string path, string name)
        => Source.Projects["Test"].Files.Single(file => file.Path == path).AllTypes.Single(type => type.Name == name);
}
