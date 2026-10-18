using TSpec.Architecture;
using TSpec.Assert;

namespace TSpec.Architecture.CSharp.Test;

public class WhenReadWhichProjectsSeeItsInternals : Spec
{
    [Fact]
    public void ThenListTheProjectsItOpensItsInternalsTo()
        => Source.Projects["."].OpensInternalsTo.Is().EqualTo(["Test"]);

    [Fact]
    public void GivenAProjectThatOpensThemToNone_ThenListNone()
        => Source.Projects["Test"].OpensInternalsTo.Is().Empty();
}
