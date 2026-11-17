using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Placement;

/// <summary>CODING-STANDARD-BACKEND.md 10b and 10c as an invariant over the source: a file declares one
/// public type at its top level and is named after it, and types that differ only in their type parameters
/// share a file. A nested type is public only in a test project, or as a record that stays with its owner (a
/// discriminated union's case). A file in a Mirrors folder (0e) has the schema's grouping. One fact per
/// project (Sources).</summary>
public class TypesPerFile : TSpec.Spec
{
    public static TheoryData<string> Projects => Sources.Projects;
    public static TheoryData<string> Production => [.. Sources.Names.Where(project => !IsTest(project))];

    [Theory, MemberData(nameof(Projects))]
    public void AFileDeclaresOnePublicType(string project) =>
        Source.Projects[project].Files.Where(Ours).Has().None(Crowded);

    [Theory, MemberData(nameof(Projects))]
    public void AFileIsNamedAfterItsPublicType(string project) =>
        Source.Projects[project].Files.Where(Ours).Has().None(Misnamed);

    [Theory, MemberData(nameof(Production))]
    public void OnlyATestProjectNestsAPublicClass(string project) =>
        Source.Projects[project].AllTypes.Where(IsNested).Where(type => type.Files.All(Ours))
            .Has().None(PublicButNoRecord);

    private static bool IsTest(string project) => Source.Projects[project].IsSpecProject;

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
