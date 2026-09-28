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
