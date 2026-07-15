using TSpec.Assert;

namespace TSpec.Test.Assert.AssertionExtensions;

public class WhenAnd : Spec<string>
{
    public WhenAnd() => When(_ => _);

    [Fact]
    public void GivenTrainwreck_ThenThrowSetupFailed()
        => Xunit.Assert.Throws<SetupFailed>(
            () => Then().Result.Is().Not(null!).And(Result.Length));

    [Fact]
    public void GivenTrainwreckAfterTestResult_ThenThrowSetupFailed()
        => Xunit.Assert.Throws<SetupFailed>(
            () => Then().DoesNotThrow().And(Result.Length));

    [Fact]
    public void GivenTrainwreckInThenSubject_ThenThrowSetupFailed()
        => Xunit.Assert.Throws<SetupFailed>(
            () => Then(Result.Length));
}
