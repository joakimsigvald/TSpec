using TSpec.Assert;

namespace TSpec.Test.AutoMock;

public class WhenInjectingAnInterfaceWithUsing : Spec<InterfaceService, int>
{
    public WhenInjectingAnInterfaceWithUsing()
        => Using(new MyComponent(An<IMyLogger>(), An<int>())).When(_ => _.GetValue());

    [Fact]
    public void ThenGetValue()
    {
        Result.Is(The<int>());
        Specification.Is(
            """
            Using new MyComponent(an IMyLogger, an int)
            When _.GetValue()
            Then Result is the int
            """);
    }

    [Fact]
    public void ThenInterfaceIsMocked()
    {
        Then<IMyLogger>(_ => _.LogValue(The<int>()));
        Specification.Is(
            """
            Using new MyComponent(an IMyLogger, an int)
            When _.GetValue()
            Then IMyLogger.LogValue(the int)
            """);
    }
}

public class WhenUseConcreteInstanceOfInterface : Spec<InterfaceService, int>
{
    public WhenUseConcreteInstanceOfInterface()
        => Using(CreateService()).And(An<int>).When(_ => _.GetServiceValue());

    [Fact]
    public void ThenUseIt()
    {
        Result.Is(TheSecond<int>());
        Specification.Is(
            """
            Using CreateService()
              and an int
            When _.GetServiceValue()
            Then Result is the second int
            """);
    }

    protected IMyService CreateService() => new MyService(new MyComponent(A<IMyLogger>(), ASecond<int>()));
}

public class WhenUsingConcreteInstanceForInterface : Spec<InterfaceService, int>
{
    public WhenUsingConcreteInstanceForInterface()
        => When(_ => _.GetValue()).Using(() => new MyComponent(An<IMyLogger>(), An<int>()))
        .And<IMyLogger>(() => new MyInvalidLogger<ApplicationException>());

    [Fact]
    public void ThenUseTheConcreteInstance()
    {
        Then().Throws<ApplicationException>();
        Specification.Is(
            """
            Using new MyComponent(an IMyLogger, an int)
              and new MyInvalidLogger<ApplicationException>()
            When _.GetValue()
            Then throws ApplicationException
            """);
    }
}

public class WhenIndirectlyUsingConcreteInstanceForInterface : Spec<InterfaceService, int>
{
    public WhenIndirectlyUsingConcreteInstanceForInterface()
        => When(_ => _.GetValue())
        .Using(A<MyComponent>)
        .And<IMyLogger>(() => new MyInvalidLogger<ApplicationException>());

    [Fact]
    public void ThenUseTheConcreteInstance()
    {
        Then().Throws<ApplicationException>();
        Specification.Is(
            """
            Using a MyComponent
              and new MyInvalidLogger<ApplicationException>()
            When _.GetValue()
            Then throws ApplicationException
            """);
    }
}

public class WhenUsingConcreteInstanceForInterfaceWithAutoMockedConstructorArgument : Spec<InterfaceService, int>
{
    public WhenUsingConcreteInstanceForInterfaceWithAutoMockedConstructorArgument()
        => When(_ => _.GetValue()).Using(A<MyComponent>).And(An<int>);

    [Fact]
    public void ThenAutoMockComponent()
    {
        Result.Is(The<int>());
        Specification.Is(
            """
            Using a MyComponent
              and an int
            When _.GetValue()
            Then Result is the int
            """);
    }
}

public interface IMyService
{
    int GetValue();
    Task<int> GetValueAsync();
    void SetValue(int value);
    Task SetValueAsync(int value);
}

public class MyService(MyComponent component) : IMyService
{
    private readonly MyComponent _component = component;
    public int GetValue() => _component.GetValue();
    public Task<int> GetValueAsync() => Task.FromResult(_component.GetValue());

    public Task SetValueAsync(int value)
    {
        throw new NotImplementedException();
    }

    public void SetValue(int value)
    {
        throw new NotImplementedException();
    }
}

public class InterfaceService(MyComponent component, IMyService service)
{
    private readonly MyComponent _component = component;
    private readonly IMyService _service = service;
    public int GetValue() => _component.GetValue();
    public int GetServiceValue() => _service.GetValue();
    public Task<int> GetServiceValueAsync() => _service.GetValueAsync();
    public void SetValue(int value) => _service.SetValue(value);
    public Task SetValueAsync(int value) => _service.SetValueAsync(value);
}

public class MyComponent(IMyLogger logger, int value)
{
    private readonly IMyLogger _logger = logger;
    private int _value = value;

    public int GetValue()
    {
        _logger.LogValue(_value);
        return _value;
    }

    public void SetValue(int newValue) => _value = newValue;
}

public interface IMyLogger
{
    void LogValue(int value);
}

public class MyInvalidLogger<TException> : IMyLogger
    where TException : Exception, new()
{
    public void LogValue(int value) => throw new TException();
}