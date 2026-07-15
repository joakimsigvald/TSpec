using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Given;

public class WhenGivenDefaultEnumValue : Spec<MyService, MyEnum>
{
    public WhenGivenDefaultEnumValue() => When(_ => MyService.Echo(The<MyEnum>())).Using(MyEnum.Two);
    [Fact]
    public void ThenUseDefaultValue()
    {
        Result.Is(MyEnum.Two);
        Specification.Is(
            """
            Using MyEnum.Two
            When MyService.Echo(the MyEnum)
            Then Result is MyEnum.Two
            """);
    }
}