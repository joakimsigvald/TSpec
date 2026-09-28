using System.Reflection;
using TSpec.Assert;
using TSpec.Internal.Pipelines;

namespace TSpec.Test.Pipeline;

/// A Fact that failed on a shared run is run again alone, and the hint is written only if it passes then.
public class WhenAFactFailsOnASharedRun
{
    private sealed class OneSpec : Spec<int>
    {
        public OneSpec() => When(() => 1);

        public void ThenItIsOne() => Result.Is(1);

        public void ThenItIsTwo() => Result.Is(2);

        public async Task ThenItIsOneLater()
        {
            await Task.Yield();
            Result.Is(1);
        }

        public async Task ThenItIsTwoLater()
        {
            await Task.Yield();
            Result.Is(2);
        }
    }

    private sealed class FixtureSpec(object fixture) : Spec<object>
    {
        public void ThenItHasTheFixture() => When(() => fixture).Then().Result.Is().Not(null!);
    }

    [Theory]
    [InlineData(nameof(OneSpec.ThenItIsOne), true)]
    [InlineData(nameof(OneSpec.ThenItIsTwo), false)]
    [InlineData(nameof(OneSpec.ThenItIsOneLater), true)]
    [InlineData(nameof(OneSpec.ThenItIsTwoLater), false)]
    public void ThenTellWhetherItPassesAlone(string fact, bool passes)
        => CollisionHint.PassesAlone(typeof(OneSpec), Method<OneSpec>(fact)).Is(passes);

    [Fact]
    public void GivenItsClassTakesAFixture_ThenDoNotRunItAgain()
        => CollisionHint.PassesAlone(typeof(FixtureSpec), Method<FixtureSpec>(nameof(FixtureSpec.ThenItHasTheFixture)))
            .Is().False();

    [Fact]
    public void ThenTheHintNamesTheFactsThatRanBefore()
        => CollisionHint.Hint("ThenC", ["ThenA", "ThenB"]).Is(
            "ThenC passes when run alone. It shared its run with ThenA, ThenB, which ran before it "
            + "and may have changed what it reads. Move ThenC to a class of its own.");

    private static MethodInfo Method<TSpec>(string name) => typeof(TSpec).GetMethod(name)!;
}
