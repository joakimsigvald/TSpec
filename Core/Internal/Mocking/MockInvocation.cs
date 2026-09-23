using System.Reflection;
using TSpec.Internal.Pipelines;

namespace TSpec.Internal.Mocking;

internal sealed record MockInvocation(MethodInfo Method, IReadOnlyList<object?> Arguments, Phase Phase)
{
    internal object? Answer { get; set; }

    /// Calls made while arranging are logged, but not counted.
    internal bool IsCounted => Phase == Phase.Act;
}
