using TSpec.Architecture;
using TSpec.Assert;

namespace TSpec.Test.Architecture;

public class WhenReadTheBuiltGraph : Spec
{
    private static ProjectGraph Graph => ProjectGraph.Parse(MyHotelBuild.DepsJson, "MyHotel.Spec");

    [Fact]
    public void ThenListEveryProjectButTheSpecProject()
        => Graph.Projects.Is().EqualTo(
            ["MyHotel", "MyHotel.Contract", "MyHotel.Core", "MyHotel.Entry", "MyHotel.Infra"]);

    [Fact]
    public void ThenMapEachToWhatItReferencesDirectly()
        => Graph["MyHotel"].Is().EqualTo(["MyHotel.Entry", "MyHotel.Infra", "P:Scalar.AspNetCore"]);

    [Fact]
    public void ThenNamePackagesWithP()
        => Graph["MyHotel.Core"].Is().EqualTo(
            ["MyHotel.Contract", "P:Microsoft.Extensions.DependencyInjection.Abstractions"]);

    [Fact]
    public void GivenTheRunningSpecProject_ThenReadItsOwnBuild()
        => Project.Dependencies.Projects.Is().EqualTo(["TSpec"]);
}
