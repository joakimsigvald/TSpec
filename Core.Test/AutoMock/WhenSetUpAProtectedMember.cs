using TSpec.Assert;

namespace TSpec.Test.AutoMock;

/// <summary>
/// A protected member is named rather than written as a lambda, because a lambda cannot name it.
/// That it is protected is a fact about the mock, not about the behaviour, so the specification
/// says what it would say for any other call.
/// </summary>
public class WhenSetUpAProtectedMember : Spec<DispatchService, string>
{
    public WhenSetUpAProtectedMember() => When(_ => _.Send(A<string>()));

    [Fact]
    public void ThenItAnswers()
    {
        Given<Dispatcher>().ThatProtected<string>("SendAsync").Returns(() => "answered");
        Then().Result.Is("answered");
        Specification.Is(
            """
            Given Dispatcher.SendAsync returns "answered"
            When Send(a string)
            Then Result is "answered"
            """);
    }

    [Fact]
    public void ThenItCanBeTapped()
    {
        var seen = string.Empty;
        Given<Dispatcher>().ThatProtected<string>("SendAsync")
            .Tap<string, int>((request, attempt) => seen = $"{request}/{attempt}")
            .Returns(() => "answered");
        Then().Result.Is("answered");
        seen.Is($"{The<string>()}/1");
    }
}

/// A protected member answering with nothing is named the same way, and tapped the same way.
public class WhenSetUpAProtectedVoidMember : Spec<DispatchService>
{
    private string _noted = string.Empty;

    public WhenSetUpAProtectedVoidMember() => When(_ => _.Note(A<string>()));

    [Fact]
    public void ThenItCanBeTapped()
    {
        Given<Dispatcher>().ThatProtected("Record").Tap<string>(note => _noted = note).Returns();
        Then().Completes();
        _noted.Is(The<string>());
    }

    /// The default of nothing is nothing — this used to fail on any void call, named or not.
    [Fact]
    public void ThenItTakesReturnsDefault()
        => Given<Dispatcher>().ThatProtected("Record").ReturnsDefault().Then().Completes();
}
