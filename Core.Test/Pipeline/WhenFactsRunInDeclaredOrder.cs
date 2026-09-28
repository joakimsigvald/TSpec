using TSpec.Assert;

namespace TSpec.Test.Pipeline;

/// The leaf's Facts are named against alphabetical order, so that no other order passes by chance.
public abstract class WhenFactsRunInDeclaredOrder : Spec<int>
{
    private static readonly List<string> _ran = [];

    private static readonly string[] _declared =
    [
        nameof(ThenTheBaseFactRunsFirst),
        nameof(GivenALeafClass.ThenTheLeafsFirstFactFollows),
        nameof(GivenALeafClass.ThenAnotherFollowsIt),
        nameof(GivenALeafClass.ThenALastOneFollowsThem),
    ];

    [Fact] public void ThenTheBaseFactRunsFirst() => Ran(nameof(ThenTheBaseFactRunsFirst));

    private protected static void Ran(string fact)
    {
        _ran.Add(fact);
        _ran.Is().EqualTo(_declared[.._ran.Count]);
    }

    public class GivenALeafClass : WhenFactsRunInDeclaredOrder
    {
        [Fact] public void ThenTheLeafsFirstFactFollows() => Ran(nameof(ThenTheLeafsFirstFactFollows));

        [Fact] public void ThenAnotherFollowsIt() => Ran(nameof(ThenAnotherFollowsIt));

        [Fact] public void ThenALastOneFollowsThem() => Ran(nameof(ThenALastOneFollowsThem));
    }
}
