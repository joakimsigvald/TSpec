using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Placement;

/// <summary>CODING-STANDARD-BACKEND.md 10b and 10c as an invariant over the source: a file declares one
/// public type at its top level and is named after it, and types that differ only in their type parameters
/// share a file. A nested type is public only in a test project, or as a record that stays with its owner (a
/// discriminated union's case). A file in a Mirrors folder (0e) has the schema's grouping.</summary>
public class TypesPerFile : TSpec.Spec
{
    [Fact]
    public void AFileDeclaresOnePublicType() =>
        Source.Projects.Has().All(project => project.Files.Where(Ours).Has().None(Crowded));

    [Fact]
    public void AFileIsNamedAfterItsPublicType() =>
        Source.Projects.Has().All(project => project.Files.Where(Ours).Has().None(Misnamed));

    [Fact]
    public void OnlyATestProjectNestsAPublicClass() =>
        Source.Projects.Where(Production).Has().All(
            project => NestedTypesOf(project).Has().None(PublicButNoRecord));

    private static bool Production(SourceProject project) => !project.IsSpecProject;

    private static IEnumerable<DeclaredType> NestedTypesOf(SourceProject project) =>
        project.AllTypes.Where(IsNested).Where(type => type.Files.All(Ours));

    /// <summary>A nested type is at the top level of none of its files.</summary>
    private static bool IsNested(DeclaredType type) => !type.Files.Any(file => file.Types.Contains(type));

    private static bool Ours(SourceFile file) => !file.MirrorsASchema;
    private static bool Crowded(SourceFile file) => PublicTypesOf(file).Count > 1;

    private static bool Misnamed(SourceFile file) =>
        PublicTypesOf(file) is [var name] && name != file.Name;

    /// <summary>A record that stays with its owner is a discriminated union's case.</summary>
    private static bool PublicButNoRecord(DeclaredType type) =>
        type.IsPublic && type.Kind is not (DeclaredKind.Record or DeclaredKind.RecordStruct);

    /// <summary>By name, so types that differ only in their type parameters count as one.</summary>
    private static IReadOnlyList<string> PublicTypesOf(SourceFile file) =>
        [.. file.Types.Where(type => type.IsPublic).Select(type => type.Name).Distinct()];
}
