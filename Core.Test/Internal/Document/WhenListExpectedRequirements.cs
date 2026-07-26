using TSpec.Assert;
using TSpec.Internal.Document;

namespace TSpec.Test.Internal.Document;

public class WhenListExpectedRequirements : Spec
{
    private static IReadOnlySet<string> Expected
        => ExpectedRequirements.Of(typeof(WhenListExpectedRequirements).Assembly);

    private static bool Lists<T>(string methodName) => Expected.Contains(ExpectedRequirements.Identity(typeof(T), methodName));

    [Fact]
    public void ThenIncludeARunnableTest()
        => Lists<WhenListExpectedRequirements>(nameof(ThenIncludeARunnableTest)).Is(true);

    /// <summary>A skipped test contributes nothing, so expecting it would block every document.</summary>
    [Fact]
    public void ThenExcludeASkippedTest()
        => Lists<SkippedSample>(nameof(SkippedSample.ThenNeverRuns)).Is(false);

    /// <summary>xunit never runs an abstract class, so it must not be expected to report.</summary>
    [Fact]
    public void ThenExcludeAnAbstractSpec()
        => Lists<AbstractSample>(nameof(AbstractSample.ThenOnlyRunsViaASubclass)).Is(false);

    [Fact]
    public void ThenIncludeATestInheritedByAConcreteClass()
        => Lists<ConcreteSample>(nameof(AbstractSample.ThenOnlyRunsViaASubclass)).Is(true);

    [Fact]
    public void GivenNothingReported_ThenEveryRequirementIsMissing()
        => SpecificationCollector.Missing(new HashSet<string> { "A.b", "A.a" }).Is().EqualTo(["A.a", "A.b"]);

    public class SkippedSample : Spec
    {
        [Fact(Skip = "Present so that skip-handling has a regression test")]
        public void ThenNeverRuns() { }
    }

    public abstract class AbstractSample : Spec
    {
        [Fact] public void ThenOnlyRunsViaASubclass() { }
    }

    public class ConcreteSample : AbstractSample;
}
