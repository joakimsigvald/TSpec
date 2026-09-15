using TSpec.Assert;

namespace TSpec.Test.AutoMock;

public class TwoCallService(IMyValueIntRepo repo)
{
    public string GetTwice(int first, int second) => repo.Get(first) + repo.Get(second);
}

/// <summary>
/// A sequence says what each call answers; a tap says what each call was asked. The two could not
/// be stated together before, so reading the calls back meant holding the script in a tag behind
/// one from-arguments Returns that also recorded them.
/// </summary>
public class WhenTapASequence : Spec<TwoCallService, string>
{
    private readonly List<int> _asked = [];
    private readonly List<string> _seen = [];

    public WhenTapASequence() => When(_ => _.GetTwice(AFirst<int>(), ASecond<int>()));

    [Fact]
    public void ThenEachStepTapsItsOwnCall()
    {
        Given<IMyValueIntRepo>().That(_ => _.Get(Any<int>()))
            .First().Tap<int>(_asked.Add).Returns(() => "a")
            .AndNext().Tap<int>(_asked.Add).Returns(() => "b");
        Then().Result.Is("ab");
        _asked.Is().EqualTo([AFirst<int>(), ASecond<int>()]);
    }

    /// A tap before First reads as a tap outside a sequence does: it taps every call.
    [Fact]
    public void GivenATapBeforeFirst_ThenItTapsEveryCall()
    {
        Given<IMyValueIntRepo>().That(_ => _.Get(Any<int>()))
            .Tap<int>(_asked.Add)
            .First().Returns(() => "a")
            .AndNext().Returns(() => "b");
        Then().Result.Is("ab");
        _asked.Is().EqualTo([AFirst<int>(), ASecond<int>()]);
    }

    [Fact]
    public void GivenATapBeforeFirst_ThenItTapsACallPastTheLastStepToo()
    {
        Given<IMyValueIntRepo>().That(_ => _.Get(Any<int>()))
            .Tap<int>(_asked.Add)
            .First().Returns(() => "a");
        Then().Result.Is("a");
        _asked.Is().EqualTo([AFirst<int>(), ASecond<int>()]);
    }

    [Fact]
    public void GivenATapBeforeFirstAndOneOnAStep_ThenTheTapBeforeFirstRunsFirst()
    {
        Given<IMyValueIntRepo>().That(_ => _.Get(Any<int>()))
            .Tap(() => _seen.Add("every"))
            .First().Tap(() => _seen.Add("first")).Returns(() => "a")
            .AndNext().Returns(() => "b");
        Then().Result.Is("ab");
        _seen.Is().EqualTo(["every", "first", "every"]);
    }

    [Fact]
    public void GivenAnyArgument_ThenTheSpecificationReadsItAsInASingleSetup()
    {
        Given<IMyValueIntRepo>().That(_ => _.Get(Any<int>()))
            .First().Returns(() => "a")
            .AndNext().Returns(() => "b");
        Then().Result.Is("ab");
        Specification.Is(
            """
            Given IMyValueIntRepo.Get(any int) first returns "a"
              and next returns "b"
            When GetTwice(a first int, a second int)
            Then Result is "ab"
            """);
    }

    [Fact]
    public void GivenATapBeforeFirst_ThenTheSpecificationStatesItBeforeFirst()
    {
        Given<IMyValueIntRepo>().That(_ => _.Get(Any<int>()))
            .Tap<int>(_asked.Add)
            .First().Returns(() => "a")
            .AndNext().Returns(() => "b");
        Then().Result.Is("ab");
        Specification.Is(
            """
            Given IMyValueIntRepo.Get(any int) tap(_asked.Add) first returns "a"
              and next returns "b"
            When GetTwice(a first int, a second int)
            Then Result is "ab"
            """);
    }
}
