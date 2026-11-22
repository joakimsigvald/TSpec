using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Idioms;

/// <summary>CODING-STANDARD-BACKEND.md 6a as an invariant over the syntax trees: LINQ is written as method
/// calls, never in query syntax (`from x in xs select ...`).</summary>
public class QuerySyntax : TSpec.Spec
{
    [Fact]
    public void NoQueryIsWrittenInQuerySyntax() =>
        Source.Projects.Has().All(project => project.Nodes<QueryExpressionSyntax>().Is().Empty());
}
