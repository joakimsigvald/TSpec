using TSpec.Assert;
using TSpec.Internal.Specification;
using Xunit.Sdk;

namespace TSpec.Test.Assert;

/// <summary>
/// Locks the shape of the generated output across every combination of the four things that
/// can vary: whether the test failed, whether the failure carries a nested assertion failure,
/// whether setup warnings were collected, and whether values were assigned.
/// A passing test produces the specification text alone — warnings and values never appear.
/// </summary>
public class WhenRenderFailureOutput : Spec<int>
{
    private const string Warning =
        "Fussy: the constructor rejected the generated arguments (ArgumentException), "
        + "so the parameterless constructor was used instead. "
        + "Arrange it with Using<Fussy>(...) or Given().A<Fussy>(...) if that is not what you want.";

    // ---------- no error: only the specification is produced ----------

    [Fact]
    public void GivenNoErrorNoWarningNoValues()
    {
        int n = 1;
        n.Is(1);
        Specification.Is("N is 1");
    }

    [Fact]
    public void GivenNoErrorNoWarningWithValues()
    {
        An<int>().Is().GreaterThan(0);
        Specification.Is("An int is greater than 0");
    }

    [Fact]
    public void GivenNoErrorWithWarningNoValues()
    {
        Any<Fussy>();
        int n = 1;
        n.Is(1);
        Specification.Is("N is 1");
    }

    [Fact]
    public void GivenNoErrorWithWarningWithValues()
    {
        A<Fussy>().Quantity.Is(0);
        Specification.Is("A<Fussy>().Quantity is 0");
    }

    // ---------- error without a nested assertion failure ----------

    [Fact]
    public void GivenErrorNoInnerNoWarningNoValues()
        => AssertOutput(
            () => { int n = 1; n.Is(2); },
            "Expected n to be 2 but found 1",
            """

            N is 2
            """);

    [Fact]
    public void GivenErrorNoInnerNoWarningWithValues()
        => AssertOutput(
            () => An<int>().Is(99),
            "Expected an int to be 99 but found 1",
            """

            An int is 99

            === VALUES ===
            int:1 = 1

            """);

    [Fact]
    public void GivenErrorNoInnerWithWarningNoValues()
        => AssertOutput(
            () => { Any<Fussy>(); int n = 1; n.Is(2); },
            "Expected n to be 2 but found 1",
            $"""

            N is 2

            === WARNINGS ===
            {Warning}
            """);

    [Fact]
    public void GivenErrorNoInnerWithWarningWithValues()
        => AssertOutput(
            () => A<Fussy>().Quantity.Is(1),
            "Expected A<Fussy>().Quantity to be 1 but found 0",
            $"""

            A<Fussy>().Quantity is 1

            === WARNINGS ===
            {Warning}

            === VALUES ===
            Fussy:1 = TSpec.Test.Assert.Fussy

            """);

    // ---------- error carrying a nested assertion failure ----------

    [Fact]
    public void GivenErrorWithInnerNoWarningNoValues()
        => AssertOutput(
            () => { int[] arr = [1, 3]; arr.Has().All(it => it.Is().LessThan(3)); },
            """
            Expected arr to have all elements satisfying the assertion but found [1, 3]
            Expected it to be less than 3 but found 3
            """,
            """

            Arr has all it.Is().LessThan(3)
            """);

    [Fact]
    public void GivenErrorWithInnerNoWarningWithValues()
        => AssertOutput(
            () => Two<int>().Has().All(it => it.Is().LessThan(2)),
            """
            Expected two ints to have all elements satisfying the assertion but found [1, 2]
            Expected it to be less than 2 but found 2
            """,
            """

            Two ints has all it.Is().LessThan(2)

            === VALUES ===
            int:1 = 1
            int:2 = 2
            int[]:1 = [1, 2]

            """);

    [Fact]
    public void GivenErrorWithInnerWithWarningNoValues()
        => AssertOutput(
            () => { Any<Fussy>(); int[] arr = [1, 3]; arr.Has().All(it => it.Is().LessThan(3)); },
            """
            Expected arr to have all elements satisfying the assertion but found [1, 3]
            Expected it to be less than 3 but found 3
            """,
            $"""

            Arr has all it.Is().LessThan(3)

            === WARNINGS ===
            {Warning}
            """);

    [Fact]
    public void GivenErrorWithInnerWithWarningWithValues()
        => AssertOutput(
            () => { Any<Fussy>(); Two<int>().Has().All(it => it.Is().LessThan(2)); },
            """
            Expected two ints to have all elements satisfying the assertion but found [2, 3]
            Expected it to be less than 2 but found 2
            """,
            $"""

            Two ints has all it.Is().LessThan(2)

            === WARNINGS ===
            {Warning}

            === VALUES ===
            int:1 = 2
            int:2 = 3
            int[]:1 = [2, 3]

            """);

    // ---------- several warnings, each spanning several lines ----------

    [Fact]
    public void GivenTwoMultiLineWarnings_ThenBlankLineSeparatesThem()
    {
        AddWarning("""
            First warning, first line
            first warning, second line
            """);
        AddWarning("""
            Second warning, first line
            second warning, second line
            """);
        AssertOutput(
            () => { int n = 1; n.Is(2); },
            "Expected n to be 2 but found 1",
            """

            N is 2

            === WARNINGS ===
            First warning, first line
            first warning, second line

            Second warning, first line
            second warning, second line
            """);
    }

    [Fact]
    public void GivenTheSameWarningTwice_ThenItIsReportedOnce()
    {
        AddWarning("Reported once");
        AddWarning("Reported once");
        AssertOutput(
            () => { int n = 1; n.Is(2); },
            "Expected n to be 2 but found 1",
            """

            N is 2

            === WARNINGS ===
            Reported once
            """);
    }

    private static void AddWarning(string warning) => SpecificationContext.Current.AddSetupWarning(warning);

    private static void AssertOutput(Action act, string expectedMessage, string expectedSpecification)
    {
        var ex = Xunit.Assert.Throws<XunitException>(act);
        ex.Message.NormalizeLineEndings().Is(expectedMessage.NormalizeLineEndings());
        ex.InnerException!.Message.NormalizeLineEndings().Is(expectedSpecification.NormalizeLineEndings());
    }
}
