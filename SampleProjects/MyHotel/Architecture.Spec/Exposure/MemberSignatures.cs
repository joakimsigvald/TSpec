using Microsoft.CodeAnalysis;
using TSpec.Architecture.CSharp;

namespace MyHotel.Architecture.Spec.Exposure;

/// <summary>A member as the signature rules read it: whether another class may call it - public or internal,
/// an interface's member too.</summary>
internal static class MemberSignatures
{
    extension(DeclaredMember member)
    {
        public bool IsExposed =>
            member.Accessibility
                is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal;
    }
}
