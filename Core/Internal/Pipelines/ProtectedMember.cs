using Moq;
using Moq.Protected;
using System.Reflection;
using TSpec.Internal.Specification;

namespace TSpec.Internal.Pipelines;

/// <summary>
/// Sets up a member a test cannot write a lambda for, by naming it.
/// </summary>
/// <remarks>
/// A protected member is not accessible to the expression a setup is normally written as, so it is
/// named instead — which is how Moq reaches one too. Everything Moq-specific about that route lives
/// here: what comes back is an ordinary setup, so the outcome vocabulary above knows nothing about
/// how the call was named. A name states no arguments, so every parameter takes whatever it is
/// passed.
/// </remarks>
internal static class ProtectedMember
{
    private const BindingFlags Declared =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    /// <summary>
    /// The Moq setup for the named protected member. Refuses, rather than guesses, wherever naming
    /// alone cannot identify one member or Moq cannot intercept it — each refusal saying which of
    /// those it is, since "no such member" would be read as a misspelling.
    /// </summary>
    internal static object Setup<TService>(Mock<TService> mock, string member, Type[] returnTypes)
        where TService : class
    {
        var methods = Inheritance(typeof(TService))
            .SelectMany(level => level.GetMethods(Declared))
            .Where(candidate => candidate.Name == member)
            .ToArray();
        if (methods.Length == 0)
            return SetupProperty(mock, member, returnTypes);

        // Before selecting by return type, since a generic member's return type is its own type
        // parameter and would otherwise be reported as simply the wrong one.
        var named = methods.Where(candidate => !candidate.ContainsGenericParameters).ToArray();
        if (named.Length == 0)
            throw new SetupFailed(
                $"{typeof(TService).Alias()}.{member} is generic, and a name carries no type "
                + "argument. Setting up a generic protected member is not supported");

        var method = Sole(named, member, returnTypes, typeof(TService));
        Verify(method, member, typeof(TService));
        return Install(mock, method);
    }

    private static IEnumerable<Type> Inheritance(Type? type)
    {
        for (; type is not null && type != typeof(object); type = type.BaseType)
            yield return type;
    }

    /// <summary>
    /// A name identifies one member or it identifies none. Overloads are refused rather than all
    /// set up together: a name states no arguments, so nothing in the test could say which overload
    /// it meant, and answering for all of them is a guess the test never made.
    /// </summary>
    private static MethodInfo Sole(
        MethodInfo[] methods, string member, Type[] returnTypes, Type service)
    {
        var answering = methods.Where(m => returnTypes.Contains(m.ReturnType)).ToArray();
        if (answering.Length == 1)
            return answering[0];

        if (answering.Length == 0)
            throw new SetupFailed(
                $"{service.Alias()}.{member} does not return {Either(returnTypes)}. It returns "
                + $"{Either([.. methods.Select(m => m.ReturnType)])}. Name the type the member "
                + "answers with — for an async member the value inside the task — and name it "
                + "exactly: a base type or interface it happens to satisfy will not match");

        throw new SetupFailed(
            $"{service.Alias()}.{member} is overloaded ({answering.Length} of them return "
            + $"{Either(returnTypes)}), and a name states no arguments, so it cannot say which one "
            + "is meant. Setting up an overloaded protected member is not supported");
    }

    /// <summary>
    /// What a name cannot carry, and what Moq cannot reach. Each is a real member the test named
    /// correctly, so none of them may be reported as a missing one.
    /// </summary>
    private static void Verify(MethodInfo method, string member, Type service)
    {
        if (method.IsPublic)
            throw new SetupFailed(
                $"{service.Alias()}.{member} is public, so state the call as an expression: "
                + "That(_ => _.Method(...)). Naming a member is for one no expression can reach");
        if (method.GetParameters().FirstOrDefault(p => p.ParameterType.IsByRef) is { } byRef)
            throw new SetupFailed(
                $"{service.Alias()}.{member} takes '{byRef.Name}' by reference, which cannot be "
                + "matched by a setup that states no arguments. Setting up a member with a ref or "
                + "out parameter is not supported");
        if (!method.IsVirtual || method.IsFinal)
            throw new SetupFailed(
                $"{service.Alias()}.{member} is not virtual or abstract, so nothing can intercept "
                + "it. Only a member the mock can override may be set up");
    }

    private static object Install<TService>(Mock<TService> mock, MethodInfo method)
        where TService : class
    {
        var matchers = method.GetParameters().Select(AnyOf).ToArray();
        return method.ReturnType == typeof(void)
            ? mock.Protected().Setup(method.Name, exactParameterMatch: true, args: matchers)
            : Invoke(SetupOf<TService>(method.ReturnType), mock.Protected(), [method.Name, true, matchers]);
    }

    /// A protected property is named the same way; it takes no arguments, so it states no matchers.
    private static object SetupProperty<TService>(
        Mock<TService> mock, string member, Type[] returnTypes)
        where TService : class
    {
        var property = Inheritance(typeof(TService))
            .Select(level => level.GetProperty(member, Declared))
            .FirstOrDefault(found => found is not null) 
            ?? throw new SetupFailed(
                $"{typeof(TService).Alias()} has no member named '{member}'. "
                + "ThatProtected names a member of the mocked type; check the spelling, "
                + "and prefer nameof so the compiler checks it");
        if (!returnTypes.Contains(property.PropertyType))
            throw new SetupFailed(
                $"{typeof(TService).Alias()}.{member} is a {property.PropertyType.Alias()}, not "
                + $"{Either(returnTypes)}. Name the type exactly: a base type or interface it "
                + "happens to satisfy will not match");

        return Invoke(
            SetupOf<TService>(property.PropertyType), mock.Protected(), [member, true, Array.Empty<object>()]);
    }

    /// <summary>
    /// Moq's Setup is generic in the return type, which is known only at run time here, so it is
    /// reached by reflection. What it throws arrives wrapped in a wrapper that says nothing, so the
    /// reason inside is what gets raised.
    /// </summary>
    private static MethodInfo SetupOf<TService>(Type returnType) where TService : class
        => typeof(IProtectedMock<TService>).GetMethods()
            .First(candidate => candidate.Name == "Setup"
                && candidate.IsGenericMethod
                && candidate.GetParameters().Length == 3)
            .MakeGenericMethod(returnType);

    private static object Invoke(MethodInfo setup, object target, object?[] arguments)
    {
        try
        {
            return setup.Invoke(target, arguments)!;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            throw new SetupFailed(ex.InnerException.Message, ex.InnerException);
        }
    }

    private static object AnyOf(ParameterInfo parameter)
        => typeof(ItExpr).GetMethod(nameof(ItExpr.IsAny))!
            .MakeGenericMethod(parameter.ParameterType)
            .Invoke(null, null)!;

    private static string Either(IEnumerable<Type> types)
        => string.Join(" or ", types.Select(type => type.Alias()).Distinct());
}
