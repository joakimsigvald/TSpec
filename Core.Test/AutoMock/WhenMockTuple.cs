using TSpec.Assert;

namespace TSpec.Test.AutoMock;

public class WhenMockTuple : Spec<StaticTupleService, (int, string)>
{
    public WhenMockTuple() => Using(A<(int, string)>).When(_ => _.GetValue());
    public class UsingAValue : WhenMockTuple
    {
        [Fact]
        public void Then_It_Has_TheValue()
        {
            Then().Result.Is(The<(int, string)>());
            Specification.Is(
                """
                Using a (int, string)
                When _.GetValue()
                Then Result is the (int, string)
                """);
        }
    }

    public class GivenItWasProvided : WhenMockTuple
    {
        [Theory]
        [InlineData(0, null)]
        [InlineData(1, "")]
        [InlineData(2, "hej")]
        public void Then_It_Has_ProvidedValue(int v1, string? v2)
        {
            Using((v1, v2)).Then().Result.Is((v1, v2));
            Specification.Is(
                """
                Using (v1, v2)
                  and a (int, string)
                When _.GetValue()
                Then Result is (v1, v2)
                """);
        }
    }
}

public class StaticTupleService((int, string) value)
{
    private readonly (int, string) _value = value;
    public (int, string) GetValue() => _value;
}