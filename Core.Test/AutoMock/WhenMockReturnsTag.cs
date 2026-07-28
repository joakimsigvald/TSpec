using TSpec.Assert;

namespace TSpec.Test.AutoMock;

public class WhenMockReturnsTag : Spec<InterfaceService, int> 
{
    const int _123 = 123;
    static readonly Tag<int> _value = new();

    public WhenMockReturnsTag() 
        => When(_ => _.GetServiceValue())
        .Given<IMyService>().That(_ => _.GetValue()).Returns(_value)
        .And(_value).Is(_123);

    [Fact]
    public void ThenReturnTaggedValue()
    {
        Then().Result.Is(_123);
        Specification.Is(
            """
            Given Value is _123
              and IMyService.GetValue() returns Value
            When _.GetServiceValue()
            Then Result is _123
            """);
    }
}

public class WhenMockWithTag : Spec<InterfaceService>
{
    static readonly Tag<int> _value = new();

    [Fact]
    public void WhenThrowsSpecificException()
    {
        When(_ => _.SetValue(The(_value)))
        .Given<IMyService>().That(_ => _.SetValue(The(_value))).Throws(() => new ArgumentException())
        .Then().Throws<ArgumentException>();
        Specification.Is(
            """
            Given IMyService.SetValue(the Value) throws new ArgumentException()
            When _.SetValue(the Value)
            Then throws ArgumentException
            """);
    }

    [Fact]
    public void WhenThrowsTypeOfException()
    {
        When(_ => _.SetValue(The(_value)))
        .Given<IMyService>().That(_ => _.SetValue(The(_value))).Throws<ArgumentException>()
        .Then().Throws<ArgumentException>();
        Specification.Is(
            """
            Given IMyService.SetValue(the Value) throws ArgumentException
            When _.SetValue(the Value)
            Then throws ArgumentException
            """);
    }
}