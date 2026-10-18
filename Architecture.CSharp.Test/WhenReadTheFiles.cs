using TSpec.Architecture;
using TSpec.Assert;

namespace TSpec.Architecture.CSharp.Test;

public class WhenReadTheFiles : Spec
{
    [Fact]
    public void ThenNameEachFileByItsPathInTheProject()
        => Source.Projects["."].Files.Select(file => file.Path).Does().Contain("Internal/ProjectSources.cs");

    [Fact]
    public void ThenLeaveOutWhatTheBuildGeneratesUnderObj()
        => Source.Projects["."].Files
            .Where(file => file.Path.StartsWith("obj/", StringComparison.Ordinal)).Is().Empty();

    [Fact]
    public void ThenReadAFileAsItIsWritten()
        => Source.Projects["Test"].Files.Single(file => file.Path == "WhenReadTheFiles.cs")
            .Raw.Does().Contain("public class WhenReadTheFiles : Spec");

    /// An extension member keeps what it computes against the file it was given.
    [Fact]
    public void ThenHandOutTheSameFilesEveryTime()
        => Source.Projects["."].Files[0].Is().SameAs(Source.Projects["."].Files[0]);

    [Fact]
    public void GivenAFailure_ThenNameEachFileByItsProjectAndPath()
        => Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => Source.Projects["."].Files.Where(file => file.Path == "Internal/ProjectSources.cs").Is().Empty())
            .Message.Does().EndWith("but found [./Internal/ProjectSources.cs]");

    [Fact]
    public void GivenTheRunningSpecProject_ThenListItsFilesToo()
        => Source.Projects["Test"].Files.Select(file => file.Path).Does().Contain("WhenReadTheFiles.cs");

    [Fact]
    public void GivenTheRunningSpecProject_ThenLeaveOutTheFilesAPackageCompilesIn()
        => Source.Projects["Test"].Files
            .Where(file => file.Path.EndsWith("DefaultRunnerReporters.cs", StringComparison.Ordinal)).Is().Empty();

    [Fact]
    public void GivenAProjectNamedUnderARoot_ThenNameItsFilesSo()
        => Source.Projects["Test"].Files.Select(file => file.ToString())
            .Does().Contain("Test/WhenReadTheFiles.cs");
}
