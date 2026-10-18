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

    [Fact]
    public void ThenExcludeAnExplicitTest()
        => Lists<ExplicitSample>(nameof(ExplicitSample.ThenRunsOnlyWhenAsked)).Is(false);

    /// <summary>xunit never runs an abstract class, so it must not be expected to report.</summary>
    [Fact]
    public void ThenExcludeAnAbstractSpec()
        => Lists<AbstractSample>(nameof(AbstractSample.ThenOnlyRunsViaASubclass)).Is(false);

    [Fact]
    public void ThenIncludeATestInheritedByAConcreteClass()
        => Lists<ConcreteSample>(nameof(AbstractSample.ThenOnlyRunsViaASubclass)).Is(true);

    [Fact]
    public void GivenNothingReported_ThenEveryRequirementIsMissing()
        => new SpecificationCollector().Missing(new HashSet<string> { "A.b", "A.a" }).Is().EqualTo(["A.a", "A.b"]);

    /// <summary>
    /// A test skipped while it ran — Assert.Skip in the body, or a mapped SkipException — reports
    /// nothing, and the attribute cannot tell it apart from a test that never ran. Counted as
    /// missing it would stop the document being written for as long as the skip stood, on a green
    /// pipeline that says nothing is wrong.
    /// </summary>
    [Fact]
    public void GivenATestSkippedWhileItRan_ThenItIsNotMissing()
    {
        SpecificationCollector collector = new();
        collector.Add("A.SkippedWhileItRan", TestResult.Skipped, AnEntry);
        collector.Missing(new HashSet<string> { "A.SkippedWhileItRan" }).Is().Empty();
    }

    /// <summary>A skip excuses itself and nothing else: a run that fell short is still short.</summary>
    [Fact]
    public void GivenATestSkippedWhileItRan_ThenTheRestAreStillMissing()
    {
        SpecificationCollector collector = new();
        collector.Add("A.AlsoSkipped", TestResult.Skipped, AnEntry);
        collector.Missing(new HashSet<string> { "A.AlsoSkipped", "A.NeverRan" }).Is().EqualTo(["A.NeverRan"]);
    }

    /// <summary>A theory's rows share one requirement, so a row that passed does not excuse one that failed.</summary>
    [Fact]
    public void GivenATheoryRowFailedAfterAnotherPassed_ThenItIsMissing()
    {
        SpecificationCollector collector = new();
        collector.Add("A.Theory", TestResult.Passed, AnEntry);
        collector.Add("A.Theory", TestResult.Failed, AnEntry);
        collector.Missing(new HashSet<string> { "A.Theory" }).Is().EqualTo(["A.Theory"]);
    }

    [Fact]
    public void GivenATheoryRowFailedBeforeAnotherPassed_ThenItIsMissing()
    {
        SpecificationCollector collector = new();
        collector.Add("A.Theory", TestResult.Failed, AnEntry);
        collector.Add("A.Theory", TestResult.Passed, AnEntry);
        collector.Missing(new HashSet<string> { "A.Theory" }).Is().EqualTo(["A.Theory"]);
    }

    private static SpecificationEntry AnEntry() => new("A", "", "Requirement", []);

    public class SkippedSample : Spec
    {
        [Fact(Skip = "Present so that skip-handling has a regression test")]
        public void ThenNeverRuns() { }
    }

    public class ExplicitSample : Spec
    {
        [Fact(Explicit = true)]
        public void ThenRunsOnlyWhenAsked() { }
    }

    public abstract class AbstractSample : Spec
    {
        [Fact] public void ThenOnlyRunsViaASubclass() { }
    }

    public class ConcreteSample : AbstractSample;
}
