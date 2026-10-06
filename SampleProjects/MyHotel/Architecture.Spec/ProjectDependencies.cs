using TSpec.Architecture;

namespace MyHotel.Architecture.Spec;

public class ProjectDependencies
{
    [Fact]
    public void AreOnlyTheseAndNotRedundant()
        => Project.Dependencies.Under("MyHotel").Is().Within(p => p switch
        {
            "." => ["Entry", "Infra", "P:Scalar.AspNetCore"],
            "Entry" or "Core" => ["Contract"],
            "Infra" => ["Core"],
            "Spec" => ["."],
            "Core.Spec" => ["Core"],
            _ => [],
        }, p => p.Segment(^1) == "Spec" ? ["P:*"] : ["P:Microsoft.*"]).and.not.Redundant();
}
