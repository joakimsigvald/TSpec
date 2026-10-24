using TSpec.Architecture;
using TSpec.Assert;

namespace TSpec.Test.Architecture;

public class WhenNonRedundant : Spec
{
    [Fact]
    public void GivenNoDependencyIsReachedThroughAnother_ThenCompletes()
        => new SourceProjects(new()
        {
            ["Host"] = ["Entry", "Infra"],
            ["Entry"] = ["Contract"],
            ["Infra"] = ["Core"],
            ["Core"] = ["Contract"],
        }).Is().NonRedundant();

    [Fact]
    public void GivenADependencyAnotherAlreadyReaches_ThenGetException()
    {
        var dependencies = new SourceProjects(new() { ["Host"] = ["Entry", "Contract"], ["Entry"] = ["Contract"] });
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => dependencies.Is().NonRedundant());
        ex.HasMessage("""Expected dependencies to be non redundant but found 1: ["Host -> Contract"]""");
    }

    [Fact]
    public void GivenItFollowsAPassingWithin_ThenGetException()
    {
        var dependencies = new SourceProjects(new() { ["Host"] = ["Entry", "Contract"], ["Entry"] = ["Contract"] });
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => dependencies.Is().Within(_ => ["*"]).and.NonRedundant());
        ex.HasMessage("""Expected dependencies to be non redundant but found 1: ["Host -> Contract"]""");
    }

    [Fact]
    public void GivenItIsReachedThroughSeveralProjects_ThenGetException()
    {
        var dependencies = new SourceProjects(new()
        {
            ["Host"] = ["Infra", "Contract"],
            ["Infra"] = ["Core"],
            ["Core"] = ["Contract"],
        });
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => dependencies.Is().NonRedundant());
        ex.HasMessage("""Expected dependencies to be non redundant but found 1: ["Host -> Contract"]""");
    }

    [Fact]
    public void GivenAPackageAlsoReachedThroughAnotherProject_ThenCompletes()
        => new SourceProjects(new() { ["Infra"] = ["Core", "P:Logging"], ["Core"] = ["P:Logging"] }).Is().NonRedundant();

    /// MyHotel.Spec reaches MyHotel.Contract through MyHotel too, as a spec project referencing every project does.
    [Fact]
    public void GivenTheRunningSpecProject_ThenLeaveItOut()
        => SourceProjects.Parse(MyHotelBuild.DepsJson, "MyHotel.Spec").Is().NonRedundant();

    [Fact]
    public void GivenACycle_ThenCompletes() => new SourceProjects(new() { ["A"] = ["B"], ["B"] = ["A"] }).Is().NonRedundant();

    [Fact]
    public void GivenItReferencesTheProjectItsNameExtends_ThenCompletes()
        => new SourceProjects(new()
        {
            ["Core.Spec"] = ["Common.Spec", "Core"],
            ["Common.Spec"] = ["Core"],
        }).Is().NonRedundant();

    [Fact]
    public void GivenItReferencesAProjectFurtherUpItsName_ThenGetException()
    {
        var dependencies = new SourceProjects(new()
        {
            ["Core.Rooms.Spec"] = ["Core.Rooms", "Core"],
            ["Core.Rooms"] = ["Core"],
        });
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => dependencies.Is().NonRedundant());
        ex.HasMessage("""Expected dependencies to be non redundant but found 1: ["Core.Rooms.Spec -> Core"]""");
    }

    [Fact]
    public void ThenSpecifyItAsNonRedundant()
    {
        var dependencies = new SourceProjects(new() { ["Host"] = ["Entry"], ["Entry"] = [] });
        dependencies.Is().NonRedundant();
        Specification.Is("Dependencies is non redundant");
    }
}
