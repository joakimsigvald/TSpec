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
}
