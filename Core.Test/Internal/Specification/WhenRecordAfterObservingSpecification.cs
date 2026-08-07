using TSpec.Assert;
using TSpec.Internal.Specification;

namespace TSpec.Test.Internal.Specification;

/// <summary>
/// The specification freezes the first time it is observed. <c>Specification.Is(...)</c> reads it
/// from inside a test and then asserts — and that assertion is itself a recordable step, so without
/// the freeze a test would describe the act of checking its own description. Nothing else covers
/// this: it is visible only where the steps are read again afterwards, which is the document.
/// </summary>
public class WhenRecordAfterObservingSpecification : Spec
{
    private static (SpecificationRecording Recording, ActionPhrases Steps) Recorded()
    {
        var recording = new SpecificationRecording();
        var steps = new ActionPhrases(recording);
        steps.AddWhen("_.Act()");
        return (recording, steps);
    }

    [Fact]
    public void ThenTheTextIsUnchanged()
    {
        var (recording, steps) = Recorded();
        var observed = recording.ToString();
        steps.AddHaving("_.TooLate()");
        recording.ToString().Is(observed);
    }

    [Fact]
    public void ThenTheClausesAreUnchanged()
    {
        var (recording, steps) = Recorded();
        recording.ToString();
        steps.AddHaving("_.TooLate()");
        recording.Clauses.Count.Is(1);
    }

    /// <summary>Reading the clauses freezes it just as reading the text does.</summary>
    [Fact]
    public void GivenTheClausesWereReadFirst_ThenTheTextIsUnchanged()
    {
        var (recording, steps) = Recorded();
        recording.Clauses.Count.Is(1);
        steps.AddHaving("_.TooLate()");
        recording.ToString().Does().not.Contain("TooLate");
    }
}
