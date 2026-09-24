using System.Reflection;

namespace TSpec.Internal.Mocking;

/// Which calls a setup answers: those one call states, or every call to a member of a name.
internal interface ICallMatcher
{
    bool Matches(MethodInfo method, IReadOnlyList<object?> arguments);

    void WriteOutArguments(object?[] arguments);
}
