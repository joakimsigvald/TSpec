using Microsoft.CodeAnalysis;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Exposure;

/// <summary>CODING-STANDARD-BACKEND.md 2d as an invariant over the source: no member, whatever its
/// accessibility, takes or returns a mutable collection - an array, or a collection that can be written to. A
/// field that is not private returns its value too; a private one only holds it. An override keeps the
/// signature its base gave it, and an interface's implementation the interface's.</summary>
public class MutableCollections : TSpec.Spec
{
    // The collections of System.Collections.Generic that can be written to, and the interfaces that write.
    private static readonly Type[] Mutable =
    [
        typeof(List<>),
        typeof(HashSet<>),
        typeof(Dictionary<,>),
        typeof(SortedDictionary<,>),
        typeof(SortedList<,>),
        typeof(SortedSet<>),
        typeof(LinkedList<>),
        typeof(Queue<>),
        typeof(Stack<>),
        typeof(IList<>),
        typeof(ICollection<>),
        typeof(ISet<>),
        typeof(IDictionary<,>),
    ];

    [Fact]
    public void NoMemberTakesOrReturnsAMutableCollection() =>
        Source.Projects.Has().All(project => project.Members.Has().None(PassesAMutableCollection));

    private static bool PassesAMutableCollection(DeclaredMember member) =>
        Passes(member) && CarriesAMutableCollection(member) && member.IsOriginal;

    private static bool Passes(DeclaredMember member) =>
        member.Kind is not MemberKind.Field || member.Accessibility is not Accessibility.Private;

    private static bool CarriesAMutableCollection(DeclaredMember member) =>
        member.Carries(TypeKind.Array) || Mutable.Any(member.Carries);
}
