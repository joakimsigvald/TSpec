using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Enumerable.HasEnumerable;

public class WhenAllAssert : Spec
{
    [Fact]
    public void GivenEmpty_ThenCompletes()
        => Zero<int>().Has().All(it => it.Is(1)).and.Is().Empty();

    [Fact]
    public void GivenAllSatisfyCondition_ThenCompletes()
    {
        int[] arr = [1, 2];
        arr.Has().All(it => it.Is().not.LessThan(1)).and.Is().not.Empty();
    }

    [Fact]
    public void GivenConditionNotSatisfiedForAll_ThenGetException()
    {
        int[] arr = [1, 3];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Has().All(it => it.Is().LessThan(3)));
        ex.HasMessage(
            "Expected arr to have all elements be less than 3 but found 1 that does not: [3]",
            """
            For each of arr:
                it is less than 3
            """);
    }

    [Fact]
    public void GivenSeveralItemsFailAlike_ThenTheAssertionGoesInline()
    {
        int[] arr = [1, 3, 4];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Has().All(it => it.Is().LessThan(3)));
        ex.HasMessage(
            "Expected arr to have all elements be less than 3 but found 2 that does not: [3, 4]",
            """
            For each of arr:
                it is less than 3
            """);
    }

    [Fact]
    public void GivenAMemberOfTheItemFails_ThenEachFailureIsListedWithItsItemNamed()
    {
        string[] words = ["one", "three", "seven"];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => words.Has().All(word => word.Length.Is().LessThan(4)));
        ex.HasMessage(
            """
            Expected words to have all elements satisfying the assertion but found 2 that does not: ["three", "seven"]
              Expected "three"'s Length to be less than 4 but it was 5
              Expected "seven"'s Length to be less than 4 but it was 5
            """,
            """
            For each of words:
                its Length is less than 4
            """);
    }

    [Fact]
    public void GivenAnAssertionThatNamesTheItem_ThenItIsListedNotInline()
    {
        int[] arr = [1, 3];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Has().All(it => it.Is().LessThan(it - 1)));
        ex.HasMessage(
            """
            Expected arr to have all elements satisfying the assertion but found 2 that does not: [1, 3]
              Expected 1 to be less than 0
              Expected 3 to be less than 2
            """,
            """
            For each of arr:
                it is less than it - 1
            """);
    }

    [Fact]
    public void GivenTheItemAsAnArgument_ThenItsFailureNamesTheItem()
    {
        int[] arr = [1, -3];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Has().All(it => Math.Abs(it).Is().LessThan(2)));
        ex.HasMessage(
            """
            Expected arr to have all elements satisfying the assertion but found 1 that does not: [-3]
              Expected Math.Abs(-3) to be less than 2 but it was 3
            """,
            """
            For each of arr:
                Math.Abs(it) is less than 2
            """);
    }

    [Fact]
    public void GivenANestedAllFails_ThenEachInnerFailureIsListedOnceBelowItsItem()
    {
        string[] words = ["one", "three"];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => words.Has().All(word => word.Split('e').Has().All(part => part.Length.Is().LessThan(3))));
        ex.HasMessage(
            """
            Expected words to have all elements satisfying the assertion but found 1 that does not: ["three"]
              Expected "three"'s Split('e') to have all elements satisfying the assertion but found 1 that does not: ["thr"]
                Expected "thr"'s Length to be less than 3 but it was 3
            """,
            """
            For each of words:
                for each of its Split('e'):
                    its Length is less than 3
            """);
    }

    [Fact]
    public void ThenItReadsAsTheAssertionOnEachItem()
    {
        int[] arr = [1, 2];
        arr.Has().All(it => it.Is().LessThan(3));
        Specification.Is(
            """
            For each of arr:
                it is less than 3
            """);
    }

    [Fact]
    public void GivenAnAndAfterIt_ThenTheAndStandsAtTheMarginAndNamesTheActual()
    {
        int[] arr = [1, 2];
        arr.Has().All(it => it.Is().not.LessThan(1)).and.Is().not.Empty();
        Specification.Is(
            """
            For each of arr:
                it is not less than 1
              and arr is not empty
            """);
    }

    [Fact]
    public void GivenAMemberOfTheItem_ThenTheItemReadsAsIts()
    {
        string[] words = ["one", "two"];
        words.Has().All(word => word.Length.Is().LessThan(4));
        Specification.Is(
            """
            For each of words:
                its Length is less than 4
            """);
    }

    [Fact]
    public void GivenAGenericMethodOfTheItem_ThenTheItemReadsAsIts()
    {
        object[][] rows = [[1], [2]];
        rows.Has().All(row => row.OfType<int>().Has().None(cell => cell > 5));
        Specification.Is(
            """
            For each of rows:
                its OfType<int>() has none cell > 5
            """);
    }

    [Fact]
    public void GivenAnEitherEndedByAFix_ThenTheFixIsLeftOut()
    {
        int[][] rows = [[1], [2]];
        rows.Has().All(row => row.Has().either.None(cell => cell > 5).or.Fix("split the row"));
        Specification.Is(
            """
            For each of rows:
                row has none cell > 5
            """);
    }

    [Fact]
    public void GivenAnAllOnEachItem_ThenItsForEachStandsBelow()
    {
        int[][] rows = [[1], [2]];
        rows.Has().All(row => row.Has().All(cell => cell.Is().LessThan(5)));
        Specification.Is(
            """
            For each of rows:
                for each of row:
                    cell is less than 5
            """);
    }

    [Fact]
    public void GivenAnAndAfterANestedAll_ThenTheAndStandsAtTheMargin()
    {
        int[][] rows = [[1], [2]];
        rows.Has().All(row => row.Has().All(cell => cell.Is().LessThan(5))).and.Is().not.Empty();
        Specification.Is(
            """
            For each of rows:
                for each of row:
                    cell is less than 5
              and rows is not empty
            """);
    }

    [Fact]
    public void GivenAValue_ThenItReadsAsTheValue()
    {
        int[] arr = [1, 1];
        arr.Has().All(it => it.Is(1));
        Specification.Is(
            """
            For each of arr:
                it is 1
            """);
    }

    [Fact]
    public void GivenAConditionOnTheItemsOfAnItem_ThenTheConditionReadsAsItsBody()
    {
        int[][] rows = [[1], [2]];
        rows.Has().All(row => row.Has().None(cell => cell > 5));
        Specification.Is(
            """
            For each of rows:
                row has none cell > 5
            """);
    }

    [Fact]
    public void GivenDoes_ThenItReadsInTheThirdPerson()
    {
        string[] words = ["one", "bone"];
        words.Has().All(word => word.Does().Contain("one"));
        Specification.Is(
            """
            For each of words:
                word contains "one"
            """);
    }

    [Fact]
    public void GivenANegation_ThenItReadsWithNot()
    {
        string[] words = ["one", "two"];
        words.Has().All(word => word.Does().not.Contain("x"));
        Specification.Is(
            """
            For each of words:
                word does not contain "x"
            """);
    }

    [Fact]
    public void GivenTheItemAsAnArgument_ThenItKeepsItsName()
    {
        int[] arr = [1, 2];
        arr.Has().All(it => Math.Abs(it).Is(it));
        Specification.Is(
            """
            For each of arr:
                Math.Abs(it) is it
            """);
    }

    [Fact]
    public void GivenAnAssertionItDoesNotRead_ThenTheAssertionStaysAsWritten()
    {
        int[] arr = [1, 2];
        arr.Has().All(it => { it.Is().GreaterThan(0); });
        Specification.Is(
            """
            For each of arr:
                { it.Is().GreaterThan(0); }
            """);
    }
}
