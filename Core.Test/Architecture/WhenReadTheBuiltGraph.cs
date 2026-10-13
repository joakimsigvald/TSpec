using TSpec.Architecture;
using TSpec.Assert;

namespace TSpec.Test.Architecture;

public class WhenReadTheBuiltGraph : Spec
{
    private static SourceProjects Graph => SourceProjects.Parse(MyHotelBuild.DepsJson, "MyHotel.Spec");

    [Fact]
    public void ThenListEveryProjectTheRunningSpecProjectToo()
        => Graph.Select(project => project.Name).Is().EqualTo(
            ["MyHotel", "MyHotel.Contract", "MyHotel.Core", "MyHotel.Entry", "MyHotel.Infra", "MyHotel.Spec"]);

    [Fact]
    public void ThenNameTheRunningSpecProjectCurrentSpec()
        => Graph.CurrentSpec.Name.Is("MyHotel.Spec");

    [Fact]
    public void GivenARoot_ThenNameTheCurrentSpecUnderIt()
        => Graph.Under("MyHotel").CurrentSpec.Name.Is("Spec");

    [Fact]
    public void ThenGiveEachProjectWhatItDependsOnDirectly()
        => Graph["MyHotel"].Dependencies.Is().EqualTo(["MyHotel.Entry", "MyHotel.Infra", "P:Scalar.AspNetCore"]);

    [Fact]
    public void ThenNamePackagesWithP()
        => Graph["MyHotel.Core"].Dependencies.Is().EqualTo(
            ["MyHotel.Contract", "P:Microsoft.Extensions.DependencyInjection.Abstractions"]);

    [Fact]
    public void GivenAFailure_ThenNameEachProjectByItsName()
        => Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => Graph.Where(project => project.Name.StartsWith("MyHotel.C", StringComparison.Ordinal)).Is().Empty())
            .Message.Does().EndWith("but found [MyHotel.Contract, MyHotel.Core]");

    [Fact]
    public void GivenTheRunningSpecProject_ThenGiveWhatItDependsOnToo()
        => Graph.CurrentSpec.Dependencies.Is().EqualTo(["MyHotel", "MyHotel.Contract", "P:xunit.v3"]);

    [Fact]
    public void GivenANameThatIsNoProject_ThenRefuse()
        => Xunit.Assert.Throws<SetupFailed>(() => Graph["MyHotel.Entyr"])
            .Message.Is("TSpec found no project 'MyHotel.Entyr' in the build.");

    [Fact]
    public void GivenTheRunningSpecProject_ThenReadItsOwnBuild()
        => Source.Projects.Select(project => project.Name).Is().EqualTo(["TSpec", "TSpec.Test"]);
}
