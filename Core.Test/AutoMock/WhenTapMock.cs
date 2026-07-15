using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.AutoMock;

public class WhenTapMock : Spec<MyValueIntService, string>
{
    private const string _retVal = "abc";
    private int _tappedValue = 0;

    [Fact]
    public void GivenReturnValue_ThenTappedValueIsSet()
    {
        When(_ => _.GetValue(A<MyValueInt>()))
        .Given<IMyValueIntRepo>()
        .That(_ => _.Get(The<MyValueInt>()))
        .Tap<int>(i => _tappedValue = i)
        .Returns(() => _retVal).
        Then();
        _tappedValue.Is(The<MyValueInt>());
        Specification.Is(
            """
            Given IMyValueIntRepo.Get(the MyValueInt) tap(i => _tappedValue = i) returns
                  _retVal
            When _.GetValue(a MyValueInt)
            Then _tappedValue is the MyValueInt
            """);
    }

    [Fact]
    public void GivenReturnVoid_ThenTappedValueIsSet()
    {
        When(_ => _.SetValue(A<MyValueInt>()))
        .Given<IMyValueIntRepo>()
        .That(_ => _.Set(The<MyValueInt>()))
        .Tap<int>(i => _tappedValue = i)
        .Returns()
        .Then();
        _tappedValue.Is(The<MyValueInt>());
        Specification.Is(
            """
            Given IMyValueIntRepo.Set(the MyValueInt) tap(i => _tappedValue = i) returns
            When _.SetValue(a MyValueInt)
            Then _tappedValue is the MyValueInt
            """);
    }
}