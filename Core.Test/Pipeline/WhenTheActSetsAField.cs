using TSpec.Assert;

namespace TSpec.Test.Pipeline;

/// The field is declared above the class the Facts are in, so the whole hierarchy below Spec is copied.
public abstract class WhenTheActSetsAField : Spec<int>
{
    private protected int _seen;

    protected WhenTheActSetsAField() => When(() => _seen = 42);

    public class GivenFactsThatShareTheRun : WhenTheActSetsAField
    {
        [Fact] public void ThenOneReadsWhatTheActSet() => ThenTheFieldHoldsWhatTheActSet();

        [Fact] public void ThenAnotherReadsItToo() => ThenTheFieldHoldsWhatTheActSet();

        private void ThenTheFieldHoldsWhatTheActSet()
        {
            Result.Is(42);
            _seen.Is(42);
        }
    }
}

/// A field handed to Then is read before the run, so the Fact runs alone and sees what its own act added.
public class WhenAFieldIsHandedToThen : Spec<int>
{
    private readonly List<int> _added = [];

    public WhenAFieldIsHandedToThen() => When(() => _added.Add(42));

    [Fact] public void ThenOneSeesWhatTheActAdded() => ThenItHoldsWhatTheActAdded();

    [Fact] public void ThenAnotherSeesItToo() => ThenItHoldsWhatTheActAdded();

    private void ThenItHoldsWhatTheActAdded() => Then(_added).Has().OneItem();
}

/// Each test is given an output of its own, and the Fact that takes the run keeps it.
public class WhenATestTakesAnOutput : Spec<int>
{
    private readonly ITestOutputHelper _output;

    public WhenATestTakesAnOutput(ITestOutputHelper output)
    {
        _output = output;
        When(() => 1);
    }

    [Fact] public void ThenOneWritesToItsOwn() => ThenItWritesToItsOwn();

    [Fact] public void ThenAnotherWritesToItsOwnToo() => ThenItWritesToItsOwn();

    private void ThenItWritesToItsOwn()
    {
        Result.Is(1);
        Xunit.Assert.Same(TestContext.Current.TestOutputHelper, _output);
    }
}
