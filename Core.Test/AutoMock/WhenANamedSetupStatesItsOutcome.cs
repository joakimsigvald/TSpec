using TSpec.Assert;

namespace TSpec.Test.AutoMock;

public class WhenANamedSetupStatesItsOutcome : Spec<CatalogService, string>
{
    private static readonly Tag<int> _count = new();

    [Fact]
    public void GivenThrows_ThenTheCallThrows()
        => When(_ => _.CountOf(A<string>()))
            .Given<ICatalog>().That<int>(nameof(ICatalog.Count)).Throws<InvalidOperationException>()
            .Then().Throws<InvalidOperationException>();

    [Fact]
    public void GivenATap_ThenItSeesTheArguments()
    {
        var seen = string.Empty;
        When(_ => _.CountOf(A<string>()))
            .Given<ICatalog>().That<int>(nameof(ICatalog.Count)).Tap<string>(shelf => seen = shelf).Returns(() => 7)
            .Then().Result.Is("7");
        seen.Is(The<string>());
    }

    [Fact]
    public void GivenATapReadingNoArguments_ThenItSeesACallToAnyOverload()
    {
        var calls = 0;
        When(_ => _.FindByCode(A<string>()))
            .Given<ICatalog>().That<string>(nameof(ICatalog.Find)).Tap(() => calls++).Returns(() => "found")
            .Then().Result.Is("found");
        calls.Is(1);
    }

    [Fact]
    public void GivenReturnsDefault_ThenTheCallAnswersTheDefault()
        => When(_ => _.CountOf(A<string>()))
            .Given<ICatalog>().That<int>(nameof(ICatalog.Count)).ReturnsDefault()
            .Then().Result.Is("0");

    [Fact]
    public void GivenATag_ThenTheCallAnswersItsValue()
        => When(_ => _.CountOf(A<string>()))
            .Given<ICatalog>().That<int>(nameof(ICatalog.Count)).Returns(_count)
            .Then().Result.Is(The(_count).ToString());

    [Fact]
    public void GivenAnAnswerFromTheArguments_ThenItIsComputedFromThem()
        => When(_ => _.CountOf("shelf"))
            .Given<ICatalog>().That<int>(nameof(ICatalog.Count)).Returns((string shelf) => shelf.Length)
            .Then().Result.Is("5");

    [Fact]
    public void GivenASequence_ThenEachCallAnswersTheNextStep()
        => When(_ => _.CountTwice(A<string>(), ASecond<string>()))
            .Given<ICatalog>().That<int>(nameof(ICatalog.Count)).First().Returns(() => 1).AndNext().Returns(() => 2)
            .Then().Result.Is("1,2");
}
