using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Using;

public class WhenUsingConcreteInstanceOfInterface : Spec<MyService, int>
{
    public WhenUsingConcreteInstanceOfInterface() => When(_ => _.GetNextId());

    [Fact]
    public void WithoutCast_ThenUseIt()
    {
        Using(new FakeRepository(An<int>()), For.Subject)
            .Then().Result.Is(The<int>());
    }

    [Fact]
    public void WithDifferentCast_ThenDoNotUseIt()
    {
        Using<object>(new FakeRepository(An<int>()), For.Subject)
            .Then().Result.Is().Not(The<int>());
    }

    [Fact]
    public void WithCast_ThenUseIt()
    {
        Using<IMyRepository>(new FakeRepository(An<int>()), For.Subject)
            .Then().Result.Is(The<int>());
    }

    [Fact]
    public void GivenConcreteTypeArg_ThenUseIt()
    {
        Using<FakeRepository>().And(123, For.Subject).Then().Result.Is(123);
        Specification.Is(
            """
            Using FakeRepository
              and 123 for Subject
            When _.GetNextId()
            Then Result is 123
            """
            );
    }

    [Fact]
    public void GivenConcreteTypeArgWithScope_ThenMentionScope()
    {
        Using<FakeRepository>(For.Subject).And(123, For.Subject).Then().Result.Is(123);
        Specification.Is(
            """
            Using FakeRepository for Subject
              and 123 for Subject
            When _.GetNextId()
            Then Result is 123
            """
            );
    }
}

public class FakeRepository(int fakeId) : IMyRepository
{
    public int[] GetIds() => [];
    public MyModel GetModel() => new();
    public MyModel[] GetModels() => [];
    public Task<IEnumerable<MyModel>> GetModelsAsync() => Task.FromResult(Enumerable.Empty<MyModel>());
    public int GetNextId() => fakeId;
    public MyModel SetModel(MyModel model) => model;
}