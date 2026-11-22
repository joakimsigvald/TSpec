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
    [Fact]
    public void APublicTypeIsNamedByAnotherProjectOrInAPublicSignature() =>
        Source.Projects.Where(Production).Has().All(
            project => PublicTypesOf(project).Has().All(type => Needed(project, type).Is().True()));

    /// <summary>Every project of the graph but the specs, and Contract: the wire, public as a
    /// whole.</summary>
    private static bool Production(SourceProject project) =>
        !project.IsSpecProject && project.Name != "Contract";

    private static IEnumerable<DeclaredType> PublicTypesOf(SourceProject project) =>
        project.AllTypes.Where(type => type.IsPublic);

    private static bool Needed(SourceProject project, DeclaredType type) =>
        UsedElsewhere(project, type) || Exposed(project, type);

    /// <summary>The spec a project opens its internals to may use an internal, so its uses do not
    /// count.</summary>
    private static bool UsedElsewhere(SourceProject project, DeclaredType type) =>
        Sources.Names.Except([project.Name, .. project.OpensInternalsTo])
            .Any(other => Source.Projects[other].Files.Any(file => file.Uses.Contains(type)));

    /// <summary>A type's own signatures may name it whatever it is.</summary>
    private static bool Exposed(SourceProject project, DeclaredType type) =>
        project.OpensInternalsTo.Prepend(project.Name)
            .SelectMany(other => Source.Projects[other].AllTypes)
            .Any(other => other != type && other.Exposes.Contains(type));
}
