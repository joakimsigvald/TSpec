using TSpec.Assert;

namespace TSpec.Test.AutoMock;

public class WhenANameReachesAMemberAnsweringNothing : Spec<CatalogService>
{
    [Fact]
    public void GivenAVoidMember_ThenItIsReached()
    {
        var noted = string.Empty;
        When(_ => _.Note(A<string>()))
            .Given<ICatalog>().That(nameof(ICatalog.Note)).Tap<string>(entry => noted = entry).Returns()
            .Then().Completes();
        noted.Is(The<string>());
    }

    [Fact]
    public void GivenATaskMember_ThenItIsReached()
    {
        When(_ => _.Save(A<string>()))
            .Given<ICatalog>().That(nameof(ICatalog.SaveAsync)).Throws<InvalidOperationException>()
            .Then().Throws<InvalidOperationException>();
        Specification.Is(
            """
            Given ICatalog.SaveAsync throws InvalidOperationException
            When Save(a string)
            Then throws InvalidOperationException
            """);
    }

    [Fact]
    public void GivenAValueTaskMember_ThenItIsReached()
        => When(_ => _.Flush())
            .Given<ICatalog>().That(nameof(ICatalog.FlushAsync)).Throws<InvalidOperationException>()
            .Then().Throws<InvalidOperationException>();

    [Fact]
    public void GivenAfterAnotherSetup_ThenItIsReached()
        => When(_ => _.Save(A<string>()))
            .Given<ICatalog>().That<int>(nameof(ICatalog.Count)).Returns(() => 7)
            .AndThat(nameof(ICatalog.SaveAsync)).Throws<InvalidOperationException>()
            .Then().Throws<InvalidOperationException>();

    [Fact]
    public void GivenAfterASetupOnOneMock_ThenSetupFailsNamingTheType()
        => Xunit.Assert.Throws<SetupFailed>(() =>
                Given(The<ICatalog>).That(_ => _.Count(Any<string>())).Returns(() => 1)
                    .AndThat(nameof(ICatalog.SaveAsync)))
            .Message.Is(
                "A setup by name applies to every ICatalog, not to the ICatalog alone. "
                + "Set it up with Given<ICatalog>().That(…)");
}
