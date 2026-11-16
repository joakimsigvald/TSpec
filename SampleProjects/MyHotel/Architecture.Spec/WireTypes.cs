using Microsoft.CodeAnalysis;
using TSpec.Architecture;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec;

/// <summary>CODING-STANDARD-BACKEND.md 7b as an invariant over Contract. The boundary is what Entry
/// serializes, and it carries strings, primitives and named records: no public property of a Contract record
/// is, or holds, an enum or value type of ours, a time type other than DateOnly, object or dynamic. A type
/// with its own JsonConverter crosses as what the converter writes, so its members are not the
/// wire's.</summary>
public class WireTypes : TSpec.Spec
{
    // Every time type but DateOnly, which has one ISO form.
    private static readonly Type[] Untyped =
        [typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan), typeof(TimeOnly)];

    [Fact]
    public void NoWirePropertyCarriesAnEnumATimeOrAnObject() =>
        Contract.Members.Where(IsWireProperty).Has().None(property => Refused(property));

    private static bool Refused(DeclaredMember property) =>
        Untyped.Any(time => property.Gives(time)) || property.Gives(IsRefused);

    /// <summary>Object, dynamic, and Contract's own enums and value types without a converter.</summary>
    private static bool IsRefused(ITypeSymbol type) =>
        type.SpecialType is SpecialType.System_Object
        || type.TypeKind is TypeKind.Dynamic
        || (type.IsValueType && type.ContainingAssembly?.Name == Contract.AssemblyName && !Converted(type));

    private static bool Converted(ITypeSymbol type) =>
        type.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "JsonConverterAttribute");

    /// <summary>A record's public property, a positional one too, that no converter on the record
    /// writes.</summary>
    private static bool IsWireProperty(DeclaredMember member) =>
        member.Kind is MemberKind.Property
        && member.Accessibility is Accessibility.Public
        && member.DeclaringType is { Kind: DeclaredKind.Record or DeclaredKind.RecordStruct } record
        && !record.HasAttribute("JsonConverter");

    private static SourceProject Contract => Source.Projects["Contract"];
}
