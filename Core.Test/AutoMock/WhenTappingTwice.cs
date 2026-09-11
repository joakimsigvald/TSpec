using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.AutoMock;

/// <summary>
/// Two taps on one call are two observations of it, not a correction of the first: both run, in the
/// order they were written, and the specification states both. Moq's own Callback keeps only the
/// last, so composing them is TSpec's to do.
/// </summary>
public class WhenTappingTwice : Spec<MyValueIntService, string>
{
    private readonly List<string> _seen = [];

    public WhenTappingTwice() => When(_ => _.GetValue(A<MyValueInt>()));

    [Fact]
    public void ThenBothRunInDeclarationOrder()
        => Given<IMyValueIntRepo>().That(_ => _.Get(Any<int>()))
            .Tap(() => _seen.Add("first"))
            .Tap(() => _seen.Add("second"))
            .Returns(() => "x")
        .Then().Result.Is("x").And(_seen).Is().EqualTo(["first", "second"]);

    [Fact]
    public void ThenTheSpecificationStatesBoth()
    {
        Given<IMyValueIntRepo>().That(_ => _.Get(Any<int>()))
            .Tap(() => _seen.Add("first"))
            .Tap(() => _seen.Add("second"))
            .Returns(() => "x");
        Then().Result.Is("x");
        Specification.Is(
            """
            Given IMyValueIntRepo.Get(any int) tap(_seen.Add("first"))
                  tap(_seen.Add("second")) returns "x"
            When GetValue(a MyValueInt)
            Then Result is "x"
            """);
    }
}

/// A step of a sequence composes its taps the same way, and they stay that step's own.
public class WhenTappingASequenceStepTwice : Spec<TwoGetService, string>
{
    private readonly List<string> _seen = [];

    public WhenTappingASequenceStepTwice() => When(_ => _.GetTwice(1, 2));

    [Fact]
    public void ThenBothRunOnThatStepAlone()
    {
        Given<IMyValueIntRepo>().That(_ => _.Get(Any<int>()))
            .First().Tap(() => _seen.Add("a1")).Tap(() => _seen.Add("a2")).Returns(() => "a")
            .AndNext().Tap(() => _seen.Add("b1")).Returns(() => "b");
        Then().Result.Is("ab");
        _seen.Is().EqualTo(["a1", "a2", "b1"]);
    }
}

public class TwoGetService(IMyValueIntRepo repo)
{
    public string GetTwice(int first, int second) => repo.Get(first) + repo.Get(second);
}
