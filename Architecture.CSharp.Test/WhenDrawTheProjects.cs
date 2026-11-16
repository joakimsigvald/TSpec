using TSpec.Architecture.CSharp.Internal;
using TSpec.Assert;

namespace TSpec.Architecture.CSharp.Test;

/// <summary>
/// The architecture page's diagram of the projects, and the subject its title names. TSpec lays out the
/// title and the tests below it.
/// </summary>
public class WhenDrawTheProjects : Spec
{
    private static readonly SourceProjects _hotel = new(new()
    {
        ["Company.Hotel"] = ["Company.Hotel.Entry", "P:Scalar.AspNetCore"],
        ["Company.Hotel.Entry"] = ["Company.Hotel.Contract"],
        ["Company.Hotel.Contract"] = [],
        ["Company.Hotel.Spec"] = ["Company.Hotel", "P:TSpec", "P:xunit.v3"],
        ["Company.Hotel.Entry.Spec"] = ["Company.Hotel.Entry", "P:TSpec"],
    }, spec: "Company.Hotel.Architecture.Spec");

    private static readonly Dictionary<string, string> _noIndexes = [];

    [Fact]
    public void ThenDrawTheProjectsUnderTheirRoot()
        => ProjectDiagram.Of(_hotel, _noIndexes).Is(
            """
            ```mermaid
            flowchart TD
                Hotel
                Contract
                Entry
                Entry_Spec["Entry.Spec"]:::spec
                Hotel_Spec["Hotel.Spec"]:::spec
                Hotel --> Entry
                Entry --> Contract
                Entry_Spec -.-> Entry
                Hotel_Spec -.-> Hotel
                classDef spec fill:none,stroke-dasharray:4 3,opacity:0.8
            ```
            """.ReplaceLineEndings("\n"));

    [Fact]
    public void GivenAProjectHasASpecification_ThenLinkItsNodeToTheIndex()
        => ProjectDiagram.Of(_hotel,
            new Dictionary<string, string> { ["Company.Hotel.Spec"] = "../Hotel.Spec/_specification/README.md" })
            .Does().Contain(
                "    Hotel_Spec -.-> Hotel\n"
                + "    click Hotel \"../Hotel.Spec/_specification/README.md\"\n"
                + "    classDef spec");

    [Fact]
    public void GivenAProjectReachesTSpecThroughAnother_ThenDrawItAsASpecProject()
        => ProjectDiagram.Of(new(new()
        {
            ["Hotel"] = [],
            ["Hotel.Spec"] = ["Hotel", "Hotel.Testing"],
            ["Hotel.Testing"] = ["P:TSpec"],
        }), _noIndexes).Does().Contain("    Hotel_Spec[\"Hotel.Spec\"]:::spec\n");

    [Fact]
    public void ThenNameTheSubjectAfterTheRoot()
        => ArchitectureDocument.SubjectOf(_hotel, "Company.Hotel.Architecture.Spec").Is("Hotel");

    [Fact]
    public void GivenNoRoot_ThenNameTheSubjectAfterTheSpecProject()
        => ArchitectureDocument.SubjectOf(new(new() { ["Hotel"] = [], ["Tools"] = [] }), "Hotel.Architecture.Spec")
            .Is("Hotel.Architecture.Spec");
}
