using TSpec.Assert;

namespace TSpec.Test.AutoMock;

public class WhenMockNTuple : Spec<StaticNTupleService, (int, string, int, float)>
{
    public WhenMockNTuple() => Using(A<(int, string, int, float)>).When(_ => _.GetValue());
    public class UsingAValue : WhenMockNTuple
    {
        [Fact]
        public void Then_It_Has_TheValue()
        {
            Then().Result.Is(The<(int, string, int, float)>());
            Specification.Is(
                """
                Using a (int, string, int, float)
                When _.GetValue()
                Then Result is the (int, string, int, float)
                """);
        }
    }

    public class GivenItWasProvided : WhenMockNTuple
    {
        [Theory]
        [InlineData(0, null, 1, 2)]
        [InlineData(1, "", 2, 3)]
        [InlineData(2, "hej", 3, 4)]
        public void Then_It_Has_ProvidedValue(int v1, string? v2, int v3, float v4)
        {
            Using((v1, v2, v3, v4)).Then().Result.Is((v1, v2, v3, v4));
            Specification.Is(
                """
                Using (v1, v2, v3, v4)
                  and a (int, string, int, float)
                When _.GetValue()
                Then Result is (v1, v2, v3, v4)
                """);
        }
    }
}

public class StaticNTupleService((int, string, int, float) value)
{
    private readonly (int, string, int, float) _value = value;
    public (int, string, int, float) GetValue() => _value;
}