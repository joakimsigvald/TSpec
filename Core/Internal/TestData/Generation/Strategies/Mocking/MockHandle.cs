using Moq;
using Moq.Protected;
using System.Linq.Expressions;
using System.Reflection;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

/// <summary>
/// TSpec's hold on one mock: the type it stands in for, the instance handed to the subject, and the
/// calls that instance received. What TSpec asks of a mock is asked here, so Moq stays behind it.
/// </summary>
/// <remarks>
/// A call is set up with one answer: a function from the call's arguments to what it answers with,
/// or a throw. Reading the call and answering it are the same function, so nothing above depends on
/// the order a mocking library would run a callback and a return in. The answer produces a value of
/// its own answer type; where the call is awaited and that is the value inside the task, the task is
/// made here.
/// </remarks>
internal sealed class MockHandle(Type mockedType, Mock moqMock)
{
    internal Type MockedType => mockedType;

    internal object Instance => moqMock.Object;

    internal IReadOnlyList<MockInvocation> Invocations
        => [.. moqMock.Invocations.Select(invocation => new MockInvocation(invocation.Method, invocation.Arguments))];

    /// The Moq mock itself, for what has not moved behind the handle yet: verification by expression.
    internal Mock MoqMock => moqMock;

    internal void Answer<TService>(Expression<Action<TService>> call, Func<IReadOnlyList<object>, object?> answer)
        where TService : class
        => Mocked<TService>().Setup(call)
            .Callback(new InvocationAction(invocation => answer(invocation.Arguments)));

    internal void Answer<TService, TResult>(
        Expression<Func<TService, TResult>> call, Type answerType, Func<IReadOnlyList<object>, object?> answer)
        where TService : class
        => Mocked<TService>().Setup(call)
            .Returns(Responding(typeof(TResult), answerType, answer));

    /// <summary>
    /// A member no expression can name — a protected method or property. A name states no
    /// arguments, so every parameter takes whatever it is passed.
    /// </summary>
    internal void Answer<TService>(MemberInfo member, Type answerType, Func<IReadOnlyList<object>, object?> answer)
        where TService : class
    {
        var (returnType, matchers) = member switch
        {
            PropertyInfo property => (property.PropertyType, Array.Empty<object>()),
            MethodInfo method => (method.ReturnType, method.GetParameters().Select(AnyOf).ToArray()),
            _ => throw new ArgumentException($"{member.Name} is neither a method nor a property", nameof(member))
        };
        var protectedMock = Mocked<TService>().Protected();
        if (returnType == typeof(void))
        {
            protectedMock.Setup(member.Name, exactParameterMatch: true, args: matchers)
                .Callback(new InvocationAction(invocation => answer(invocation.Arguments)));
            return;
        }
        var setup = Invoke(SetupOf<TService>(returnType), protectedMock, [member.Name, true, matchers]);
        Invoke(ReturnsOf<TService>(returnType), setup, [Responding(returnType, answerType, answer)]);
    }

    private Mock<TService> Mocked<TService>() where TService : class => (Mock<TService>)moqMock;

    private static InvocationFunc Responding(
        Type returnType, Type answerType, Func<IReadOnlyList<object>, object?> answer)
        => new(invocation => AsyncAnswer.Respond(returnType, answerType, () => answer(invocation.Arguments))!);

    /// <summary>
    /// Moq's protected Setup is generic in the return type, which is known only at run time here, so
    /// it is reached by reflection. What it throws arrives wrapped in a wrapper that says nothing, so
    /// the reason inside is what gets raised.
    /// </summary>
    private static MethodInfo SetupOf<TService>(Type returnType) where TService : class
        => typeof(IProtectedMock<TService>).GetMethods()
            .First(candidate => candidate.Name == "Setup"
                && candidate.IsGenericMethod
                && candidate.GetParameters().Length == 3)
            .MakeGenericMethod(returnType);

    private static MethodInfo ReturnsOf<TService>(Type returnType) where TService : class
        => typeof(Moq.Language.IReturns<,>).MakeGenericType(typeof(TService), returnType)
            .GetMethod("Returns", [typeof(InvocationFunc)])!;

    private static object Invoke(MethodInfo method, object target, object?[] arguments)
    {
        try
        {
            return method.Invoke(target, arguments)!;
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
}

internal sealed record MockInvocation(MethodInfo Method, IReadOnlyList<object?> Arguments);
