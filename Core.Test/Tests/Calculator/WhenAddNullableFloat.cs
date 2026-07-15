using TSpec.Assert;
using static TSpec.Test.Subjects.Calculator;

namespace TSpec.Test.Tests.Calculator;

public class AddF_1_1 : Spec<float?>
{
    public AddF_1_1() => When(_ => Add(1, 1));

    [Fact]
    public void Then_Return_2()
    {
        Result.Is(2);
        Specification.Is(
            """
            When add 1, 1
            Then Result is 2
            """);
    }

    [Fact]
    public void Then_Return_Between_1_And_3()
    {
        Result.Is().GreaterThan(1).and.LessThan(3);
        Specification.Is(
            """
            When add 1, 1
            Then Result is greater than 1
                and less than 3
            """);
    }
}

public class AddF_2_3 : Spec<float?>
{
    public AddF_2_3() => When(_ => Add(2, 3));
    [Fact] public void Then_Return_5() => Result.Is(5);
}