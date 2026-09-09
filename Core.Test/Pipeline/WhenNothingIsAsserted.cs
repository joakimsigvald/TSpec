using Moq;
using TSpec.Assert;

namespace TSpec.Test.Pipeline;

/// <summary>
/// A test is green by what it asserts, so one that asserts nothing is green for no reason. The
/// pipeline checks at teardown that a test which provided a When also claimed something, and that a
/// subject handed to Then or And was asserted on — the two ways a test silently verifies nothing.
/// </summary>
public class WhenNothingIsAsserted
{
    private sealed class MySpec : Spec<int>
    {
        internal int AnInt() => An<int>();
    }

    private sealed class MyServiceSpec : Spec<MyService, int> { }

    public interface IMyService { int Get(); }

    public class MyService(IMyService service)
    {
        public int Get() => service.Get();
    }

    private const string NothingAsserted =
        "Nothing was asserted. A test that provides When must assert on the result or a subject, "
        + "verify a mock, or state Then().DoesNotThrow(); a bare Then() asserts nothing";

    [Fact]
    public void GivenNoWhen_ThenDoNotComplain()
    {
        var spec = new MySpec();
        spec.Dispose();
    }

    [Fact]
    public void GivenWhenButNoThen_ThenThrowSetupFailed()
    {
        var spec = new MySpec();
        spec.When(_ => 1);
        Xunit.Assert.Throws<SetupFailed>(spec.Dispose).Message.Is(NothingAsserted);
    }

    [Fact]
    public void GivenBareThen_ThenThrowSetupFailed()
    {
        var spec = new MySpec();
        spec.When(_ => 1).Then();
        Xunit.Assert.Throws<SetupFailed>(spec.Dispose).Message.Is(NothingAsserted);
    }

    [Fact]
    public void GivenThenWithSubjectButNoAssertion_ThenThrowSetupFailed()
    {
        var spec = new MySpec();
        var other = 2;
        spec.When(_ => 1).Then(other);
        Xunit.Assert.Throws<SetupFailed>(spec.Dispose).Message.Is(
            "Then(other) hands over a subject to be asserted on, but no assertion follows it");
    }

    [Fact]
    public void GivenAndWithSubjectButNoAssertion_ThenThrowSetupFailed()
    {
        var spec = new MySpec();
        var other = 2;
        spec.When(_ => 1).Then().Result.Is(1).And(other);
        Xunit.Assert.Throws<SetupFailed>(spec.Dispose).Message.Is(
            "And(other) hands over a subject to be asserted on, but no assertion follows it");
    }

    [Fact]
    public void GivenAssertionOnResult_ThenDoNotComplain()
    {
        var spec = new MySpec();
        spec.When(_ => 1).Then().Result.Is(1);
        spec.Dispose();
    }

    [Fact]
    public void GivenAssertionOnSubject_ThenDoNotComplain()
    {
        var spec = new MySpec();
        var other = 2;
        spec.When(_ => 1).Then(other).Is(2);
        spec.Dispose();
    }

    [Fact]
    public void GivenDoesNotThrow_ThenDoNotComplain()
    {
        var spec = new MySpec();
        spec.When(_ => 1).Then().DoesNotThrow();
        spec.Dispose();
    }

    [Fact]
    public void GivenMockVerification_ThenDoNotComplain()
    {
        var spec = new MyServiceSpec();
        spec.When(_ => _.Get()).Then<IMyService>(wasInvoked: Times.Once());
        spec.Dispose();
    }

    [Fact]
    public void GivenAssertionOnGeneratedDataOnly_ThenDoNotComplain()
    {
        var spec = new MySpec();
        spec.When(_ => 1);
        spec.AnInt().Is(spec.AnInt());
        spec.Dispose();
    }

    [Fact]
    public void GivenSetupFailedWasThrown_ThenDoNotComplain()
    {
        var spec = new MySpec();
        Xunit.Assert.Throws<SetupFailed>(() => spec.When(_ => 1).When(_ => 2));
        spec.Dispose();
    }
}
