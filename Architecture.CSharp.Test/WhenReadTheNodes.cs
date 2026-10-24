using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture;
using TSpec.Assert;
using TSpec.Test.Internal.Document;
using static TSpec.Architecture.CSharp.Test.WrittenFiles;

namespace TSpec.Architecture.CSharp.Test;

public class WhenReadTheNodes : Spec
{
    [Fact]
    public void ThenListTheNodesOfTheKindInTheOrderTheyAreWritten()
        => NodesOf<LambdaExpressionSyntax>("""
            class A
            {
                System.Func<int, int> Twice = x => 2 * x;
                System.Func<int> One = () => 1;
            }
            """).Select(lambda => lambda.Syntax.ToString()).Is().EqualTo(["x => 2 * x", "() => 1"]);

    [Fact]
    public void ThenNameEachByItsFileAndTheLineItStartsOn()
        => NodesOf<LambdaExpressionSyntax>("class A\n{\n    System.Func<int> One = () => 1;\n}\n").Single().ToString()
            .Is("Entry/Rooms/RoomEndpoints.cs:3");

    [Fact]
    public void GivenAFailure_ThenNameEachNodeByItsFileAndLine()
        => Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => NodesOf<ClassDeclarationSyntax>("class A;").Is().Empty())
            .Message.Does().EndWith("[Entry/Rooms/RoomEndpoints.cs:1]");

    [Fact]
    public void ThenGiveTheFileItIsIn()
        => NodesOf<ClassDeclarationSyntax>("class A;").Single().File.ToString().Is("Entry/Rooms/RoomEndpoints.cs");

    [Fact]
    public void ThenTellWhereItStartsAndEnds()
        => NodesOf<ClassDeclarationSyntax>("class A;\nclass B\n{\n}\n")[1].Span.Is(new SourceSpan(new(2, 1), new(4, 2)));

    [Fact]
    public void ThenCountTheLinesItSpans()
        => NodesOf<ClassDeclarationSyntax>("class A;\nclass B\n{\n}\n").Select(type => type.Span.Lines)
            .Is().EqualTo([1, 3]);

    [Fact]
    public void GivenADirective_ThenListItToo()
        => NodesOf<PragmaWarningDirectiveTriviaSyntax>("#pragma warning disable CS0168\nclass A;\n").Single().ToString()
            .Is("Entry/Rooms/RoomEndpoints.cs:1");

    [Fact]
    public void GivenADocumentationComment_ThenListNothingInIt()
        => NodesOf<IdentifierNameSyntax>("/// <see cref=\"A\"/>\nclass A;\n").Is().Empty();

    [Fact]
    public void GivenAProject_ThenListTheNodesOfAllItsFiles()
        => Source.Projects["Test"].Nodes<MethodDeclarationSyntax>().Select(method => method.ToString())
            .Does().Contain("Test/Declared/APartial.More.cs:5");

    private static IReadOnlyList<SourceNode<T>> NodesOf<T>(string code) where T : SyntaxNode
    {
        using var project = new TempProject("Entry");
        return Written(project, code).Nodes<T>();
    }
}
