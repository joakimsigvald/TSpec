using TSpec.Architecture;
using TSpec.Assert;

namespace TSpec.Test.Architecture;

public class WhenReadTheFiles : Spec
{
    [Fact]
    public void ThenNameEachFileByItsPathInTheProject()
        => Source.Projects["TSpec"].Files.Select(file => file.Path).Does().Contain("Architecture/SourceProjects.cs");

    [Fact]
    public void ThenLeaveOutWhatTheBuildGeneratesUnderObj()
        => Source.Projects["TSpec"].Files
            .Where(file => file.Path.StartsWith("obj/", StringComparison.Ordinal)).Is().Empty();

    [Fact]
    public void ThenReadAFileAsItIsWritten()
        => Source.Projects["TSpec.Test"].Files.Single(file => file.Path == "Architecture/WhenReadTheFiles.cs")
            .Raw.Does().Contain("public class WhenReadTheFiles : Spec");

    /// An extension member keeps what it computes against the file it was given.
    [Fact]
    public void ThenHandOutTheSameFilesEveryTime()
        => Source.Projects["TSpec"].Files[0].Is().SameAs(Source.Projects["TSpec"].Files[0]);

    [Fact]
    public void GivenAFailure_ThenNameEachFileByItsProjectAndPath()
        => Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => Source.Projects["TSpec"].Files.Where(file => file.Path == "Architecture/SourceProjects.cs").Is().Empty())
            .Message.Does().EndWith("but found [TSpec/Architecture/SourceProjects.cs]");

    [Fact]
    public void GivenTheRunningSpecProject_ThenListItsFilesToo()
        => Source.Projects["TSpec.Test"].Files.Select(file => file.Path).Does().Contain("Architecture/WhenReadTheFiles.cs");

    [Fact]
    public void GivenTheRunningSpecProject_ThenLeaveOutTheFilesAPackageCompilesIn()
        => Source.Projects["TSpec.Test"].Files
            .Where(file => file.Path.EndsWith("DefaultRunnerReporters.cs", StringComparison.Ordinal)).Is().Empty();

    [Fact]
    public void GivenAProjectNamedUnderARoot_ThenNameItsFilesSo()
        => Source.Projects.Under("TSpec")["Test"].Files.Select(file => file.ToString())
            .Does().Contain("Test/Architecture/WhenReadTheFiles.cs");
}
