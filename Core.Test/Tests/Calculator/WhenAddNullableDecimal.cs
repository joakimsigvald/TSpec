using TSpec.Assert;
using static TSpec.Test.Subjects.Calculator;

namespace TSpec.Test.Tests.Calculator;

public class Add_1_1 : Spec<decimal?>
{
    public Add_1_1() => When(_ => Add(1M, 1M));

    [Fact]
    public void Then_Return_2()
    {
        Result.Is(2);
        Specification.Is(
            """
            When add 1M, 1M
            Then Result is 2
            """);
    }

    [Fact]
    public void Then_Return_Between_1_And_3()
    {
        Result.Is().GreaterThan(1).and.LessThan(3);
        Specification.Is(
            """
            When add 1M, 1M
            Then Result is greater than 1
                and less than 3
            """);
    }
}

public class Add_2_3 : Spec<decimal?>
{
    public Add_2_3() => When(_ => Add(2, 3));

    [Fact]
    public void Then_Return_5()
    {
        Result.Is(5);
        Specification.Is(
            """
            When add 2, 3
            Then Result is 5
            """);
    }

    [Fact]
    public void Then_Not_Return_Null()
    {
        Result.Is().not.Null();
        Specification.Is(
            """
            When add 2, 3
            Then Result is not null
            """);
    }
}

public class Add_1_Null : Spec<decimal?>
{
    public Add_1_Null() => When(_ => Add(1M, null));

    [Fact]
    public void Then_Return_Null()
    {
        Result.Is().Null();
        Specification.Is(
            """
            When add 1M, null
            Then Result is null
            """);
    }
}