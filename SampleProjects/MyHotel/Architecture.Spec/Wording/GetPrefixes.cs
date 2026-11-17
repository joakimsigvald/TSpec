using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Wording;

/// <summary>CODING-STANDARD-BACKEND.md 4c as an invariant over the source: a query is named by the noun it
/// returns and a command by a verb, so no method, local function or property of ours starts with the word
/// Get. An override or an interface's implementation keeps the name its base or interface gave it
/// (GetHashCode, GetEnumerator). One fact per project (Sources).</summary>
public class GetPrefixes : TSpec.Spec
{
    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void NoMemberIsNamedGetSomething(string project) =>
        Source.Projects[project].Members.Has().None(NamedGetSomething);

    private static bool NamedGetSomething(DeclaredMember member) =>
        member.Kind is MemberKind.Method or MemberKind.LocalFunction or MemberKind.Property
        && StartsWithGet(member.Name)
        && member.IsOriginal;

    /// <summary>Get as a word of its own - Get, GetRows - and not the start of one, as in Getaway.</summary>
    private static bool StartsWithGet(string name) =>
        name.StartsWith("Get", StringComparison.Ordinal) && (name.Length == 3 || char.IsUpper(name[3]));
}
