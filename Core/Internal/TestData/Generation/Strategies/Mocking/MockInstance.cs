using Castle.DynamicProxy;
using System.Reflection;
using TSpec.Internal.Specification;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

/// <summary>
/// The instance a mock hands out: a Castle proxy of a class, or of object implementing an interface, or
/// a delegate compiled to forward its calls. Every call it receives goes to the receiver it is made with.
/// </summary>
internal static class MockInstance
{
    private static readonly ProxyGenerator _generator = new();
    private static readonly ProxyGenerationOptions _options = new(new MockHook());

    internal static object Create(
        Type type, string name, Func<MethodInfo, object?[], object?> receive, Func<object?[]> constructorArguments)
    {
        if (typeof(Delegate).IsAssignableFrom(type))
            return DelegateForwarder.Create(type, receive);

        if (type.IsInterface)
            return _generator.CreateClassProxy(typeof(object), [type], _options, new Interceptor(name, receive));

        if (type.IsSealed)
            throw new SetupFailed($"{type.Alias()} is sealed, so it cannot be mocked. Provide one with Using instead");

        return CreateClassProxy(type, new Interceptor(name, receive), constructorArguments());
    }

    private static object CreateClassProxy(Type type, IInterceptor interceptor, object?[] arguments)
    {
        if (arguments.Length == 0)
            return _generator.CreateClassProxy(type, _options, interceptor);

        try
        {
            return _generator.CreateClassProxy(type, _options, arguments!, interceptor);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is { } rejection)
        {
            throw new SetupFailed(
                $"Provide {type.Alias()} with Using instead of a mock: its constructor threw "
                + $"{rejection.GetType().Name} for the arguments TSpec generated", rejection);
        }
    }

    private sealed class Interceptor(string name, Func<MethodInfo, object?[], object?> receive) : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method.DeclaringType == typeof(object))
            {
                invocation.ReturnValue = name;
                return;
            }
            invocation.ReturnValue = receive(invocation.GetConcreteMethod(), invocation.Arguments);
        }
    }

    /// <summary>
    /// Every member the mocked type lets a proxy override is intercepted. Of what every object has,
    /// only ToString is, so a mock names itself as the specification names it; equality stays the
    /// instance's own.
    /// </summary>
    private sealed class MockHook : IProxyGenerationHook
    {
        public void MethodsInspected() { }

        public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo) { }

        public bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
            => methodInfo.DeclaringType != typeof(object) || methodInfo.Name == nameof(ToString);

        public override bool Equals(object? obj) => obj is MockHook;

        public override int GetHashCode() => typeof(MockHook).GetHashCode();
    }
}
