using TSpec.Generator.Test.Samples;

namespace TSpec.Generator.Test;

/// <summary>
/// A test method that only asserts on a result no assertion can change may share its siblings' run.
/// Anything else it does could depend on a run of its own, so it gets one.
/// </summary>
public class WhenListShareableThens : Spec<string[]>
{
    [Theory]
    [InlineData(nameof(WhenGetCount_SplitAsserts),
        nameof(WhenGetCount_SplitAsserts.ThenItIsNotNull), nameof(WhenGetCount_SplitAsserts.ThenItIs42))]
    [InlineData(nameof(WhenGetCount_CombinedAsserts), 
        nameof(WhenGetCount_CombinedAsserts.ThenItIs42))]
    [InlineData(nameof(WhenGetText), 
        nameof(WhenGetText.ThenItHasA4))]
    [InlineData(nameof(WhenGetNumbers))]
    [InlineData(nameof(WhenGetCount_NonConstantArgument))]
    [InlineData(nameof(WhenGetCount_AssertOnAStatic))]
    [InlineData(nameof(WhenGetCount_Async))]
    [InlineData(nameof(WhenGetCount_ProtectedGiven))]
    public void ThenListOnlyWhatAssertsOnAPrimitiveResult(string sample, params string[] listed)
        => When(() => ShareableThens.In(sample)).Then().Result.Is().Like(listed);
}
