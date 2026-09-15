using TSpec.Assert;

namespace TSpec.Test.Pipeline;

/// <summary>
/// C# copies a value type when it is passed, so a subject handed to Then before the pipeline runs is
/// read before the act and cannot see what the act changes.
/// </summary>
public class WhenAValueIsHandedOverBeforeThePipelineRuns
{
    private sealed class MySpec : Spec<int> { }

    [Fact]
    public void GivenAValueType_ThenThrowSetupFailed()
    {
        using var spec = new MySpec();
        var tapped = 0;
        spec.When(() => tapped = 1);
        Xunit.Assert.Throws<SetupFailed>(() => spec.Then(tapped)).Message.Is(
            "Then(tapped) hands over a copy of tapped taken before the pipeline runs, so it cannot see "
            + "what the pipeline changes. Call Then() first, then assert on tapped");
    }

    [Fact]
    public void GivenAValueTypeAfterThePipelineRan_ThenDoNotComplain()
    {
        using var spec = new MySpec();
        var tapped = 0;
        spec.When(() => tapped = 1).Then();
        spec.Then(tapped).Is(1);
    }
}
