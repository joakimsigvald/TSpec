using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Using;

public class WhenUsingFromForSubjectAndInputSeparately : Spec<MyService, int>
{
    public WhenUsingFromForSubjectAndInputSeparately() => When(_ => _.GetNextId());

    [Fact]
    public void ThenEachScopeUsesItsOwnValueSet()
    {
        Using<int>(For.Subject).From<int>().StartingAt(100);
        Using<int>(For.Input).From<int>().StartingAt(5).Then().Result.Is(100);
        A<int>().Is(5);
    }
}

public class WhenUsingFromWithOverlappingScopes : Spec<int>
{
    [Fact]
    public void GivenSameScopeTwice_ThenThrowSetupFailed()
    {
        Using<int>(For.Input).From<int>().StartingAt(1);
        Xunit.Assert.Throws<SetupFailed>(() => Using<int>(For.Input).From<byte>());
    }

    [Fact]
    public void GivenAllOverlapsSubject_ThenThrowSetupFailed()
    {
        Using<int>(For.Subject).From<int>().StartingAt(1);
        Xunit.Assert.Throws<SetupFailed>(() => Using<int>().From<byte>());
    }
}
