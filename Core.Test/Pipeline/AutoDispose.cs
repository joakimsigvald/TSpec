using Moq;

namespace TSpec.Test.Pipeline;

public class AutoDispose
{
    private sealed class DisposableSutSpec : Spec<DisposableSubject, int> { }
    private sealed class AsyncDisposableSutSpec : Spec<AsyncDisposableSubject, int> { }
    private sealed class OrderedSutSpec : Spec<OrderedSubject, int> { }
    private sealed class MockedServiceSutSpec : Spec<SubjectWithMockedService, int> { }
    private sealed class NeverRunSpec : Spec<CountingDisposable, int> { }

    private sealed class InputDataSpec : Spec<DisposableSubject, DisposableModel>
    {
        public InputDataSpec() => When(_ => A<DisposableModel>());
    }

    [Fact]
    public void GivenSutCreatedByTSpec_ThenDisposeItAfterTearDown()
    {
        var spec = new DisposableSutSpec();
        var sut = spec.When(_ => _.GetValue()).Then().SubjectUnderTest;
        Xunit.Assert.False(sut.IsDisposed);
        spec.Dispose();
        Xunit.Assert.Equal(1, sut.DisposeCount);
    }

    [Fact]
    public void GivenSutProvidedAsValue_ThenDoNotDisposeIt()
    {
        var mySut = new DisposableSubject();
        var spec = new DisposableSutSpec();
        var sut = spec.Using(mySut).When(_ => _.GetValue()).Then().SubjectUnderTest;
        Xunit.Assert.Same(mySut, sut);
        spec.Dispose();
        Xunit.Assert.False(mySut.IsDisposed);
    }

    [Fact]
    public void GivenSutProvidedAsFactory_ThenDoNotDisposeIt()
    {
        var mySut = new DisposableSubject();
        var spec = new DisposableSutSpec();
        var sut = spec.Using(() => mySut).When(_ => _.GetValue()).Then().SubjectUnderTest;
        Xunit.Assert.Same(mySut, sut);
        spec.Dispose();
        Xunit.Assert.False(mySut.IsDisposed);
    }

    [Fact]
    public void GivenAsyncDisposableSut_ThenDisposeItAsynchronously()
    {
        var spec = new AsyncDisposableSutSpec();
        var sut = spec.When(_ => _.GetValue()).Then().SubjectUnderTest;
        Xunit.Assert.False(sut.IsDisposed);
        spec.Dispose();
        Xunit.Assert.True(sut.IsDisposed);
    }

    [Fact]
    public void GivenDisposableDependency_ThenDisposeSutBeforeDependency_AfterUntil()
    {
        var log = new DisposalLog();
        var spec = new OrderedSutSpec();
        spec.Using(log, For.Subject)
            .When(_ => _.GetValue())
            .Until(_ => log.Entries.Add("until"))
            .Then();
        spec.Dispose();
        Xunit.Assert.Equal(["until", "subject", "dependency"], log.Entries);
    }

    [Fact]
    public void GivenUntilThrows_ThenStillDisposeSut()
    {
        var spec = new DisposableSutSpec();
        var sut = spec.When(_ => _.GetValue())
            .Until(_ => throw new InvalidOperationException())
            .Then().SubjectUnderTest;
        Xunit.Assert.Throws<InvalidOperationException>(spec.Dispose);
        Xunit.Assert.Equal(1, sut.DisposeCount);
    }

    [Fact]
    public void GivenSutAlreadyDisposedByUntil_ThenDisposeAgainWithoutError()
    {
        var spec = new DisposableSutSpec();
        var sut = spec.When(_ => _.GetValue())
            .Until(_ => _.Dispose())
            .Then().SubjectUnderTest;
        spec.Dispose();
        Xunit.Assert.Equal(2, sut.DisposeCount);
    }

    [Fact]
    public void GivenMockedDisposableService_ThenDoNotDisposeMock()
    {
        var spec = new MockedServiceSutSpec();
        var sut = spec.When(_ => _.GetValue()).Then().SubjectUnderTest;
        spec.Dispose();
        Mock.Get(sut.Service).Verify(_ => _.Dispose(), Times.Never());
    }

    [Fact]
    public void GivenGeneratedInputData_ThenDoNotDisposeIt()
    {
        var spec = new InputDataSpec();
        var model = spec.Then().Result;
        spec.Dispose();
        Xunit.Assert.False(model!.IsDisposed);
    }

    [Fact]
    public void GivenPipelineWasNeverRun_ThenTearDownDoesNotCreateSut()
    {
        var createdBefore = CountingDisposable.Created;
        var spec = new NeverRunSpec();
        spec.When(_ => _.GetValue());
        spec.Dispose();
        Xunit.Assert.Equal(createdBefore, CountingDisposable.Created);
    }
}

public sealed class DisposableSubject : IDisposable
{
    public int DisposeCount { get; private set; }
    public bool IsDisposed => DisposeCount > 0;
    public int GetValue() => DisposeCount + 1;
    public void Dispose() => DisposeCount++;
}

public sealed class AsyncDisposableSubject : IAsyncDisposable
{
    public bool IsDisposed { get; private set; }
    public int GetValue() => IsDisposed ? 0 : 1;

    public ValueTask DisposeAsync()
    {
        IsDisposed = true;
        return ValueTask.CompletedTask;
    }
}

public sealed class DisposableModel : IDisposable
{
    public bool IsDisposed { get; private set; }
    public void Dispose() => IsDisposed = true;
}

public sealed class DisposalLog
{
    public List<string> Entries { get; } = [];
}

public sealed class OrderedDependency(DisposalLog log) : IDisposable
{
    public void Dispose() => log.Entries.Add("dependency");
}

public sealed class OrderedSubject(OrderedDependency dependency, DisposalLog log) : IDisposable
{
    private readonly OrderedDependency _dependency = dependency;
    public int GetValue() => _dependency is null ? 0 : 1;
    public void Dispose() => log.Entries.Add("subject");
}

public interface IDisposableService : IDisposable
{
    int Get();
}

public sealed class SubjectWithMockedService(IDisposableService service)
{
    public IDisposableService Service => service;
    public int GetValue() => service.Get();
}

public sealed class CountingDisposable : IDisposable
{
    public static int Created { get; private set; }
    public int Index { get; } = ++Created;
    public int GetValue() => Index;
    public void Dispose() { }
}