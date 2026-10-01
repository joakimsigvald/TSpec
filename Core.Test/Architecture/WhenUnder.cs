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
        => _dependencies.Under("Host")["Entry"].Is().EqualTo(["/Other.Lib", "Contract"]);

    [Fact]
    public void ThenNameTheRootItselfDot() => _dependencies.Under("Host")["."].Is().EqualTo(["/HostTools", "Entry"]);

    [Fact]
    public void ThenMarkNamesOutsideTheRootWithASlash()
        => _dependencies.Under("Host").Projects.Is().EqualTo([".", "/HostTools", "Contract", "Entry"]);

    [Fact]
    public void ThenKeepPackageNamesAsTheyAre()
        => new ProjectGraph { ["Host.Entry"] = ["P:Host.Logging"] }.Under("Host")["Entry"]
            .Is().EqualTo(["P:Host.Logging"]);

    [Fact]
    public void GivenADependencyIsNotAllowed_ThenNameItAsTheSwitchDoes()
        => Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => _dependencies.Under("Host").Is().Within(p => p switch
            {
                "." => ["Entry", "/HostTools"],
                "Entry" => ["Contract"],
                _ => [],
            }))
            .Message.Does().EndWith("""but found ["Entry -> /Other.Lib"]""");
}
