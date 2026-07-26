using TSpec.Assert;

namespace TSpec.Test.Using;

/// <summary>
/// For.None is the empty scope — the zero value of the flags enum. It is never meaningful as an
/// argument, and every arrangement that takes a scope rejects it the same way.
/// </summary>
public class WhenUsingScopeNone : Spec<int>
{
    private static readonly Tag<int> _tag = new(nameof(_tag));

    private const string _message =
        "For.None is not a valid scope: it would apply the arrangement to neither Input nor Subject. "
        + "Use For.Input, For.Subject or For.All";

    [Fact]
    public void GivenValue_ThenSetupFailedExplainsScope()
        => AssertRejected(() => Using(42, For.None));

    [Fact]
    public void GivenFactory_ThenSetupFailedExplainsScope()
        => AssertRejected(() => Using(() => 42, For.None));

    [Fact]
    public void GivenTag_ThenSetupFailedExplainsScope()
        => AssertRejected(() => Using(_tag, For.None));

    [Fact]
    public void GivenConversion_ThenSetupFailedExplainsScope()
        => AssertRejected(() => Using<int>(For.None).From<byte>());

    private static void AssertRejected(Action arrange)
        => Xunit.Assert.Throws<SetupFailed>(arrange).Message.Is(_message);
}
