using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Sizes;

/// <summary>CODING-STANDARD-BACKEND.md 4b and 3d as invariants over the source. A method's ceiling is 4b's
/// own four; a constructor's or a positional record's is 3d's six. A record in a Mirrors folder has the
/// schema's width (0e); a theory's parameters are its data rows. Contract owns the API's schema, so its
/// records are held to the ceiling.</summary>
public class Arities : TSpec.Spec
{
    private const int MethodArguments = 4;
    private const int ConstructorArguments = 6;

    [Fact]
    public void AMethodTakesAtMostTheCeilingsArguments() =>
        Source.Projects.Has().All(
            project => project.Members.Where(MethodOverTheCeiling).Select(Reported).Is().Empty());

    [Fact]
    public void AConstructorTakesAtMostTheCeilingsArguments() =>
        Source.Projects.Has().All(
            project => project.Members.Where(ConstructorOverTheCeiling).Select(Reported).Is().Empty());

    private static bool MethodOverTheCeiling(DeclaredMember member) =>
        member.Kind is MemberKind.Method or MemberKind.LocalFunction
        && !member.HasAttribute("Theory")
        && member.Parameters.Count > MethodArguments;

    private static bool ConstructorOverTheCeiling(DeclaredMember member) =>
        member.Kind is MemberKind.Constructor
        && !member.File.MirrorsASchema
        && member.Parameters.Count > ConstructorArguments;

    private static string Reported(DeclaredMember member) => $"{member}({member.Parameters.Count})";
}
