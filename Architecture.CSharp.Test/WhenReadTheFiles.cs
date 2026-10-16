using TSpec.Architecture;
using TSpec.Assert;

namespace TSpec.Architecture.CSharp.Test;

public class WhenReadTheFiles : Spec
{
    [Fact]
    public void ThenNameEachFileByItsPathInTheProject()
        => Source.Projects["TSpec.Architecture.CSharp"].Files.Select(file => file.Path).Does().Contain("Internal/ProjectSources.cs");

    [Fact]
    public void ThenLeaveOutWhatTheBuildGeneratesUnderObj()
        => Source.Projects["TSpec.Architecture.CSharp"].Files
            .Where(file => file.Path.StartsWith("obj/", StringComparison.Ordinal)).Is().Empty();

    [Fact]
    public void ThenReadAFileAsItIsWritten()
        => Source.Projects["TSpec.Architecture.CSharp.Test"].Files.Single(file => file.Path == "WhenReadTheFiles.cs")
            .Raw.Does().Contain("public class WhenReadTheFiles : Spec");

    /// An extension member keeps what it computes against the file it was given.
    [Fact]
    public void ThenHandOutTheSameFilesEveryTime()
        => Source.Projects["TSpec.Architecture.CSharp"].Files[0].Is().SameAs(Source.Projects["TSpec.Architecture.CSharp"].Files[0]);

    [Fact]
    public void GivenAFailure_ThenNameEachFileByItsProjectAndPath()
        => Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => Source.Projects["TSpec.Architecture.CSharp"].Files.Where(file => file.Path == "Internal/ProjectSources.cs").Is().Empty())
            .Message.Does().EndWith("but found [TSpec.Architecture.CSharp/Internal/ProjectSources.cs]");

    [Fact]
    public void GivenTheRunningSpecProject_ThenListItsFilesToo()
        => Source.Projects["TSpec.Architecture.CSharp.Test"].Files.Select(file => file.Path).Does().Contain("WhenReadTheFiles.cs");

    [Fact]
    public void GivenTheRunningSpecProject_ThenLeaveOutTheFilesAPackageCompilesIn()
        => Source.Projects["TSpec.Architecture.CSharp.Test"].Files
            .Where(file => file.Path.EndsWith("DefaultRunnerReporters.cs", StringComparison.Ordinal)).Is().Empty();

    [Fact]
    public void GivenAProjectNamedUnderARoot_ThenNameItsFilesSo()
        => Source.Projects.Under("TSpec.Architecture.CSharp")["Test"].Files.Select(file => file.ToString())
            .Does().Contain("Test/WhenReadTheFiles.cs");
}
