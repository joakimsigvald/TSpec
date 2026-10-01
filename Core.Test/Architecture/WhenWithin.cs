using TSpec.Architecture;
using TSpec.Assert;

namespace TSpec.Test.Architecture;

public class WhenWithin : Spec
{
    private static readonly ProjectGraph _dependencies = new()
    {
        ["Entry"] = ["Contract"],
        ["Core"] = ["Contract"],
    };

    [Fact]
    public void GivenEveryDependencyIsAllowed_ThenCompletes()
        => _dependencies.Is().Within(project => project switch
        {
            "Entry" or "Core" => ["Contract"],
            _ => [],
        });

    [Fact]
    public void GivenAnAllowedDependencyIsNotMade_ThenCompletes()
        => _dependencies.Is().Within(project => project switch
        {
            "Entry" => ["Contract"],
            "Core" => ["Contract", "Infra"],
            _ => [],
        });

    [Fact]
    public void GivenADependencyIsNotAllowed_ThenGetException()
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => _dependencies.Is().Within(project => project switch
            {
                "Entry" => ["Contract"],
                _ => [],
            }));
        ex.HasMessage("""Expected _dependencies to be within what is allowed but found ["Core -> Contract"]""");
    }

    [Fact]
    public void GivenAnotherRuleAllowsTheDependency_ThenCompletes()
        => new ProjectGraph { ["Entry"] = ["Common", "Contract"], ["Core"] = ["Common"] }.Is().Within(
            p => p switch
            {
                "Entry" => ["Contract"],
                _ => [],
            },
            _ => ["Common"]);

    [Fact]
    public void GivenNoRuleAllowsTheDependency_ThenGetException()
    {
        var dependencies = new ProjectGraph { ["Entry"] = ["Common", "Infra"] };
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => dependencies.Is().Within(p => p switch { _ => [] }, _ => ["Common"]));
        ex.HasMessage("""Expected dependencies to be within what is allowed but found ["Entry -> Infra"]""");
    }

    [Fact]
    public void GivenATargetEndingInStar_ThenAllowEveryNameItBegins()
        => new ProjectGraph { ["Core"] = ["P:Microsoft.Extensions.Logging", "P:Microsoft.Extensions.Options"] }
            .Is().Within(_ => ["P:Microsoft.*"]);

    [Fact]
    public void GivenATargetEndingInDotStar_ThenNotAllowTheNameBeforeTheDot()
    {
        var dependencies = new ProjectGraph { ["Core"] = ["P:Moq"] };
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => dependencies.Is().Within(_ => ["P:Moq.*"]));
        ex.HasMessage("""Expected dependencies to be within what is allowed but found ["Core -> P:Moq"]""");
    }

    [Fact]
    public void GivenWithinAndNotRedundant_ThenCompletes()
        => _dependencies.Is().Within(project => project switch
        {
            "Entry" or "Core" => ["Contract"],
            _ => [],
        }).and.not.Redundant();
}
