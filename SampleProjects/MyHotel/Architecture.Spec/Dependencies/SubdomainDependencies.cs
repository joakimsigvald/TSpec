using TSpec.Architecture.CSharp;

namespace MyHotel.Architecture.Spec.Dependencies;

/// <summary>The sub-domains of Core as an invariant over the source: one way, and only along the graph. A
/// folder uses another when its code uses a type the other declares. A folder the graph does not place may
/// use none and none may use it, so a new one is a decision too. Every other project only keeps its folders
/// free of cycles (FolderCycles).</summary>
public class SubdomainDependencies : TSpec.Spec
{
    [Fact]
    public void InCoreASubdomainUsesOnlyTheOnesTheGraphDrawsBelowIt() =>
        Source.Projects["Core"].Folders.Is().Within(CoreGraph);

    private static IEnumerable<string> CoreGraph(string subdomain) =>
        subdomain switch
        {
            "Bookings" => ["Rooms"],
            _ => [],
        };
}
