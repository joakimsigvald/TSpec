using TSpec.Assert;

namespace TSpec.Architecture.CSharp.Test;

public class WhenAcyclic : Spec
{
    [Fact]
    public void GivenNoNodesUseEachOther_ThenCompletes()
        => Graph(("Workbench", ["Generation", "Chat"]), ("Generation", ["Chat"]), ("Chat", [])).Is().Acyclic();

    [Fact]
    public void GivenTwoNodesUseEachOther_ThenGetException()
    {
        var graph = Graph(("A", ["B"]), ("B", ["A"]));
        Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => graph.Is().Acyclic())
            .Message.Is("""Expected graph to be acyclic but found 1: ["A -> B -> A"]""");
    }

    [Fact]
    public void GivenALongerLoop_ThenNameItWhole()
    {
        var graph = Graph(("A", ["B"]), ("B", ["C"]), ("C", ["A"]));
        Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => graph.Is().Acyclic())
            .Message.Is("""Expected graph to be acyclic but found 1: ["A -> B -> C -> A"]""");
    }

    [Fact]
    public void GivenALoopReachedFromOutsideIt_ThenNameTheLoopAlone()
    {
        var graph = Graph(("A", ["B"]), ("B", ["C"]), ("C", ["B"]));
        Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => graph.Is().Acyclic())
            .Message.Is("""Expected graph to be acyclic but found 1: ["B -> C -> B"]""");
    }

    [Fact]
    public void GivenSeveralLoops_ThenNameTheFirstOnly()
    {
        var graph = Graph(("D", ["C"]), ("C", ["D"]), ("B", ["A"]), ("A", ["B"]));
        Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => graph.Is().Acyclic())
            .Message.Is("""Expected graph to be acyclic but found 1: ["A -> B -> A"]""");
    }

    [Fact]
    public void GivenANodeUsesItself_ThenGetException()
    {
        var graph = Graph(("A", ["A"]));
        Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => graph.Is().Acyclic())
            .Message.Is("""Expected graph to be acyclic but found 1: ["A -> A"]""");
    }

    [Fact]
    public void GivenADependencyOnANameThatIsNoNode_ThenCompletes()
        => Graph(("A", ["B", "Outside"]), ("B", [])).Is().Acyclic();

    [Fact]
    public void GivenItFollowsAPassingWithin_ThenGetException()
    {
        var graph = Graph(("A", ["B"]), ("B", ["A"]));
        Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => graph.Is().Within(_ => ["*"]).and.Acyclic())
            .Message.Is("""Expected graph to be acyclic but found 1: ["A -> B -> A"]""");
    }

    [Fact]
    public void ThenSpecifyItAsAcyclic()
    {
        var graph = Graph(("A", ["B"]), ("B", []));
        graph.Is().Acyclic();
        Specification.Is("Graph is acyclic");
    }

    private static DependencyGraph Graph(params (string Node, IReadOnlyList<string> Uses)[] nodes)
        => new("Folder", nodes.ToDictionary(node => node.Node, node => node.Uses));
}
