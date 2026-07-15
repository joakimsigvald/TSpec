using TSpec.Assert;

namespace TSpec.Test.Characterization;

public class WhenSUTHasUriArg_FallbackProducesTSpecHost : Spec<UriHolder, string>
{
    public WhenSUTHasUriArg_FallbackProducesTSpecHost()
        => When(_ => _.Get().Host);

    [Fact]
    public void Then_FallbackUriIsTSpecDev()
    {
        Then().Result.Is("xspec.dev");
        Specification.Is(
            """
            When _.Get().Host
            Then Result is "xspec.dev"
            """);
    }
}

public class WhenSUTHasUriArg_FallbackProducesHttpsScheme : Spec<UriHolder, string>
{
    public WhenSUTHasUriArg_FallbackProducesHttpsScheme()
        => When(_ => _.Get().Scheme);

    [Fact]
    public void Then_FallbackUriIsHttps()
    {
        Then().Result.Is("https");
        Specification.Is(
            """
            When _.Get().Scheme
            Then Result is "https"
            """);
    }
}

public class WhenSUTIsString_EarlyReturnsAndDataGeneratorProducesString : Spec<string>
{
    public WhenSUTIsString_EarlyReturnsAndDataGeneratorProducesString()
        => When(_ => _);

    [Fact]
    public void Then_ResultIsNonEmptyString()
    {
        Then().Result.Is().Not(null!).And(Result).Length.Is().GreaterThan(0);
        Specification.Is(
            """
            When _
            Then Result is not null
              and Result.Length is greater than 0
            """);
    }
}
