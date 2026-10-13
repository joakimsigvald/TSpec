using TSpec.Architecture;
using TSpec.Assert;

#pragma warning disable CS0618 // The obsolete graph is what these specify: that it still reads as it did.

namespace TSpec.Test.Architecture;

public class WhenReadTheObsoleteDependencies : Spec
{
    private static ProjectGraph Dependencies => new(SourceProjects.Parse(MyHotelBuild.DepsJson, "MyHotel.Spec"));

    [Fact]
    public void ThenListTheProjectsByName()
        => Dependencies.Projects.Is().EqualTo(["MyHotel", "MyHotel.Contract", "MyHotel.Core", "MyHotel.Entry", "MyHotel.Infra"]);

    [Fact]
    public void ThenGiveWhatAProjectDependsOnByItsName()
        => Dependencies["MyHotel"].Is().EqualTo(["MyHotel.Entry", "MyHotel.Infra", "P:Scalar.AspNetCore"]);

    [Fact]
    public void GivenANameThatIsNoProject_ThenGiveNothing()
        => Dependencies["MyHotel.Entyr"].Is().Empty();

    [Fact]
    public void GivenTheRunningSpecProject_ThenGiveNothingForIt()
        => Dependencies["MyHotel.Spec"].Is().Empty();

    [Fact]
    public void GivenARoot_ThenNameTheProjectsUnderIt()
        => Dependencies.Under("MyHotel")["Entry"].Is().EqualTo(["Contract"]);

    [Fact]
    public void ThenAssertOnItAsBefore()
        => Dependencies.Under("MyHotel").Is().Within(p => p switch
        {
            "." => ["Entry", "Infra", "P:Scalar.AspNetCore"],
            "Entry" or "Core" => ["Contract"],
            "Infra" => ["Core"],
            _ => [],
        }, _ => ["P:Microsoft.*"]).and.not.Redundant();

    [Fact]
    public void GivenTheRunningSpecProject_ThenReadItsOwnBuild()
        => Project.Dependencies.Projects.Is().EqualTo(["TSpec"]);
}
