using TSpec.Assert;

namespace TSpec.Test.Pipeline;

/// <summary>
/// C# infers the subject's type from whatever is passed, so a lambda binds happily and hands over a
/// delegate. Running the pipeline changes nothing about a lambda, and a delegate has nothing worth
/// asserting on, so it is refused rather than silently accepted.
/// </summary>
public class WhenALambdaIsHandedOverAsSubject
{
    private sealed class MySpec : Spec<int> { }

    [Fact]
    public void GivenALambdaAsTheThenSubject_ThenThrowSetupFailed()
    {
        using var spec = new MySpec();
        spec.When(() => 1);
        Xunit.Assert.Throws<SetupFailed>(() => spec.Then(() => 42)).Message.Is(
            "Then(() => 42) hands over a lambda, which running the pipeline does not affect and which "
            + "has nothing to assert on. Hand over the value it would produce instead");
    }

    [Fact]
    public void GivenALambdaAsTheAndSubject_ThenThrowSetupFailed()
    {
        using var spec = new MySpec();
        spec.When(() => 1);
        Xunit.Assert.Throws<SetupFailed>(() => spec.Then().Result.Is(1).And(() => 42)).Message.Is(
            "And(() => 42) hands over a lambda, which running the pipeline does not affect and which "
            + "has nothing to assert on. Hand over the value it would produce instead");
    }

    [Fact]
    public void GivenAMethodGroupAsTheThenSubject_ThenThrowSetupFailed()
    {
        using var spec = new MySpec();
        spec.When(() => 1);
        Xunit.Assert.Throws<SetupFailed>(() => spec.Then(Answer)).Message.Is(
            "Then(Answer) hands over a lambda, which running the pipeline does not affect and which "
            + "has nothing to assert on. Hand over the value it would produce instead");
    }

    [Fact]
    public void GivenAValueAsTheThenSubject_ThenDoNotComplain()
    {
        using var spec = new MySpec();
        spec.When(() => 1);
        spec.Then(Answer()).Is(42);
    }

    private static int Answer() => 42;
}
