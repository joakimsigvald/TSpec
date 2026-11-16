using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Dependencies;

public class ProjectDependencies : TSpec.Spec
{
    [Fact]
    public void AreOnlyTheseAndNotRedundant() =>
        Source.Projects.Is().Within(References, Packages).and.NonRedundant();

    [Fact]
    public void EntryNeverReachesCore() => Source.Projects["Entry"].Reaches.Does().not.Contain("Core");

    private static IEnumerable<string> References(string project) =>
        project switch
        {
            "Host" => ["Entry", "Infra", "P:Scalar.AspNetCore"],
            "Entry" or "Core" => ["Contract"],
            "Infra" => ["Core"],
            "Host.Spec" => ["Host"],
            "Core.Spec" => ["Core"],
            _ => [],
        };

    private static IEnumerable<string> Packages(string project) =>
        project.Segment(^1) == "Spec" ? ["P:*"] : ["P:Microsoft.*"];
}
