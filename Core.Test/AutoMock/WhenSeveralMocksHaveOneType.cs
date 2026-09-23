using TSpec.Assert;
using static TSpec.Times;

namespace TSpec.Test.AutoMock;

public interface IRule
{
    bool Passes();
    int Weight();
}

public class RulePair(IRule first, IRule second)
{
    public IRule First => first;
    public bool AreOne() => ReferenceEquals(first, second);
    public bool BothPass() => first.Passes() && second.Passes();
    public string Passing() => $"{first.Passes()},{second.Passes()}";
    public bool FirstPasses() => first.Passes();
    public string Weights() => $"{first.Weight()},{second.Weight()}";
}

public class WhenASubjectTakesTwoMocksOfOneType : Spec<RulePair, bool>
{
    [Fact]
    public void ThenEachIsAMockOfItsOwn() => When(_ => _.AreOne()).Then().Result.Is(false);

    [Fact]
    public void ThenASetupOnTheTypeAnswersOnBoth()
        => When(_ => _.BothPass()).Given<IRule>().That(_ => _.Passes()).Returns(() => true).Then().Result.Is(true);

    [Fact]
    public void ThenAVerificationOnTheTypeCountsTheCallsOnBoth()
        => When(_ => _.BothPass()).Given<IRule>().That(_ => _.Passes()).Returns(() => true)
            .Then<IRule>(_ => _.Passes(), Exactly(2));
}

public class WhenAMockOfTheSubjectsTypeIsMentioned : Spec<RulePair, IRule>
{
    [Fact]
    public void ThenTheSubjectHoldsAnother() => When(_ => _.First).Then().Result.Is().Not(The<IRule>());
}

public class WhenMocksOfOneTypeAreMentioned : Spec<RulePair, bool>
{
    [Fact]
    public void ThenDistinctMentionsAreDistinctMocks() => ReferenceEquals(A<IRule>(), ASecond<IRule>()).Is(false);

    [Fact]
    public void ThenTwoHoldsTwoMocks() => Two<IRule>().Distinct().Count().Is(2);
}

public class ParentPair(IParent first, IParent second)
{
    public bool ShareAChild() => ReferenceEquals(first.GetChild(1), second.GetChild(1));
    public string GetFromBoth() => $"{first.GetChild(1).Get(2)},{second.GetChild(1).Get(2)}";
}

public class WhenAChainIsSetUpOnATypeOfSeveralMocks : Spec<ParentPair, object>
{
    public WhenAChainIsSetUpOnATypeOfSeveralMocks()
        => Given<IParent>().That(_ => _.GetChild(1).Get(2)).Returns(() => "x");

    [Fact]
    public void ThenEachMockReachesAChildOfItsOwn()
        => When(_ => _.ShareAChild()).Then().Result.Is(false);

    [Fact]
    public void ThenTheChainAnswersThroughEach() => When(_ => _.GetFromBoth()).Then().Result.Is("x,x");
}
