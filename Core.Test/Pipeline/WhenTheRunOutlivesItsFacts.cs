using TSpec.Assert;

namespace TSpec.Test.Pipeline;

/// The subject and an owned dependency log their disposal, which must wait for the class's last Fact.
[Collection(nameof(RunEndsWithItsClass))]
public class WhenTheRunOutlivesItsFacts : Spec<OrderedSubject, int>
{
    internal static DisposalLog Log { get; } = new();

    public WhenTheRunOutlivesItsFacts()
        => Using(Log, For.Subject)
            .And(() => new OrderedDependency(Log), owned: true)
            .When(_ => _.GetValue());

    [Fact] public void ThenOneFindsItAlive() => ThenNothingIsDisposed();

    [Fact] public void ThenAnotherFindsItAliveToo() => ThenNothingIsDisposed();

    private void ThenNothingIsDisposed()
    {
        Result.Is(1);
        Log.Entries.Is().Empty();
    }
}

/// A collection's fixture is disposed after the fixtures of its classes, so it sees how the run ended.
[CollectionDefinition(nameof(RunEndsWithItsClass))]
public class RunEndsWithItsClass : ICollectionFixture<RunEndsWithItsClass.Check>
{
    public sealed class Check : IDisposable
    {
        public void Dispose() => WhenTheRunOutlivesItsFacts.Log.Entries.Is().EqualTo(["subject", "dependency"]);
    }
}
