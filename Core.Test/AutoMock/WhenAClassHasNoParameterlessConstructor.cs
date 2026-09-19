using TSpec.Assert;

namespace TSpec.Test.AutoMock;

public abstract class Greeter(string greeting)
{
    public string Greeting { get; } = greeting;
    public abstract string Name();
}

public abstract class Retrier
{
    protected Retrier(int retries = 3) => Retries = retries;
    public int Retries { get; }
    public abstract bool Try();
}

public abstract class ThreeLetterCode
{
    protected ThreeLetterCode(string code)
    {
        if (code.Length != 3)
            throw new ArgumentException("A code has three letters", nameof(code));
    }

    public abstract string Describe();
}

public class Endpoint(Uri address)
{
    public Uri Address { get; } = address;
    public virtual string Fetch() => "real";
}

public class GreeterService(Greeter greeter)
{
    public string Name() => greeter.Name();
    public string Greeting() => greeter.Greeting;
}

public class RetrierService(Retrier retrier)
{
    public int Retries() => retrier.Retries;
}

public class CodeService(ThreeLetterCode code)
{
    public string Describe() => code.Describe();
}

public class EndpointService(Endpoint endpoint)
{
    public string Fetch() => endpoint.Fetch();
}

/// A class with no parameterless constructor is mocked through its greediest, the arguments built as the subject's are.
public class WhenAClassHasNoParameterlessConstructor : Spec<GreeterService, string>
{
    [Fact]
    public void GivenACallIsSetUp_ThenTheMockAnswers()
        => When(_ => _.Name())
            .Given<Greeter>().That(_ => _.Name()).Returns(() => "Ada")
            .Then().Result.Is("Ada");

    [Fact]
    public void ThenItsConstructorGetsGeneratedArguments()
        => When(_ => _.Greeting()).Then().Result.Is().not.Empty();
}

/// A protected constructor serves the mock as well, which is a subclass; a parameter keeps its default, as the subject's does.
public class WhenAProtectedConstructorHasADefault : Spec<RetrierService, int>
{
    [Fact]
    public void ThenTheDefaultStands()
        => When(_ => _.Retries()).Then().Result.Is(3);
}

public class WhenAConstructorRejectsTheGeneratedArguments : Spec<CodeService, string>
{
    [Fact]
    public void ThenMockingItIsRefusedSayingWhatToDo()
        => Xunit.Assert.Throws<SetupFailed>(() => When(_ => _.Describe()).Then())
            .Message.Is("Provide ThreeLetterCode with Using instead of a mock: its constructor threw "
                + "ArgumentException for the arguments TSpec generated, because: A code has three letters (Parameter 'code')");
}

public class WhenASetUpClassHasNoParameterlessConstructor : Spec<EndpointService, string>
{
    [Fact]
    public void GivenACallIsSetUp_ThenTheMockAnswers()
        => When(_ => _.Fetch())
            .Given<Endpoint>().That(_ => _.Fetch()).Returns(() => "mocked")
            .Then().Result.Is("mocked");
}
