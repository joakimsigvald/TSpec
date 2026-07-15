using TSpec.Assert;
using static TSpec.Test.Subjects.Calculator;

namespace TSpec.Test.Tests.Calculator;

public class CalculatorSpec : Spec<int>
{
    [Fact] public void WhenAddZeros_ThenSumIsZero() => When(_ => Add(0, 0)).Then().Result.Is(0);

    [Fact] public void WhenAdd_1_and_2_ThenSumIs_3() => When(_ => Add(1, 2)).Then().Result.Is(3);

    [Theory]
    [InlineData(1, 1, 2)]
    [InlineData(3, 4, 7)]
    public void WhenAdd_ThenReturnSum(int x, int y, int sum)
    {
        When(_ => Add(x, y)).Then().Result.Is(sum);
        Specification.Is(
            """
            When add x, y
            Then Result is sum
            """);
    }

    [Theory]
    [InlineData(1, 1, 1)]
    [InlineData(3, 4, 12)]
    public void WhenMultiply_ThenReturnProduct(int x, int y, int product)
    {
        When(_ => Multiply(x, y)).Then().Result.Is(product);
        Specification.Is(
            """
            When multiply x, y
            Then Result is product
            """);
    }
}