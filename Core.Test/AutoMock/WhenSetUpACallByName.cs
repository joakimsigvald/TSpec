using TSpec.Assert;

namespace TSpec.Test.AutoMock;

public class WhenSetUpACallByName : Spec<CatalogService, string>
{
    public WhenSetUpACallByName() => When(_ => _.CountOf(A<string>()));

    [Fact]
    public void ThenItAnswersWhateverTheArguments()
    {
        Given<ICatalog>().That<int>(nameof(ICatalog.Count)).Returns(() => 7);
        Then().Result.Is("7");
        Specification.Is(
            """
            Given ICatalog.Count returns 7
            When CountOf(a string)
            Then Result is "7"
            """);
    }

    [Fact]
    public void GivenASpecificSetupBefore_ThenThatWins()
    {
        Given<ICatalog>().That(_ => _.Count(The<string>())).Returns(() => 1)
            .AndThat<int>(nameof(ICatalog.Count)).Returns(() => 7);
        Then().Result.Is("1");
        Specification.Is(
            """
            Given ICatalog.Count(the string) returns 1
              and Count returns 7
            When CountOf(a string)
            Then Result is "1"
            """);
    }

    [Fact]
    public void GivenASpecificSetupAfter_ThenThatWins()
        => Given<ICatalog>().That<int>(nameof(ICatalog.Count)).Returns(() => 7)
            .AndThat(_ => _.Count(The<string>())).Returns(() => 1)
            .Then().Result.Is("1");

    [Fact]
    public void GivenASetupOnTheMockItself_ThenThatWins()
        => Given(The<ICatalog>).That(_ => _.Count(The<string>())).Returns(() => 1)
            .And<ICatalog>().That<int>(nameof(ICatalog.Count)).Returns(() => 7)
            .Using(The<ICatalog>)
            .Then().Result.Is("1");

    [Fact]
    public void GivenASpecificSetupOfOtherArguments_ThenTheNameAnswers()
        => Given<ICatalog>().That(_ => _.Count("another shelf")).Returns(() => 1)
            .AndThat<int>(nameof(ICatalog.Count)).Returns(() => 7)
            .Then().Result.Is("7");

    [Fact]
    public void GivenTwoSetupsByName_ThenTheLatestWins()
        => Given<ICatalog>().That<int>(nameof(ICatalog.Count)).Returns(() => 7)
            .AndThat<int>(nameof(ICatalog.Count)).Returns(() => 8)
            .Then().Result.Is("8");

    [Fact]
    public void GivenAServiceWideDefault_ThenTheNameWins()
        => Given<ICatalog>().Returns(() => 5)
            .AndThat<int>(nameof(ICatalog.Count)).Returns(() => 7)
            .Then().Result.Is("7");

    [Fact]
    public void GivenAfterASetupOnOneMock_ThenSetupFailsNamingTheType()
        => Xunit.Assert.Throws<SetupFailed>(() =>
                Given(The<ICatalog>).That(_ => _.Count(The<string>())).Returns(() => 1)
                    .AndThat<int>(nameof(ICatalog.Count)))
            .Message.Is(
                "A setup by name applies to every ICatalog, not to the ICatalog alone. "
                + "Set it up with Given<ICatalog>().That(…)");
}
