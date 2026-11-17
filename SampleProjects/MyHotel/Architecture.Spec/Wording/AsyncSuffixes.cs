using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Wording;

/// <summary>CODING-STANDARD-BACKEND.md 5a as an invariant over the source: no method of ours ends in Async -
/// the suffix says nothing the return type does not. An override or an interface's implementation keeps the
/// name the framework gave it.</summary>
public class AsyncSuffixes : TSpec.Spec
{
    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void NoMethodCarriesTheAsyncSuffix(string project) =>
        Source.Projects[project].Members.Has().None(AsyncSuffixed);

    private static bool AsyncSuffixed(DeclaredMember member) =>
        member.Kind is MemberKind.Method or MemberKind.LocalFunction
        && member.Name.EndsWith("Async", StringComparison.Ordinal)
        && member.IsOriginal;
}
