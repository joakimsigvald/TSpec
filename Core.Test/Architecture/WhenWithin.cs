using TSpec.Architecture;
using TSpec.Assert;
using TSpec.Internal.Specification;

namespace TSpec.Test.Architecture;

public class WhenWithin : Spec
{
    private static readonly SourceProjects _dependencies = new(new()
    {
        ["Entry"] = ["Contract"],
        ["Core"] = ["Contract"],
    });

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
    public void GivenADependencyIsNotAllowedAndAFix_ThenTheFailureSaysWhatToDo()
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => _dependencies.Is().either.Within(_ => []).or.Fix(
                "remove the reference, or allow it if the architecture changed",
                because: "only an architecture decision changes the graph"));
        ex.HasMessage("""
            Expected _dependencies to be within what is allowed but found 2: ["Core -> Contract", "Entry -> Contract"]
            Fix: remove the reference, or allow it if the architecture changed, because only an architecture decision changes the graph
            """);
    }

    [Fact]
    public void GivenADependencyIsNotAllowed_ThenGetException()
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => _dependencies.Is().Within(project => project switch
            {
                "Entry" => ["Contract"],
                _ => [],
            }));
        ex.HasMessage("""Expected _dependencies to be within what is allowed but found 1: ["Core -> Contract"]""");
    }

    [Fact]
    public void GivenAnotherRuleAllowsTheDependency_ThenCompletes()
        => new SourceProjects(new() { ["Entry"] = ["Common", "Contract"], ["Core"] = ["Common"] }).Is().Within(
            p => p switch
            {
                "Entry" => ["Contract"],
                _ => [],
            },
            _ => ["Common"]);

    [Fact]
    public void GivenNoRuleAllowsTheDependency_ThenGetException()
    {
        var dependencies = new SourceProjects(new() { ["Entry"] = ["Common", "Infra"] });
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => dependencies.Is().Within(p => p switch { _ => [] }, _ => ["Common"]));
        ex.HasMessage("""Expected dependencies to be within what is allowed but found 1: ["Entry -> Infra"]""");
    }

    [Fact]
    public void GivenATargetEndingInStar_ThenAllowEveryNameItBegins()
        => new SourceProjects(new() { ["Core"] = ["P:Microsoft.Extensions.Logging", "P:Microsoft.Extensions.Options"] })
            .Is().Within(_ => ["P:Microsoft.*"]);

    [Fact]
    public void GivenATargetEndingInDotStar_ThenNotAllowTheNameBeforeTheDot()
    {
        var dependencies = new SourceProjects(new() { ["Core"] = ["P:Moq"] });
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => dependencies.Is().Within(_ => ["P:Moq.*"]));
        ex.HasMessage("""Expected dependencies to be within what is allowed but found 1: ["Core -> P:Moq"]""");
    }

    [Fact]
    public void GivenMoreDependenciesNotAllowedThanShown_ThenCountThemAll()
    {
        var dependencies = new SourceProjects(new() { ["A"] = ["B", "C", "D", "E", "F", "G"] });
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => dependencies.Is().Within(_ => []));
        ex.Message.Does().EndWith("""but found 6: ["A -> B", "A -> C", "A -> D", "A -> E", "A -> F", ...]""");
    }

    [Fact]
    public void GivenALongName_ThenShowItInFull()
    {
        var dependencies = new SourceProjects(new() { ["Capabilities.Auth"] = ["P:Microsoft.IdentityModel.JsonWebTokens"] });
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => dependencies.Is().Within(_ => []));
        ex.Message.Does().EndWith("""but found 1: ["Capabilities.Auth -> P:Microsoft.IdentityModel.JsonWebTokens"]""");
    }

    /// The running spec project references every project to read the build: no rule need allow it.
    [Fact]
    public void GivenTheRunningSpecProject_ThenLeaveItOut()
        => SourceProjects.Parse(MyHotelBuild.DepsJson, "MyHotel.Spec").Is().Within(p => p switch
        {
            "." => ["Entry", "Infra", "P:Scalar.AspNetCore"],
            "Entry" or "Core" => ["Contract"],
            "Infra" => ["Core"],
            _ => [],
        }, _ => ["P:Microsoft.*"]);

    [Fact]
    public void GivenWithinAndNonRedundant_ThenCompletes()
        => _dependencies.Is().Within(project => project switch
        {
            "Entry" or "Core" => ["Contract"],
            _ => [],
        }).and.NonRedundant();

    [Fact]
    public void ThenSpecifyItAsWithinWhatIsAllowed()
    {
        _dependencies.Is().Within(project => project switch
        {
            "Entry" or "Core" => ["Contract"],
            _ => [],
        });
        Specification.Is("_dependencies is within what is allowed");
    }

    [Fact]
    public void ThenTableWhatEachProjectMayReferenceAndUse()
    {
        new SourceProjects(new()
        {
            ["Host"] = ["Host.Entry", "P:Scalar"],
            ["Host.Entry"] = ["Host.Contract"],
            ["Host.Contract"] = [],
            ["Host.Spec"] = ["Host", "P:xunit"],
        }, spec: "Host.Architecture.Spec").Is().Within(p => p switch
        {
            "." => ["Entry", "P:Scalar"],
            "Entry" => ["Contract"],
            "Spec" => ["."],
            _ => [],
        }, p => p.Segment(^1) == "Spec" ? ["P:*"] : ["P:Microsoft.*"]);
        Lines(Reported().Table!).Is().EqualTo([
            "Project | May reference | May use",
            "Host | Entry | Scalar, Microsoft.*",
            "Contract | - | Microsoft.*",
            "Entry | Contract | Microsoft.*",
            "Host.Spec | Host | *"]);
    }

    [Fact]
    public void ThenDrawNothingAboveTheTable_SinceTheGraphAtTheTopDrawsTheProjects()
    {
        _dependencies.Is().Within(project => project switch
        {
            "Entry" or "Core" => ["Contract"],
            _ => [],
        });
        Reported().Table!.Arrows.Is().Null();
    }

    private static string[] Lines(SpecificationTable table)
        => [string.Join(" | ", table.Headers), .. table.Rows.Select(row => string.Join(" | ", row))];
}
