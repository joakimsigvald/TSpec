using TSpec.Assert;
using Xunit.Sdk;

namespace TSpec.Test.Pipeline;

/// The act counts its runs, so a second run would show.
public class WhenFactsShareARun : Spec<int>
{
    private static int _runs;

    public WhenFactsShareARun() => When(() => ++_runs * The<int>()).Given().A(10);

    [Fact] public void ThenOneSeesTheOnlyRun() => ThenTheRunIsStatedAsItsOwn();

    [Fact] public void ThenAnotherSeesItToo() => ThenTheRunIsStatedAsItsOwn();

    private void ThenTheRunIsStatedAsItsOwn()
    {
        Result.Is(10);
        Specification.Is("""
            Given a int is 10
            When () => ++_runs * The<int>()
            Then Result is 10
            """);
        Xunit.Assert.Throws<XunitException>(() => Result.Is(11))
            .InnerException!.Message.Does().Contain("int:1 = 10");
    }
}
