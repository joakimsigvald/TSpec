using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Placement;

/// <summary>CODING-STANDARD-BACKEND.md 10f as an invariant over the source: a partial type's other part is a
/// generator's or a framework's, so it is not in the repository - a generated part lands under obj/, the
/// compiler writes the host's Program. A partial type declared in two of our files is a split, which 10f
/// forbids.</summary>
public class Partials : TSpec.Spec
{
    [Fact]
    public void APartialTypeIsDeclaredInOneFile() =>
        Source.Projects.Has().All(project => SplitTypesOf(project).Is().Empty());

    private static IEnumerable<string> SplitTypesOf(SourceProject project) =>
        project.Types
            .Where(type => type.Files.Count > 1)
            .Select(type => $"{type} in {string.Join(", ", type.Files)}");
}
