using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.AutoMock;

public class WhenTapMockWithTwoArguments : Spec<MyValueIntService, string>
{
    private const string _retVal = "abc";
    private int _tappedValue = 0;

    public WhenTapMockWithTwoArguments()
    {
        When(_ => _.GetValue2(A<MyValueInt>(), ASecond<MyValueInt>()))
            .Given<IMyValueIntRepo>()
            .That(_ => _.Get2(The<MyValueInt>(), TheSecond<MyValueInt>()))
            .Tap((int v1, int v2) => _tappedValue = v1 + v2)
            .Returns(() => _retVal);
    }

    [Fact]
    public void ThenTappedValueIsSet()
    {
        Then();
        _tappedValue.Is(The<MyValueInt>() + TheSecond<MyValueInt>());
        Specification.Is(
            """
            Given IMyValueIntRepo.Get2(the MyValueInt, the second MyValueInt) tap((int v1,
                  int v2) => _tappedValue = v1 + v2) returns _retVal
            When _.GetValue2(a MyValueInt, a second MyValueInt)
            Then _tappedValue is the MyValueInt + the second MyValueInt
            """);
    }
}