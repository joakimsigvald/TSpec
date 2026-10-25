using TSpec.Architecture;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec;

public class ProjectDependencies : TSpec.Spec
{
    [Fact]
    public void AreOnlyTheseAndNotRedundant()
        => Source.Projects.Is().Within(p => p switch
        {
            "." => ["Entry", "Infra", "P:Scalar.AspNetCore"],
            "Entry" or "Core" => ["Contract"],
            "Infra" => ["Core"],
            "Spec" => ["."],
            "Core.Spec" => ["Core"],
            _ => [],
        }, p => p.Segment(^1) == "Spec" ? ["P:*"] : ["P:Microsoft.*"]).and.NonRedundant();

    [Fact]
    public void EntryNeverReachesCore() => Source.Projects["Entry"].Reaches.Does().not.Contain("Core");
}
