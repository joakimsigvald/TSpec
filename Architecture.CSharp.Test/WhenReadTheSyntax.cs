using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Assert;
using static TSpec.Architecture.CSharp.Test.DeclaredFiles;

namespace TSpec.Architecture.CSharp.Test;

/// <summary>
/// A file's syntax, read from the build: the declarations are in Declared.
/// </summary>
public class WhenReadTheSyntax : Spec
{
    [Fact]
    public void ThenParseTheFileAsTheBuildDid()
        => DeclaredFile("Conditional.cs").Root.DescendantNodes().OfType<ClassDeclarationSyntax>()
            .Select(type => type.Identifier.Text).Is().EqualTo([Compiled]);

    [Fact]
    public void ThenHandOutTheSameSyntaxEveryTime()
        => DeclaredFile("Kinds.cs").Root.Is().SameAs(DeclaredFile("Kinds.cs").Root);
}
