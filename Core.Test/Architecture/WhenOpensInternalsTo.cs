using TSpec.Architecture;
using TSpec.Assert;

#pragma warning disable CS0618 // The deprecated project graph is what these specify: that it still reads and asserts as it did.

namespace TSpec.Test.Architecture;

public class WhenOpensInternalsTo : Spec
{
    [Fact]
    public void ThenListTheProjectsItOpensItsInternalsTo()
        => Source.Projects["."].OpensInternalsTo.Is().EqualTo(["Test"]);

    [Fact]
    public void GivenAProjectThatOpensThemToNone_ThenListNone()
        => Source.Projects["Test"].OpensInternalsTo.Is().Empty();
}
