using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.AutoMock;

public class WhenTapMockThatReturnsValue : Spec<MyValueIntService, string>
{
    private const string RetVal = "abc";
    private int _tappedValue = 0;

    public WhenTapMockThatReturnsValue()
        => When(_ => _.GetValue(A<MyValueInt>()))
        .Given<IMyValueIntRepo>()
        .That(_ => _.Get(The<MyValueInt>()))
        .Tap<int>(i => _tappedValue = i)
        .Returns(() => RetVal);

    [Fact]
    public void ThenTappedValueIsSet()
    {
        Then();
        _tappedValue.Is(The<MyValueInt>());
        Specification.Is(
            """
            Given IMyValueIntRepo.Get(the MyValueInt) tap(i => _tappedValue = i) returns
                  RetVal
            When _.GetValue(a MyValueInt)
            Then _tappedValue is the MyValueInt
            """);
    }
}