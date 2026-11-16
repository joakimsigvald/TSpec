using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Exposure;

/// <summary>CODING-STANDARD-BACKEND.md 3b as an invariant over the source: a type of a production project is
/// public because another project uses it, or because a public or protected signature of another public type
/// names it - in its own project, or in the spec it opens its internals to, where a body may use an internal
/// and a signature may not. Everything else is internal. Contract is the wire and public as a
/// whole.</summary>
public class PublicTypes : TSpec.Spec
{
    public static TheoryData<string> Projects => [.. Sources.Names.Where(Production)];

    [Theory, MemberData(nameof(Projects))]
    public void APublicTypeIsNamedByAnotherProjectOrInAPublicSignature(string project) =>
        Source.Projects[project].AllTypes.Where(type => type.IsPublic)
            .Has().All(type => Needed(project, type));

    /// <summary>Every project of the graph but the specs, and Contract: the wire, public as a
    /// whole.</summary>
    private static bool Production(string project) =>
        !Source.Projects[project].IsSpecProject && project != "Contract";

    private static bool Needed(string project, DeclaredType type) =>
        UsedElsewhere(project, type) || Exposed(project, type);

    /// <summary>The spec a project opens its internals to may use an internal, so its uses do not
    /// count.</summary>
    private static bool UsedElsewhere(string project, DeclaredType type) =>
        Sources.Names.Except([project, .. Friends(project)])
            .Any(other => Source.Projects[other].Files.Any(file => file.Uses.Contains(type)));

    /// <summary>A type's own signatures may name it whatever it is.</summary>
    private static bool Exposed(string project, DeclaredType type) =>
        Friends(project).Prepend(project)
            .SelectMany(other => Source.Projects[other].AllTypes)
            .Any(other => other != type && other.Exposes.Contains(type));

    private static IReadOnlyList<string> Friends(string project) => Source.Projects[project].OpensInternalsTo;
}
