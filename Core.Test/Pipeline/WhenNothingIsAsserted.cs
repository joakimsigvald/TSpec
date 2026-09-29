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
        + "verify a mock, or state Then().Completes(); a bare Then() asserts nothing";

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
        var other = "other";
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
    public void GivenThatCheckedByContains_ThenThrowSetupFailed()
    {
        var spec = Throwing();
        _ = spec.Then().Throws<InvalidOperationException>().that.Message.Contains("text");
        Xunit.Assert.Throws<SetupFailed>(spec.Dispose).Message.Is(ThatNotAsserted);
    }

    [Fact]
    public void GivenThatCheckedByStartsWith_ThenThrowSetupFailed()
    {
        var spec = Throwing();
        _ = spec.Then().Throws<InvalidOperationException>().that.Message.StartsWith("te");
        Xunit.Assert.Throws<SetupFailed>(spec.Dispose).Message.Is(ThatNotAsserted);
    }

    [Fact]
    public void GivenThatComparedWithEquals_ThenThrowSetupFailed()
    {
        var spec = Throwing();
        _ = spec.Then().Throws<InvalidOperationException>().that.Message == "text";
        Xunit.Assert.Throws<SetupFailed>(spec.Dispose).Message.Is(ThatNotAsserted);
    }

    [Fact]
    public void GivenThatCheckedByXunit_ThenThrowSetupFailed()
    {
        var spec = Throwing();
        Xunit.Assert.Contains("text", spec.Then().Throws<InvalidOperationException>().that.Message);
        Xunit.Assert.Throws<SetupFailed>(spec.Dispose).Message.Is(ThatNotAsserted);
    }

    [Fact]
    public void GivenThatOnlyRead_ThenThrowSetupFailed()
    {
        var spec = Throwing();
        _ = spec.Then().Throws<InvalidOperationException>().that;
        Xunit.Assert.Throws<SetupFailed>(spec.Dispose).Message.Is(ThatNotAsserted);
    }

    [Fact]
    public void GivenAssertionThenALaterDanglingThat_ThenThrowSetupFailed()
    {
        var spec = Throwing();
        spec.Then().Throws<InvalidOperationException>().that.Message.Does().Contain("text");
        _ = spec.Then().Throws<InvalidOperationException>().that.Message.Contains("text");
        Xunit.Assert.Throws<SetupFailed>(spec.Dispose).Message.Is(ThatNotAsserted);
    }

    [Fact]
    public void GivenThatReadInsideAnAssertion_ThenDoNotComplain()
    {
        var spec = Throwing();
        string Message() => spec.Then().Throws<InvalidOperationException>().that.Message;
        new[] { "text" }.Has().All(it => it == Message());
        spec.Dispose();
    }

    public interface IStore { void Save(string text); }

    public class Saver(IStore store)
    {
        public string[] Save() { store.Save("text"); return ["text"]; }
    }

    private sealed class SaverSpec : Spec<Saver, string[]> { }

    [Fact]
    public void GivenThatReadInsideAVerification_ThenDoNotComplain()
    {
        var spec = new SaverSpec();
        spec.When(_ => _.Save());
        Func<string> saved = () => spec.Then().Result.Has().OneItem().that;
        spec.Then<IStore>(_ => _.Save(saved()));
        spec.Dispose();
    }

    [Fact]
    public void GivenThatReadInsideAThrowsCondition_ThenDoNotComplain()
    {
        var spec = Throwing();
        string Message() => spec.Then().Throws<InvalidOperationException>().that.Message;
        spec.Then().Throws<InvalidOperationException>(ex => ex.Message == Message());
        spec.Dispose();
    }

    [Fact]
    public void GivenThatReadInsideAThrowsAssertion_ThenDoNotComplain()
    {
        var spec = Throwing();
        string Message() => spec.Then().Throws<InvalidOperationException>().that.Message;
        spec.Then().Throws<InvalidOperationException>(ex => Xunit.Assert.Equal(Message(), ex.Message));
        spec.Dispose();
    }

    private static MySpec Throwing()
    {
        var spec = new MySpec();
        spec.When(_ => throw new InvalidOperationException("text"));
        return spec;
    }

    [Fact]
    public void GivenElementThatButNoAssertion_ThenThrowSetupFailed()
    {
        var spec = new MySpec();
        _ = spec.When(_ => 1).Then().Result.Is(1).And(new[] { "text" }).Has().OneItem().that.Contains("x");
        Xunit.Assert.Throws<SetupFailed>(spec.Dispose).Message.Is(ThatNotAsserted);
    }

    [Fact]
    public void GivenAssertionOnThat_ThenDoNotComplain()
    {
        var spec = new MySpec();
        spec.When(_ => throw new InvalidOperationException("text"))
            .Then().Throws<InvalidOperationException>().that.Message.Does().Contain("text");
        spec.Dispose();
    }

    private const string ThatNotAsserted =
        "that hands over a subject to be asserted on, but no assertion follows it. "
        + "Assert with Is() or Does(), e.g. that.Message.Does().Contain(\"x\") rather than that.Message.Contains(\"x\")";

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
        var other = "other";
        spec.When(_ => 1).Then(other).Is("other");
        spec.Dispose();
    }

    [Fact]
    public void GivenCompletes_ThenDoNotComplain()
    {
        var spec = new MySpec();
        spec.When(_ => 1).Then().Completes();
        spec.Dispose();
    }

    [Fact]
    public void GivenMockVerification_ThenDoNotComplain()
    {
        var spec = new MyServiceSpec();
        spec.When(_ => _.Get()).Then<IMyService>(wasInvoked: Times.Once);
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

    [Fact]
    public async Task GivenTheTestFailedBeforeItsAssertion_ThenDoNotComplain()
    {
        var spec = new MySpec();
        var other = "other";
        spec.When(_ => 1).Then(other);
        await DisposeAfterAFailure(spec);
    }

    /// xUnit disposes a test class once the test is over, in a context that tells whether it failed;
    /// set in a flow of its own, so this test's context is left as it was.
    private static Task DisposeAfterAFailure(IDisposable spec)
    {
        var test = TestContext.Current;
        return Task.Run(() =>
        {
            TestContext.SetForTest(test.Test!, TestEngineStatus.CleaningUp, test.CancellationToken,
                TestResultState.FromException(0, new Exception("the test failed")));
            spec.Dispose();
        }, test.CancellationToken);
    }
}
