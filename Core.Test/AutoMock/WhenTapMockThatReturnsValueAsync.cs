using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.AutoMock;

public class WhenTapMockThatReturnsValueAsync : Spec<MyValueIntService, string>
{
    private const string _retVal = "abc";
    private int _tappedValue = 0;

    [Fact]
    public void ThenTappedValueIsSet()
    {
        When(_ => _.GetValueAsync(A<MyValueInt>()))
            .Given<IMyValueIntRepo>()
            .That(_ => _.GetAsync(The<MyValueInt>()))
            .Tap((int value) => _tappedValue = value)
            .Returns(() => _retVal)
            .Then();
        _tappedValue.Is(The<MyValueInt>());
        Specification.Is(
            """
            Given IMyValueIntRepo.GetAsync(the MyValueInt) tap((int value) => _tappedValue
                  = value) returns _retVal
            When _.GetValueAsync(a MyValueInt)
            Then _tappedValue is the MyValueInt
            """);
    }

    [Fact]
    public void ThenTapAsyncWithoutReturnValue()
    {
        When(_ => _.SetValueAsync(A<MyValueInt>()))
            .Given<IMyValueIntRepo>()
            .That(_ => _.SetAsync(The<MyValueInt>()))
            .Tap((int value) => _tappedValue = value)
            .Returns()
            .Then();
        _tappedValue.Is(The<MyValueInt>());
        Specification.Is(
            """
            Given IMyValueIntRepo.SetAsync(the MyValueInt) tap((int value) => _tappedValue
                  = value) returns
            When _.SetValueAsync(a MyValueInt)
            Then _tappedValue is the MyValueInt
            """);
    }

    [Fact]
    public void ThenCanReturnTappedValueAsync()
    {
        When(_ => _.GetValueAsync(A<MyValueInt>()))
                .Given<IMyValueIntRepo>()
            .That(_ => _.GetAsync(The<MyValueInt>()))
            .Returns<int>(i => $"{2 * i}")
            .And<IMyValueIntRepo>().That(_ => _.Get(The<MyValueInt>()))
            .Returns<int>(i => $"{3 * i}")
            .Then().Result.Is($"{2 * The<MyValueInt>()}");
        Specification.Is(
            """
            Given IMyValueIntRepo.GetAsync(the MyValueInt) returns "{2 * i}"
              and Get(the MyValueInt) returns "{3 * i}"
            When _.GetValueAsync(a MyValueInt)
            Then Result is "{2 * The<MyValueInt>()}"
            """);
    }

    [Fact]
    public void ThenCanReturnTappedValue()
    {
        When(_ => _.GetValue(A<MyValueInt>()))
                .Given<IMyValueIntRepo>()
            .That(_ => _.GetAsync(The<MyValueInt>()))
            .Returns<int>(i => $"{2 * i}")
            .And<IMyValueIntRepo>().That(_ => _.Get(The<MyValueInt>()))
            .Returns<int>(i => $"{3 * i}")
            .Then().Result.Is($"{3 * The<MyValueInt>()}");
        Specification.Is(
            """
            Given IMyValueIntRepo.GetAsync(the MyValueInt) returns "{2 * i}"
              and Get(the MyValueInt) returns "{3 * i}"
            When _.GetValue(a MyValueInt)
            Then Result is "{3 * The<MyValueInt>()}"
            """);
    }
}