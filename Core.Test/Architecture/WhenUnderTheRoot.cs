using TSpec.Architecture;
using TSpec.Assert;

namespace TSpec.Test.Architecture;

public class WhenUnderTheRoot : Spec
{
    private static readonly SourceProjects _dependencies = UnderHost(new()
    {
        ["Host"] = ["Host.Entry", "HostTools"],
        ["Host.Entry"] = ["Host.Contract", "Other.Lib"],
        ["Host.Contract"] = [],
        ["HostTools"] = [],
    });

    [Fact]
    public void ThenDropTheRootAndItsDotFromTheNamesUnderIt()
        => EveryReferenceAsReported(_dependencies).Does().Contain("\"Entry -> Contract\"");

    [Fact]
    public void ThenNameTheRootItselfDot()
        => EveryReferenceAsReported(_dependencies).Does().Contain("\". -> Entry\"");

    [Fact]
    public void ThenMarkNamesOutsideTheRootWithASlash()
        => EveryReferenceAsReported(_dependencies).Does().Contain("\". -> /HostTools\"");

    [Fact]
    public void ThenKeepPackageNamesAsTheyAre()
        => EveryReferenceAsReported(UnderHost(new() { ["Host.Entry"] = ["P:Host.Logging"] }))
            .Does().Contain("\"Entry -> P:Host.Logging\"");

    [Fact]
    public void ThenListTheProjectsByTheirNamesUnderIt()
        => _dependencies.Select(project => project.Name).Is().EqualTo([".", "/HostTools", "Contract", "Entry"]);

    [Fact]
    public void ThenGiveTheDependenciesOfAProjectNamedUnderIt()
        => _dependencies["Entry"].Dependencies.Is().EqualTo(["/Other.Lib", "Contract"]);

    [Fact]
    public void ThenGiveTheAssemblyOfAProjectNamedUnderIt()
        => _dependencies["Entry"].AssemblyName.Is("Host.Entry");

    [Fact]
    public void GivenARuleAllowsAPackageNamedLikeTheRoot_ThenCompletes()
        => UnderHost(new() { ["Host.Entry"] = ["P:Host.Logging"] }).Is().Within(_ => ["P:Host.Logging"]);

    [Fact]
    public void GivenADependencyIsNotAllowed_ThenNameItAsTheSwitchDoes()
        => Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => _dependencies.Is().Within(p => p switch
            {
                "." => ["Entry", "/HostTools"],
                "Entry" => ["Contract"],
                _ => [],
            }))
            .Message.Does().EndWith("""but found 1: ["Entry -> /Other.Lib"]""");

    [Fact]
    public void GivenARedundantReference_ThenNameItAsTheSwitchDoes()
        => Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => UnderHost(new() { ["Host"] = ["Host.Entry", "Host.Contract"], ["Host.Entry"] = ["Host.Contract"] })
                .Is().NonRedundant())
            .Message.Does().EndWith("""but found 1: [". -> Contract"]""");

    [Fact]
    public void GivenTheRootsSpecProjectAlsoReachesTheRootThroughAnother_ThenItIsNotRedundant()
        => UnderHost(new() { ["Host.Spec"] = ["Host", "Host.TestHost"], ["Host.TestHost"] = ["Host"] }).Is().NonRedundant();

#pragma warning disable CS0618 // The obsolete Under still names the projects under the root it is given.
    [Fact]
    public void GivenTheObsoleteUnder_ThenNameTheProjectsUnderTheRootItIsGiven()
        => new SourceProjects(new() { ["Host.Entry"] = [] }).Under("Host").Select(project => project.Name).Is().EqualTo(["Entry"]);
#pragma warning restore CS0618

    private static SourceProjects UnderHost(Dictionary<string, IReadOnlyList<string>> references)
        => new(references, spec: "Host.Architecture.Spec");

    private static string EveryReferenceAsReported(SourceProjects dependencies)
        => Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => dependencies.Is().Within(_ => [])).Message;
}
