using TSpec.Architecture;
using TSpec.Assert;

namespace TSpec.Test.Architecture;

public class WhenIsSpecProject : Spec
{
    [Fact]
    public void GivenAProjectThatReferencesTSpec_ThenIsASpecProject()
        => new SourceProjects(new() { ["A.Spec"] = ["A", "P:TSpec"], ["A"] = [] })["A.Spec"].IsSpecProject.Is(true);

    [Fact]
    public void GivenAProjectThatReachesTSpecThroughAnother_ThenIsASpecProject()
        => new SourceProjects(new() { ["A.Test"] = ["Spec.Common"], ["Spec.Common"] = ["P:TSpec"] })["A.Test"]
            .IsSpecProject.Is(true);

    [Fact]
    public void GivenAProjectThatDoesNotReachTSpec_ThenIsNoSpecProject()
        => new SourceProjects(new() { ["A.Test"] = ["B", "P:xunit.v3"], ["B"] = [] })["A.Test"].IsSpecProject.Is(false);
}
