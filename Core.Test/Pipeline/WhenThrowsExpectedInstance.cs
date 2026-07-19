using TSpec.Assert;
using Xunit.Sdk;

namespace TSpec.Test.Pipeline;

public class WhenThrowsExpectedInstance : Spec<MyStateService, int>
{
    private static readonly ArgumentException _thrown = new("boom");

    public WhenThrowsExpectedInstance() => When(_ => Throw());

    private static int Throw() => throw _thrown;

    [Fact]
    public void GivenSameInstance_ThenPasses() => Then().Throws(() => _thrown);

    [Fact]
    public void GivenLookalikeInstance_ThenFailureExplainsIdentityContract()
    {
        var ex = Xunit.Assert.Throws<XunitException>(
            () => Then().Throws(() => new ArgumentException("boom")));
        ex.Message.Is(
            "Expected the exact exception instance, but a different instance with the same type and message was thrown");
    }

    [Fact]
    public void GivenDifferentMessage_ThenFailureShowsBothExceptions()
    {
        var ex = Xunit.Assert.Throws<XunitException>(
            () => Then().Throws(() => new ArgumentException("other")));
        ex.Message.Does().StartWith("Expected the exception System.ArgumentException: other");
    }
}
