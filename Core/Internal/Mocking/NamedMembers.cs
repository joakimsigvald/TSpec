using System.Reflection;
using TSpec.Internal.Specification;

namespace TSpec.Internal.Mocking;

/// <summary>
/// What a name reaches on a mocked type, found as a setup by name is made. A name that reaches no
/// member it can answer is refused, saying why, since it would otherwise answer no call at all.
/// </summary>
internal static class NamedMembers
{
    private const BindingFlags AnyInstanceMember = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    internal static NameMatcher Matcher(Type service, string name, Type answerType)
    {
        _ = Reached(service, name, answerType);
        return new(name, answerType);
    }

    /// A read of the arguments must know what they are, which a name of several members does not say.
    internal static void AssertOne(Type service, string name, Type answerType)
    {
        if (Reached(service, name, answerType).Length == 1)
            return;

        throw new SetupFailed(
            $"{service.Alias()}.{name} names several members, so a Tap or Returns cannot read their arguments. "
            + $"Set up the one you mean with an expression: That(_ => _.{name}(…))");
    }

    private static MethodInfo[] Reached(Type service, string name, Type answerType)
    {
        var named = Named(service, name);
        if (named.Length == 0)
            throw new SetupFailed(
                $"{service.Alias()} has no method or property named '{name}'. "
                + "Check the spelling, and prefer nameof so the compiler checks it");

        var answering = named.Where(method => NameMatcher.CanAnswer(method.ReturnType, answerType)).ToArray();
        if (answering.Length == 0)
            throw new SetupFailed(
                $"{service.Alias()}.{name} does not return {Expected(answerType)}. It returns {Either(named)}. "
                + "Name the type the member answers with — for an async member, the value inside the task");

        var intercepted = answering.Where(IsVirtual).ToArray();
        if (intercepted.Length == 0)
            throw new SetupFailed(
                $"{service.Alias()}.{name} is not virtual or abstract, so nothing can intercept it. "
                + "Only a member the mock can override may be set up");

        return intercepted;
    }

    private static MethodInfo[] Named(Type service, string name)
        => [.. TypesOf(service)
            .SelectMany(type => type.GetMethods(AnyInstanceMember))
            .Where(method => NameMatcher.NameOf(method) == name)];

    /// An interface's members include those it inherits, which reflection lists only on the interfaces themselves.
    private static Type[] TypesOf(Type service)
        => service.IsInterface ? [service, .. service.GetInterfaces()] : [service];

    private static bool IsVirtual(MethodInfo method) => method.IsVirtual && !method.IsFinal;

    private static string Expected(Type answerType)
        => answerType == typeof(Continuations.Void) ? "void or Task or ValueTask" : answerType.Alias();

    private static string Either(MethodInfo[] methods)
        => string.Join(" or ", methods.Select(method => method.ReturnType.Alias()).Distinct());
}
