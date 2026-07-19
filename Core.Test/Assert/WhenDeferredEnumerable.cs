using TSpec.Assert;
using Xunit.Sdk;

namespace TSpec.Test.Assert;

// Deferred sequences are lazily cached on Is()/Has()/Does(), so each element is produced
// at most once, failure descriptions show the elements that were asserted,
// and short-circuiting assertions work on infinite sequences
public class WhenDeferredEnumerable
{
    [Fact]
    public void ThenChainedAssertionsEnumerateOnce()
    {
        var enumerations = 0;
        var seq = Enumerable.Range(1, 3).Select(i => { if (i == 1) enumerations++; return i; });
        seq.Has().Count(3).and.All(i => i > 0).and.Is().Distinct();
        enumerations.Is(1);
    }

    [Fact]
    public void GivenFailure_ThenDescriptionShowsAssertedElements()
    {
        var calls = 0;
        var seq = Enumerable.Range(0, 3).Select(_ => ++calls); // re-enumeration would yield 4, 5, 6
        var ex = Xunit.Assert.Throws<XunitException>(() => seq.Has().Count(4));
        ex.Message.Does().Contain("found [1, 2, 3]");
    }

    [Fact]
    public void GivenMaterializedCollection_ThenSameAsStillHolds()
    {
        var list = new List<int> { 1, 2, 3 };
        list.Is().SameAs(list);
    }

    [Fact]
    public void GivenInfiniteSequence_ThenShortCircuitingAssertionPasses()
    {
        var pulled = 0;
        Infinite().Does().Contain(3);
        pulled.Is(4);

        IEnumerable<int> Infinite()
        {
            for (var i = 0; ; i++)
            {
                pulled++;
                yield return i;
            }
        }
    }
}
