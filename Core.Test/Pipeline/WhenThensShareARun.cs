using TSpec.Assert;
using TSpec.Test.Pipeline;
using Xunit.Sdk;

[assembly: TSpec.ShareableThen(typeof(WhenThensShareARun), nameof(WhenThensShareARun.ThenOneSeesTheOnlyRun))]
[assembly: TSpec.ShareableThen(typeof(WhenThensShareARun), nameof(WhenThensShareARun.ThenAnotherSeesItToo))]

namespace TSpec.Test.Pipeline;

/// <summary>
/// Test methods listed as only asserting on the result share one run of the pipeline, and each
/// states that run as if it were its own. The act counts its runs, so a second run would show.
/// The listing is written by hand here, since this project does not run the source generator.
/// </summary>
public class WhenThensShareARun : Spec<int>
{
    private static int _runs;

    public WhenThensShareARun() => When(() => ++_runs * The<int>()).Given().A(10);

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
