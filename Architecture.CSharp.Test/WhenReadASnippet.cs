using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Assert;

namespace TSpec.Architecture.CSharp.Test;

public class WhenReadASnippet : Spec
{
    [Fact]
    public void ThenANameResolvesAsInTheBuildsFiles()
        => Source.Snippet("using Error = System.Exception; class C { Error _error; }").Members.Single().Gives()[0]
            .ToDisplayString().Is("System.Exception");

    [Fact]
    public void ThenItCanUseTheBuildsTypes()
        => Source.Snippet("class C { TSpec.Architecture.CSharp.Test.Signatures.Room _room; }").Uses.Single().Is()
            .SameAs(Source.Projects["Test"].Types.Single(type => type.Name == "Room"));

    [Fact]
    public void ThenItDeclaresItsTypes()
        => Source.Snippet("class B; class A { class Inner; }").Types.Select(type => type.Name).Is().EqualTo(["A", "B"]);

    [Fact]
    public void ThenItsMembersAreReadAsTheBuildsAre()
        => Source.Snippet("class C { public override string ToString() => \"\"; }").Members.Single().IsOriginal.Is(false);

    [Fact]
    public void ThenAFailureNamesItASnippet()
        => Source.Snippet("\nclass C;").Nodes<ClassDeclarationSyntax>().Single().ToString().Is("Snippet/Snippet.cs:2");
}
