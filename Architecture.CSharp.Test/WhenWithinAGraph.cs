using TSpec.Assert;

namespace TSpec.Architecture.CSharp.Test;

public class WhenWithinAGraph : Spec
{
    private static readonly DependencyGraph _folders = new("Folder", new Dictionary<string, IReadOnlyList<string>>
    {
        ["Workbench"] = ["Generation", "Chat"],
        ["Generation"] = ["Chat"],
        ["Chat"] = [],
    });

    [Fact]
    public void ThenListTheNodesAndWhatEachDependsOnInOrdinalOrder()
    {
        _folders.Keys.Is().EqualTo(["Chat", "Generation", "Workbench"]);
        _folders["Workbench"].Is().EqualTo(["Chat", "Generation"]);
    }

    [Fact]
    public void GivenEveryDependencyIsAllowed_ThenCompletes()
        => _folders.Is().Within(folder => folder switch
        {
            "Generation" => ["Chat"],
            "Workbench" => ["Generation", "Chat"],
            _ => [],
        });

    [Fact]
    public void GivenADependencyIsNotAllowed_ThenGetException()
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => _folders.Is().Within(folder => folder switch
            {
                "Generation" => ["Chat"],
                "Workbench" => ["Generation"],
                _ => [],
            }));
        ex.Message.Is("""Expected _folders to be within what is allowed but found 1: ["Workbench -> Chat"]""");
    }

    [Fact]
    public void GivenAnotherRuleAllowsTheDependency_ThenCompletes()
        => _folders.Is().Within(folder => folder switch { "Workbench" => ["Generation"], _ => [] }, _ => ["Chat"]);

    [Fact]
    public void GivenATargetEndingInStar_ThenAllowEveryNameItBegins()
        => _folders.Is().Within(_ => ["Ch*", "Gen*"]);

    [Fact]
    public void ThenSpecifyItAsWithinWhatIsAllowed()
    {
        _folders.Is().Within(_ => ["Chat", "Generation"]);
        Specification.Is("_folders is within what is allowed");
    }
}
