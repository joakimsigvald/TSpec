using TSpec.Architecture;
using TSpec.Assert;

#pragma warning disable CS0618 // The obsolete Redundant is what these specify: that it still asserts as it did.

namespace TSpec.Test.Architecture;

public class WhenAssertTheObsoleteRedundant : Spec
{
    [Fact]
    public void GivenNoDependencyIsReachedThroughAnother_ThenCompletes()
        => new SourceProjects(new() { ["Host"] = ["Entry"], ["Entry"] = ["Contract"] }).Is().not.Redundant();

    [Fact]
    public void GivenADependencyAnotherAlreadyReaches_ThenGetException()
    {
        var dependencies = new SourceProjects(new() { ["Host"] = ["Entry", "Contract"], ["Entry"] = ["Contract"] });
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => dependencies.Is().not.Redundant());
        ex.HasMessage("""Expected dependencies to not be redundant but found 1: ["Host -> Contract"]""");
    }
}
