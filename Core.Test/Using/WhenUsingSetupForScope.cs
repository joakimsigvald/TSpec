using TSpec.Assert;

namespace TSpec.Test.Using;

/// <summary>
/// A setup on a type carries a scope like every other Using arrangement: it applies where the
/// scope says values of that type come from, and nowhere else.
/// </summary>
public class WhenUsingSetupForSubject : Spec<ConfigReader, string>
{
    [Fact]
    public void ThenAppliesToTheSubjectGraph()
        => Using<Config>(_ => _.Name = "scoped", For.Subject)
        .When(_ => _.Read())
        .Then().Result.Is("scoped");

    [Fact]
    public void ThenRendersTheScope()
    {
        Using<Config>(_ => _.Name = "scoped", For.Subject)
            .When(_ => _.Read())
            .Then().Result.Is("scoped");
        Specification.Is(
            """
            Using Config with Name = "scoped" for Subject
            When Read()
            Then Result is "scoped"
            """);
    }
}

public class WhenUsingSetupForSubject_AndInputIsRequested : Spec<ConfigReader, string>
{
    [Fact]
    public void ThenDoesNotApplyToInput()
        => Using<Config>(_ => _.Name = "scoped", For.Subject)
        .When(_ => A<Config>().Name)
        .Then().Result.Is().Not("scoped");
}

public class WhenUsingSetupForInput : Spec<ConfigReader, string>
{
    [Fact]
    public void ThenAppliesToInput()
        => Using<Config>(_ => _.Name = "scoped", For.Input)
        .When(_ => A<Config>().Name)
        .Then().Result.Is("scoped");

    [Fact]
    public void ThenDoesNotApplyToTheSubjectGraph()
        => Using<Config>(_ => _.Name = "scoped", For.Input)
        .When(_ => _.Read())
        .Then().Result.Is().Not("scoped");
}

public class WhenUsingSetupForAll : Spec<ConfigReader, string>
{
    [Fact]
    public void ThenAppliesToBoth()
        => Using<Config>(_ => _.Name = "scoped")
        .When(_ => _.Read() + A<Config>().Name)
        .Then().Result.Is("scopedscoped");
}

public class WhenUsingSetupScopeNone : Spec<int>
{
    private const string Message =
        "For.None is not a valid scope: it would apply the arrangement to neither Input nor Subject. "
        + "Use For.Input, For.Subject or For.All";

    [Fact]
    public void GivenSetup_ThenSetupFailedExplainsScope()
        => Xunit.Assert.Throws<SetupFailed>(
            () => Using<Config>(_ => _.Name = "x", For.None)).Message.Is(Message);

    [Fact]
    public void GivenTransform_ThenSetupFailedExplainsScope()
        => Xunit.Assert.Throws<SetupFailed>(
            () => Using<Config>(_ => _ with { Name = "x" }, For.None)).Message.Is(Message);
}

public record Config
{
    public string Name { get; set; } = null!;
}

public class ConfigReader(Config config)
{
    public string Read() => config.Name;
}
