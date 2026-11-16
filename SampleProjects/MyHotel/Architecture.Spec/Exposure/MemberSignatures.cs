using Microsoft.CodeAnalysis;
using TSpec.Architecture.CSharp;

namespace MyHotel.Architecture.Spec.Exposure;

/// <summary>A member as the signature rules read it: whether another class may call it - public or internal,
/// an interface's member too - and whether a type it gives or takes is of a kind, is a type or meets a
/// condition.</summary>
internal static class MemberSignatures
{
    extension(DeclaredMember member)
    {
        public bool IsExposed =>
            member.Accessibility
                is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal;

        public bool Carries(Func<ITypeSymbol, bool> condition) =>
            member.Gives(condition) || member.Takes(condition);

        public bool Carries(TypeKind kind) => member.Gives(kind) || member.Takes(kind);
        public bool Carries(Type type) => member.Gives(type) || member.Takes(type);
    }
}
