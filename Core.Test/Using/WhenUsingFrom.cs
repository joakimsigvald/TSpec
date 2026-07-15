using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Using;

public class WhenUsingFrom : Spec<MyService, int>
{
    public WhenUsingFrom() => When(_ => _.GetNextId());

    [Fact]
    public void GivenFrom_ThenConcreteClassIsNotUsed()
    {
        Using<MarkerRepository>().From<int>().And(123, For.Subject).Then().Result.Is().Not(42);
        Specification.Is(
            """
            Using MarkerRepository from int
              and 123 for Subject
            When _.GetNextId()
            Then Result is not 42
            """
            );
    }
}

public class MarkerRepository : IMyRepository
{
    public int[] GetIds() => [];
    public MyModel GetModel() => new();
    public MyModel[] GetModels() => [];
    public Task<IEnumerable<MyModel>> GetModelsAsync() => Task.FromResult(Enumerable.Empty<MyModel>());
    public int GetNextId() => 42;
    public MyModel SetModel(MyModel model) => model;
}

public class WhenUsingConcreteClassForInterface : Spec<MyService, int>
{
    public WhenUsingConcreteClassForInterface() => When(_ => _.GetNextId());

    [Fact]
    public void ThenSubjectGetsConcreteInstanceInsteadOfMock()
        => Using<MarkerRepository>().Then().Result.Is(42);
}

public class WhenUsingFromForInput : Spec<int>
{
    public WhenUsingFromForInput() => Using<int>(For.Input).From((byte b) => b + 100);

    [Fact]
    public void ThenConversionAppliesToInputData()
        => Three<int>().Is().EqualTo([101, 102, 103]);
}

public class WhenUsingFromForSubject : Spec<int>
{
    public WhenUsingFromForSubject() => Using<int>(For.Subject).From((byte b) => b + 100);

    [Fact]
    public void ThenConversionDoesNotApplyToInputData()
        => Three<int>().Is().EqualTo([1, 2, 3]);
}

public class WhenUsingFromForSubjectOnMockedDependency : Spec<MyService, int>
{
    public WhenUsingFromForSubjectOnMockedDependency() => When(_ => _.GetNextId());

    [Fact]
    public void ThenConversionAppliesToSubjectDataButNotInputData()
    {
        Using<int>(For.Subject).From((byte b) => b + 100).Then().Result.Is().GreaterThan(100);
        A<int>().Is().LessThan(100);
    }
}

public class WhenUsingFromWithScope : Spec<MyService, int>
{
    public WhenUsingFromWithScope() => When(_ => _.GetNextId());

    [Fact]
    public void ThenSpecificationMentionsScope()
    {
        Using<MarkerRepository>(For.Input).From<int>().And(123, For.Subject).Then().Result.Is().Not(42);
        Specification.Is(
            """
            Using MarkerRepository from int for Input
              and 123 for Subject
            When _.GetNextId()
            Then Result is not 42
            """
            );
    }
}

public class WhenChainUsingFrom : Spec<int>
{
    public WhenChainUsingFrom() => Using<int>().From<byte>().And<long>().From<short>();

    [Fact]
    public void ThenBothConversionsApply()
    {
        Three<int>().Is().EqualTo([1, 2, 3]);
        Three<long>().Is().EqualTo([4, 5, 6]);
    }
}

public class WhenFromTwice : Spec<MyService, int>
{
    [Fact]
    public void ThenThrowsSetupFailed()
    {
        var continuation = Using<FakeRepository>();
        continuation.From<int>();
        Xunit.Assert.Throws<SetupFailed>(() => continuation.From<string>());
    }
}

public class WhenFromAfterSetup : Spec<MyService, int>
{
    public WhenFromAfterSetup() => When(_ => _.GetNextId());

    [Fact]
    public void ThenThrowsSetupFailed()
    {
        var continuation = Using<FakeRepository>();
        continuation.Then();
        Xunit.Assert.Throws<SetupFailed>(() => continuation.From<int>());
    }
}
