using TSpec.Assert;

namespace TSpec.Test.AutoMock;

/// A default after a call setup on the same mock reads "otherwise": it answers the calls that one does not.
public class WhenADefaultFollowsACallSetup : Spec<RulePair, string>
{
    public WhenADefaultFollowsACallSetup() => When(_ => _.Weights());

    [Fact]
    public void ThenItAnswersEveryMockAndReadsAsOtherwise()
    {
        Given<IRule>().That(_ => _.Passes()).Returns(() => true).AndReturnsDefault(() => 5)
            .Then().Result.Is("5,5");
        Specification.Is(
            """
            Given IRule.Passes() returns true
              and otherwise returns 5
            When Weights()
            Then Result is "5,5"
            """);
    }

    [Fact]
    public void GivenTheTypeIsNamedAgain_ThenItReadsAsOtherwiseToo()
    {
        Given<IRule>().That(_ => _.Passes()).Returns(() => true).And<IRule>().Returns(() => 5)
            .Then().Result.Is("5,5");
        Specification.Is(
            """
            Given IRule.Passes() returns true
              and otherwise returns 5
            When Weights()
            Then Result is "5,5"
            """);
    }

    [Fact]
    public void GivenTheDefaultComesFirst_ThenItReadsAsTheTypesAnswer()
    {
        Given<IRule>().Returns(() => 5).And<IRule>().That(_ => _.Passes()).Returns(() => true)
            .Then().Result.Is("5,5");
        Specification.Is(
            """
            Given IRule returns 5
              and Passes() returns true
            When Weights()
            Then Result is "5,5"
            """);
    }
}
