using TSpec.Assert;
using static TSpec.Times;

namespace TSpec.Test.AutoMock;

public class AlwaysPasses : IRule
{
    public bool Passes() => true;
    public int Weight() => 1;
}

public class WhenOneMockOfATypeIsSetUp : Spec<RulePair, string>
{
    public WhenOneMockOfATypeIsSetUp()
        => When(_ => _.Passing()).Using(new RulePair(A<IRule>(), ASecond<IRule>()));

    [Fact]
    public void ThenOnlyThatMockAnswersSo()
    {
        Given<IRule>().That(_ => _.Passes()).Returns(() => true)
            .And(TheSecond<IRule>).That(_ => _.Passes()).Returns(() => false)
            .Then().Result.Is("True,False");
        Specification.Is(
            """
            Using new RulePair(a IRule, a second IRule)
            Given IRule.Passes() returns true
              and the second IRule.Passes() returns false
            When Passing()
            Then Result is "True,False"
            """);
    }

    [Fact]
    public void GivenTheTypeIsSetUpAfter_ThenTheMocksOwnSetupStillWins()
        => Given(TheSecond<IRule>).That(_ => _.Passes()).Returns(() => false)
            .And<IRule>().That(_ => _.Passes()).Returns(() => true)
            .Then().Result.Is("True,False");

    [Fact]
    public void GivenAnotherCallOnIt_ThenThatStaysOnTheSameMock()
        => Given<IRule>().That(_ => _.Passes()).Returns(() => true)
            .And(TheSecond<IRule>).That(_ => _.Weight()).Returns(() => 7)
            .AndThat(_ => _.Passes()).Returns(() => false)
            .Then().Result.Is("True,False");

    [Fact]
    public void GivenADefaultAfterIt_ThenSetupFailsNamingTheType()
        => Xunit.Assert.Throws<SetupFailed>(() =>
                Given(TheSecond<IRule>).That(_ => _.Passes()).Returns(() => false).AndReturnsDefault(() => 5))
            .Message.Is(
                "A default is set up for every IRule, not for the second IRule alone. "
                + "Set it up with Given<IRule>().Returns(…)");

    [Fact]
    public void GivenAnotherMockNext_ThenEachAnswersAsItsOwnSetupSays()
        => Given(The<IRule>).That(_ => _.Passes()).Returns(() => false)
            .And(TheSecond<IRule>).That(_ => _.Passes()).Returns(() => true)
            .Then().Result.Is("False,True");
}

public class WhenATaggedMockIsSetUp : Spec<RulePair, string>
{
    private static readonly Tag<IRule> _primary = new();

    [Fact]
    public void ThenOnlyThatMockAnswersSo()
    {
        When(_ => _.Passing()).Using(new RulePair(The(_primary), A<IRule>()))
            .Given<IRule>().That(_ => _.Passes()).Returns(() => true)
            .And(_primary).That(_ => _.Passes()).Returns(() => false)
            .Then().Result.Is("False,True");
        Specification.Is(
            """
            Using new RulePair(the Primary, a IRule)
            Given IRule.Passes() returns true
              and the Primary.Passes() returns false
            When Passing()
            Then Result is "False,True"
            """);
    }
}

public class WhenAMentionThatIsNoMockIsSetUp : Spec<RulePair, string>
{
    [Fact]
    public void ThenSetupFailsSayingWhereToSetItUp()
        => Xunit.Assert.Throws<SetupFailed>(() =>
                When(_ => _.Passing()).Using<IRule>(new AlwaysPasses())
                    .Given(The<IRule>).That(_ => _.Passes()).Returns(() => false)
                    .Then())
            .Message.Is("The IRule is a real AlwaysPasses, which TSpec does not mock. Set it up where it was made");
}

public class WhenACallAnswersWithAMention : Spec<ParentService, string>
{
    public WhenACallAnswersWithAMention()
        => When(_ => _.GetFromChildOf(1, 2))
            .Given<IParent>().That(_ => _.GetChild(1)).Returns(TheSecond<IChild>)
            .And(TheSecond<IChild>).That(_ => _.Get(2)).Returns(() => "x");

    [Fact]
    public void ThenTheMentionsSetupAnswersThroughTheCall() => Then().Result.Is("x");

    [Fact]
    public void ThenAVerificationThroughTheCallCountsTheMentionsCalls()
        => Then<IParent>(_ => _.GetChild(1).Get(2), Once);
}
