using Microsoft.CodeAnalysis.CSharp;
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

    [Fact]
    public void ThenGiveTheCompilersViewOfItsSyntax()
        => Signing.Model.GetTypeInfo(Signing.Nodes<MethodDeclarationSyntax>()
                .Single(method => method.Syntax.Identifier.Text == "Listed").Syntax.ReturnType,
                TestContext.Current.CancellationToken)
            .Type?.ToDisplayString().Is("System.Collections.Generic.List<TSpec.Architecture.CSharp.Test.Signatures.Status>");

    [Fact]
    public void ThenHandOutTheSameModelEveryTime()
        => Signing.Model.Is().SameAs(Signing.Model);

    private static SourceFile Signing
        => Source.Projects["Test"].Files.Single(file => file.Path == "Signatures/Signing.cs");
}
