using TSpec.Assert;

namespace TSpec.Test.AutoFixture.Generator;

public class WhenSubjectConstructorHasDefaultValues : Spec<ServiceWithDefaults>
{
    public WhenSubjectConstructorHasDefaultValues() => When(_ => _);

    [Fact]
    public void ThenARequiredDependencyIsStillMocked()
        => Then().Result.Repo.Is().not.Null();

    [Fact]
    public void ThenABoolDefaultIsHonoured()
        => Then().Result.LogUsage.Is().False();

    [Fact]
    public void ThenAnIntDefaultIsHonoured()
        => Then().Result.Retries.Is(3);

    [Fact]
    public void ThenAStringDefaultIsHonoured()
        => Then().Result.Name.Is("fallback");

    [Fact]
    public void ThenANullDefaultIsHonoured()
        => Then().Result.Connection.Is().Null();

    [Fact]
    public void ThenAnEnumDefaultIsHonoured()
        => Then().Result.Effort.Is(Effort.Low);

    [Fact]
    public void ThenAStructDefaultIsHonoured()
        => Then().Result.Stamp.Is(default(DateTime));

    [Fact]
    public void ThenAParamsArrayIsStillGenerated()
        => Then().Result.Rest.Is().not.Null();

    [Fact]
    public void ThenAnUnarrangedOptionalDependencyIsNull()
        => Then().Result.Extra.Is().Null();
}

public class WhenAnOptionalDependencyIsArranged : Spec<ServiceWithDefaults>
{
    public WhenAnOptionalDependencyIsArranged() => When(_ => _);

    [Fact]
    public void ThenTheMockIsInjected()
        => Given<IOptionalDep>().That(_ => _.Speak()).Returns(() => "hello")
            .Then().Result.Extra!.Speak().Is("hello");
}

public class WhenASetupCoversAParameterWithADefault : Spec<ServiceWithDefaults>
{
    public WhenASetupCoversAParameterWithADefault() => When(_ => _);

    [Fact]
    public void ThenAProvidedValueWins()
        => Using(9).Then().Result.Retries.Is(9);

    [Fact]
    public void ThenAValueSpaceWins()
        => Using<int>().From<int>().StartingAt(6).Then().Result.Retries.Is(6);

    [Fact]
    public void ThenAProvidedImplementationWins()
    {
        var dep = new SpeakingDep();
        Using<IOptionalDep>(dep).Then().Result.Extra.Is(dep);
    }
}

public class WhenAnInputModelHasConstructorDefaults : Spec<ServiceWithDefaults>
{
    [Fact]
    public void ThenTheDefaultsAreIgnored()
    {
        var model = A<ModelWithDefaults>();
        model.Name.Is().Not("fallback");
        model.Size.Is().Not(7);
    }
}

public enum Effort { Low, High }

public interface IProbeRepo
{
    int Get();
}

public interface IOptionalDep
{
    string Speak();
}

public class SpeakingDep : IOptionalDep
{
    public string Speak() => "concrete";
}

public class ServiceWithDefaults(
    IProbeRepo repo,
    IOptionalDep? extra = null,
    bool logUsage = false,
    int retries = 3,
    string name = "fallback",
    string? connection = null,
    Effort effort = Effort.Low,
    DateTime stamp = default,
    params int[] rest)
{
    public IProbeRepo Repo => repo;
    public IOptionalDep? Extra => extra;
    public bool LogUsage => logUsage;
    public int Retries => retries;
    public string Name => name;
    public string? Connection => connection;
    public Effort Effort => effort;
    public DateTime Stamp => stamp;
    public int[] Rest => rest;
}

public class ModelWithDefaults(string name = "fallback", int size = 7)
{
    public string Name => name;
    public int Size => size;
}

public class WhenTheSubjectIsARecordWithDefaults : Spec<RecordWithDefaults>
{
    public WhenTheSubjectIsARecordWithDefaults() => When(_ => _);

    [Fact]
    public void ThenABoolDefaultIsHonoured() => Then().Result.Verbose.Is().False();

    [Fact]
    public void ThenAnIntDefaultIsHonoured() => Then().Result.Size.Is(7);

    [Fact]
    public void ThenANullDefaultIsHonoured() => Then().Result.Label.Is().Null();
}

public record RecordWithDefaults(bool Verbose = false, int Size = 7, string? Label = null);

public class WhenAComponentOfTheSubjectHasDefaults : Spec<ServiceWithComponent>
{
    public WhenAComponentOfTheSubjectHasDefaults() => When(_ => _);

    [Fact]
    public void ThenTheComponentDefaultsAreHonoured()
    {
        Then().Result.Component.Retries.Is(3);
        Then().Result.Component.Label.Is().Null();
    }
}

public class ComponentWithDefaults(int retries = 3, string? label = null)
{
    public int Retries => retries;
    public string? Label => label;
}

public class ServiceWithComponent(ComponentWithDefaults component)
{
    public ComponentWithDefaults Component => component;
}
