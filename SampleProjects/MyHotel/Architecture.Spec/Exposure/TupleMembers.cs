using Microsoft.CodeAnalysis;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Exposure;

/// <summary>CODING-STANDARD-BACKEND.md 2c as an invariant over the source: a tuple is private to its class,
/// so a public or internal member neither returns nor takes one.</summary>
public class TupleMembers : TSpec.Spec
{
    [Fact]
    public void APublicOrInternalMemberNeitherReturnsNorTakesATuple() =>
        Source.Projects.Has().All(
            project => project.Members.Has().None(member => member.IsExposed && member.Carries(IsTuple)));

    private static bool IsTuple(ITypeSymbol type) => type.IsTupleType;
}
