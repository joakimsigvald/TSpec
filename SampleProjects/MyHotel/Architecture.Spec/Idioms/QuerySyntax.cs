using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Idioms;

/// <summary>CODING-STANDARD-BACKEND.md 6a as an invariant over the syntax trees: LINQ is written as method
/// calls, never in query syntax (`from x in xs select ...`). One fact per project (Sources).</summary>
public class QuerySyntax : TSpec.Spec
{
    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void NoQueryIsWrittenInQuerySyntax(string project) =>
        Source.Projects[project].Nodes<QueryExpressionSyntax>().Is().Empty();
}
