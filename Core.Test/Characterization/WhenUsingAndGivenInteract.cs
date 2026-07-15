using TSpec.Assert;

namespace TSpec.Test.Characterization;

public class WhenUsingConcreteForInterface_NoGiven : Spec<GreeterUser, string>
{
    public WhenUsingConcreteForInterface_NoGiven()
        => Using<IGreeter>(new StaticGreeter()).When(_ => _.Greet());

    [Fact]
    public void Then_ConcreteIsUsed()
    {
        Then().Result.Is("static-hello");
        Specification.Is(
            """
            Using new StaticGreeter()
            When _.Greet()
            Then Result is "static-hello"
            """);
    }
}

public class WhenGivenThatForInterface_NoUsing : Spec<GreeterUser, string>
{
    public WhenGivenThatForInterface_NoUsing()
        => Given<IGreeter>().That(_ => _.Hello()).Returns(() => "mocked-hello")
            .When(_ => _.Greet());

    [Fact]
    public void Then_MockIsUsedWithSetup()
    {
        Then().Result.Is("mocked-hello");
        Specification.Is(
            """
            Given IGreeter.Hello() returns "mocked-hello"
            When _.Greet()
            Then Result is "mocked-hello"
            """);
    }
}

public class WhenBothUsingConcreteAndGivenThat : Spec<GreeterUser, string>
{
    public WhenBothUsingConcreteAndGivenThat()
        => Using<IGreeter>(new StaticGreeter())
            .Given<IGreeter>().That(_ => _.Hello()).Returns(() => "mocked-hello")
            .When(_ => _.Greet());

    [Fact]
    public void Then_ConcreteFromUsingWins_MockSetupIsIgnored()
    {
        Then().Result.Is("static-hello");
        Specification.Is(
            """
            Using new StaticGreeter()
            Given IGreeter.Hello() returns "mocked-hello"
            When _.Greet()
            Then Result is "static-hello"
            """);
    }
}

public interface IGreeter
{
    string Hello();
    string Bye();
}

public class StaticGreeter : IGreeter
{
    public string Hello() => "static-hello";
    public string Bye() => "static-bye";
}

public class GreeterUser(IGreeter greeter)
{
    private readonly IGreeter _greeter = greeter;
    public string Greet() => _greeter.Hello();
    public string Farewell() => _greeter.Bye();
}
