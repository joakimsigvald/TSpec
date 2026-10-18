using TSpec.Architecture;
using TSpec.Assert;

namespace TSpec.Test.Architecture;

public class WhenReaches : Spec
{
    [Fact]
    public void GivenAChain_ThenListWhatItDependsOnThroughOthersToo()
        => new SourceProjects(new() { ["A"] = ["B"], ["B"] = ["C"], ["C"] = [] })["A"].Reaches.Is().EqualTo(["B", "C"]);

    [Fact]
    public void GivenADiamond_ThenListTheProjectReachedTwiceOnce()
        => new SourceProjects(new() { ["A"] = ["B", "C"], ["B"] = ["D"], ["C"] = ["D"], ["D"] = [] })["A"]
            .Reaches.Is().EqualTo(["B", "C", "D"]);

    [Fact]
    public void GivenAReachedProjectUsesAPackage_ThenListThePackage()
        => new SourceProjects(new() { ["A"] = ["B"], ["B"] = ["P:Scalar"] })["A"].Reaches.Is().EqualTo(["B", "P:Scalar"]);

    [Fact]
    public void GivenARoot_ThenNameWhatItReachesUnderIt()
        => new SourceProjects(new()
        {
            ["Host"] = ["Host.Entry"],
            ["Host.Entry"] = ["Host.Contract", "Other.Lib"],
            ["Host.Contract"] = [],
            ["Other.Lib"] = [],
        }, spec: "Host.Architecture.Spec")["."].Reaches.Is().EqualTo(["/Other.Lib", "Contract", "Entry"]);

    [Fact]
    public void GivenNoReferences_ThenReachNothing()
        => new SourceProjects(new() { ["A"] = [] })["A"].Reaches.Is().Empty();
}
