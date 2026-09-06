using TSpec.Assert;
using TSpec.Internal.Document;
using TSpec.Internal.Document.RenderPipeline;
using TSpec.Internal.Specification;

namespace TSpec.Test.Internal.Document;

/// <summary>
/// A subject heading links to the file its class is written in, by a path relative to the spec
/// project — the one root every reader of the file shares. Nothing else links: a given-class or a
/// test method sits partway down a file, and a link that cannot say where would open at the top
/// and read as broken in a reader that does not follow line anchors, Visual Studio among them.
/// </summary>
public class WhenLinkToSource : Spec
{
    private static readonly string _root = Path.Combine(Path.GetTempPath(), "MyHotel.Spec");
    private static readonly string _file = In("Api/WhenGetVersion.cs");

    private static readonly SpecificationEntry _respondOk = new(
        "WhenGetVersion", "GivenNothing", "ThenRespondOk",
        [new([new SpecificationStep(StepLayout.Sentence) { Body = "then Result is ok" }])],
        Source: new(_file, 7));

    private static string In(string relative)
        => Path.Combine(_root, relative.Replace('/', Path.DirectorySeparatorChar));

    private static string Render(SpecificationEntry entry, string? root)
        => DocumentRenderer.Render(new("MyHotel", "0.1.0"), "MyHotel.Spec", [entry], root);

    [Fact]
    public void ThenTheSubjectHeadingLinksToItsFile()
        => Render(_respondOk, _root).Does().Contain("## [When get version](Api/WhenGetVersion.cs)\n");

    [Fact]
    public void ThenABranchHeadingAndARequirementDoNotLink()
        => Render(_respondOk, _root).Does()
            .Contain("### Given nothing\n").and.Contain("- **respond ok**");

    [Fact]
    public void GivenNoSource_ThenNoLink()
        => Render(_respondOk with { Source = null }, _root).Does().Contain("## When get version\n");

    [Fact]
    public void GivenNoSourceRoot_ThenNoLink()
        => Render(_respondOk, null).Does().Contain("## When get version\n");

    /// <summary>
    /// A build that maps source paths, as a CI build does, records the file as <c>/_/…</c> from the
    /// repository root. The path under the spec project is then whatever tail of it is a file
    /// there, so the document reads the same whichever build wrote it.
    /// </summary>
    [Fact]
    public void GivenAMappedPath_ThenLinkToTheFileFoundUnderTheRoot()
    {
        using var project = new TempProject("MyHotel.Spec");
        Directory.CreateDirectory(Path.Combine(project.Root, "Api"));
        File.WriteAllText(Path.Combine(project.Root, "Api", "WhenGetVersion.cs"), "");
        Render(_respondOk with { Source = new("/_/Spec/Api/WhenGetVersion.cs", 7) }, project.Root)
            .Does().Contain("## [When get version](Api/WhenGetVersion.cs)\n");
    }

    [Fact]
    public void GivenAMappedPathToNoFileUnderTheRoot_ThenNoLink()
    {
        using var project = new TempProject("MyHotel.Spec");
        Render(_respondOk with { Source = new("/_/Shared/ApiSpec.cs", 7) }, project.Root)
            .Does().Contain("## When get version\n");
    }

    /// A file outside the spec project has no path every reader shares, so it is not linked.
    [Fact]
    public void GivenAFileOutsideTheRoot_ThenNoLink()
    {
        var elsewhere = Path.Combine(Path.GetTempPath(), "Shared", "ApiSpec.cs");
        Render(_respondOk with { Source = new(elsewhere, 7) }, _root)
            .Does().Contain("## When get version\n");
    }
}
