using TSpec.Architecture;
using TSpec.Assert;

namespace TSpec.Test.Architecture;

public class WhenUnder : Spec
{
    private static readonly ProjectGraph _dependencies = new()
    {
        ["Host"] = ["Host.Entry", "HostTools"],
        ["Host.Entry"] = ["Host.Contract", "Other.Lib"],
        ["Host.Contract"] = [],
        ["HostTools"] = [],
    };

    [Fact]
    public void ThenDropTheRootAndItsDotFromTheNamesUnderIt()
        => EveryReferenceAsReported(_dependencies.Under("Host")).Does().Contain("\"Entry -> Contract\"");

    [Fact]
    public void ThenNameTheRootItselfDot()
        => EveryReferenceAsReported(_dependencies.Under("Host")).Does().Contain("\". -> Entry\"");

    [Fact]
    public void ThenMarkNamesOutsideTheRootWithASlash()
        => EveryReferenceAsReported(_dependencies.Under("Host")).Does().Contain("\". -> /HostTools\"");

    [Fact]
    public void ThenKeepPackageNamesAsTheyAre()
        => EveryReferenceAsReported(new ProjectGraph { ["Host.Entry"] = ["P:Host.Logging"] }.Under("Host"))
            .Does().Contain("\"Entry -> P:Host.Logging\"");

    [Fact]
    public void ThenListTheProjectsByTheirNamesUnderIt()
        => _dependencies.Under("Host").Projects.Is().EqualTo([".", "/HostTools", "Contract", "Entry"]);

    [Fact]
    public void ThenGiveTheReferencesOfAProjectNamedUnderIt()
        => _dependencies.Under("Host")["Entry"].Is().EqualTo(["/Other.Lib", "Contract"]);

    [Fact]
    public void GivenARuleAllowsAPackageNamedLikeTheRoot_ThenCompletes()
        => new ProjectGraph { ["Host.Entry"] = ["P:Host.Logging"] }.Under("Host").Is().Within(_ => ["P:Host.Logging"]);

    [Fact]
    public void GivenADependencyIsNotAllowed_ThenNameItAsTheSwitchDoes()
        => Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => _dependencies.Under("Host").Is().Within(p => p switch
            {
                "." => ["Entry", "/HostTools"],
                "Entry" => ["Contract"],
                _ => [],
            }))
            .Message.Does().EndWith("""but found 1: ["Entry -> /Other.Lib"]""");

    [Fact]
    public void GivenARedundantReference_ThenNameItAsTheSwitchDoes()
        => Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => new ProjectGraph { ["Host"] = ["Host.Entry", "Host.Contract"], ["Host.Entry"] = ["Host.Contract"] }
                .Under("Host").Is().not.Redundant())
            .Message.Does().EndWith("""but found 1: [". -> Contract"]""");

    [Fact]
    public void GivenTheRootsSpecProjectAlsoReachesTheRootThroughAnother_ThenItIsNotRedundant()
        => new ProjectGraph { ["Host.Spec"] = ["Host", "Host.TestHost"], ["Host.TestHost"] = ["Host"] }
            .Under("Host").Is().not.Redundant();

    private static string EveryReferenceAsReported(ProjectGraph dependencies)
        => Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => dependencies.Is().Within(_ => [])).Message;
}
