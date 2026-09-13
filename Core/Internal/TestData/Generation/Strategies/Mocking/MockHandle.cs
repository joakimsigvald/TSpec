using Moq;
using System.Reflection;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

/// <summary>
/// TSpec's hold on one mock: the type it stands in for, the instance handed to the subject, and the
/// calls that instance received. What TSpec asks of a mock is asked here, so Moq stays behind it.
/// </summary>
internal sealed class MockHandle(Type mockedType, Mock moqMock)
{
    internal Type MockedType => mockedType;

    internal object Instance => moqMock.Object;

    internal IReadOnlyList<MockInvocation> Invocations
        => [.. moqMock.Invocations.Select(invocation => new MockInvocation(invocation.Method, invocation.Arguments))];

    /// The Moq mock itself, for what has not moved behind the handle yet: call setup and verification by expression.
    internal Mock MoqMock => moqMock;
}

internal sealed record MockInvocation(MethodInfo Method, IReadOnlyList<object?> Arguments);
