using TSpec.Architecture;

namespace MyHotel.Spec.Architecture;

public class ProjectDependencies
{
    [Fact]
    public void AreOnlyTheseAndNotRedundant()
        => Project.Dependencies.Under("MyHotel").Is().Within(p => p switch
        {
            "." => ["Entry", "Infra", "P:Scalar.AspNetCore"],
            "Entry" or "Core" => ["Contract"],
            "Infra" => ["Core"],
            _ => [],
        }, _ => ["P:Microsoft.*"]).and.not.Redundant();
}
