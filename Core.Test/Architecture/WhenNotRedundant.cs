using TSpec.Architecture;
using TSpec.Assert;

namespace TSpec.Test.Architecture;

public class WhenNotRedundant : Spec
{
    [Fact]
    public void GivenNoDependencyIsReachedThroughAnother_ThenCompletes()
        => new ProjectGraph
        {
            ["Host"] = ["Entry", "Infra"],
            ["Entry"] = ["Contract"],
            ["Infra"] = ["Core"],
            ["Core"] = ["Contract"],
        }.Is().not.Redundant();

    [Fact]
    public void GivenADependencyAnotherAlreadyReaches_ThenGetException()
    {
        var dependencies = new ProjectGraph { ["Host"] = ["Entry", "Contract"], ["Entry"] = ["Contract"] };
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => dependencies.Is().not.Redundant());
        ex.HasMessage("""Expected dependencies to not be redundant but found ["Host -> Contract"]""");
    }

    [Fact]
    public void GivenItIsReachedThroughSeveralProjects_ThenGetException()
    {
        var dependencies = new ProjectGraph
        {
            ["Host"] = ["Infra", "Contract"],
            ["Infra"] = ["Core"],
            ["Core"] = ["Contract"],
        };
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => dependencies.Is().not.Redundant());
        ex.HasMessage("""Expected dependencies to not be redundant but found ["Host -> Contract"]""");
    }

    [Fact]
    public void GivenAPackageAlsoReachedThroughAnotherProject_ThenCompletes()
        => new ProjectGraph { ["Infra"] = ["Core", "P:Logging"], ["Core"] = ["P:Logging"] }.Is().not.Redundant();

    [Fact]
    public void GivenACycle_ThenCompletes() => new ProjectGraph { ["A"] = ["B"], ["B"] = ["A"] }.Is().not.Redundant();
}
