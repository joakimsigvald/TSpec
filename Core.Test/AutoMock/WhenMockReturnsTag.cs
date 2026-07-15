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
            Given _value is _123
              and IMyService.GetValue() returns _value
            When _.GetServiceValue()
            Then Result is _123
            """);
    }
}

public class WhenMockWithTag : Spec<InterfaceService>
{
    static readonly Tag<int> _value = new(nameof(_value));

    [Fact]
    public void WhenThrowsSpecificException()
    {
        When(_ => _.SetValue(The(_value)))
        .Given<IMyService>().That(_ => _.SetValue(The(_value))).Throws(() => new ArgumentException())
        .Then().Throws<ArgumentException>();
        Specification.Is(
            """
            Given IMyService.SetValue(the _value) throws new ArgumentException()
            When _.SetValue(the _value)
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
            Given IMyService.SetValue(the _value) throws ArgumentException
            When _.SetValue(the _value)
            Then throws ArgumentException
            """);
    }
}