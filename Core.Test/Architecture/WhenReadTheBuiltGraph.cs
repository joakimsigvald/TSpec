using TSpec.Architecture;
using TSpec.Assert;

namespace TSpec.Test.Architecture;

public class WhenReadTheBuiltGraph : Spec
{
    private static SourceProjects Graph => SourceProjects.Parse(MyHotelBuild.DepsJson, "MyHotel.Spec");

    [Fact]
    public void ThenNameTheProjectsUnderTheRootTheSpecProjectShares()
        => Graph.Select(project => project.Name).Is().EqualTo([".", "Contract", "Core", "Entry", "Infra", "Spec"]);

    [Fact]
    public void GivenAProjectFromAnotherSolution_ThenNameItOutsideTheRoot()
        => new SourceProjects(new()
        {
            ["Acme.Hotel"] = ["Acme.Hotel.Entry", "Acme.Common"],
            ["Acme.Hotel.Entry"] = [],
            ["Acme.Common"] = [],
        }, spec: "Acme.Hotel.Architecture.Spec").Select(project => project.Name).Is().EqualTo([".", "/Acme.Common", "Entry"]);

    [Fact]
    public void GivenTheSpecProjectSharesNoRoot_ThenKeepTheFullNames()
        => new SourceProjects(new() { ["Api"] = ["Contract"], ["Contract"] = [] }, spec: "Architecture.Spec")
            .Select(project => project.Name).Is().EqualTo(["Api", "Contract"]);

    [Fact]
    public void ThenNameTheRunningSpecProjectCurrentSpec()
        => Graph.CurrentSpec.Name.Is("Spec");

    [Fact]
    public void ThenGiveEachProjectWhatItDependsOnDirectly()
        => Graph["."].Dependencies.Is().EqualTo(["Entry", "Infra", "P:Scalar.AspNetCore"]);

    [Fact]
    public void ThenNamePackagesWithP()
        => Graph["Core"].Dependencies.Is().EqualTo(["Contract", "P:Microsoft.Extensions.DependencyInjection.Abstractions"]);

    [Fact]
    public void GivenAFailure_ThenNameEachProjectByItsName()
        => Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => Graph.Where(project => project.Name.StartsWith('C')).Is().Empty())
            .Message.Does().EndWith("but found [Contract, Core]");

    [Fact]
    public void GivenTheRunningSpecProject_ThenGiveWhatItDependsOnToo()
        => Graph.CurrentSpec.Dependencies.Is().EqualTo([".", "Contract", "P:xunit.v3"]);

    [Fact]
    public void GivenANameThatIsNoProject_ThenRefuse()
        => Xunit.Assert.Throws<SetupFailed>(() => Graph["Entyr"])
            .Message.Is("TSpec found no project 'Entyr' in the build.");

    [Fact]
    public void GivenTheRunningSpecProject_ThenReadItsOwnBuild()
        => Source.Projects.Select(project => project.Name).Is().EqualTo([".", "Test"]);
}
