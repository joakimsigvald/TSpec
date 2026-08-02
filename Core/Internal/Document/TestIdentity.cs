using TSpec.Internal.Specification;
using Xunit.Sdk;

namespace TSpec.Internal.Document;

/// <summary>
/// Reads who the running test is, and whether it passed, from the xunit context.
/// </summary>
internal static class TestIdentity
{
    internal static bool Passed
        => TestContext.Current.TestState?.Result == TestResult.Passed;

    internal static string Requirement
        => TestContext.Current.TestMethod?.MethodName ?? "(unknown)";

    /// <summary>
    /// Splits the test class into the method under test and the branch of given-classes below it,
    /// following the nesting rather than the inheritance chain: a shared base such as
    /// <c>ApiSpec&lt;T&gt;</c> is scaffolding, not part of the specification's structure.
    /// </summary>
    internal static (string Subject, string Branch) Locate(Type testClass)
    {
        var nesting = new List<string>();
        for (var type = testClass; type is not null; type = type.DeclaringType)
            nesting.Add(type.Name);
        nesting.Reverse();
        return (nesting[0], string.Join(".", nesting.Skip(1)));
    }

    /// <summary>
    /// The subject-under-test and return type the class declares, or null when it declares neither.
    /// </summary>
    /// <remarks>
    /// This walks the inheritance chain, unlike <see cref="Locate"/>, because the types are what a
    /// base class states rather than what the nesting expresses. The non-generic <c>Spec</c> is
    /// <c>Spec&lt;object, object&gt;</c>, so it has to be recognised before the closed generic is
    /// reached — otherwise a spec that declares no subject would claim one of type object.
    /// </remarks>
    internal static (string SubjectUnderTest, string? ReturnType)? Declares(Type testClass)
    {
        for (var type = testClass; type is not null; type = type.BaseType)
        {
            if (type == typeof(Spec))
                return null;
            // Spec<T> is Spec<T, T>, so a return type read from it would only repeat the subject's
            // name — and it is also how a spec whose result is not asserted is spelled, where naming
            // one would claim something the spec never says. Checked before Spec<,>, which it
            // derives from, for the same reason the non-generic Spec is checked before both.
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Spec<>))
                return (type.GenericTypeArguments[0].Alias(), null);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Spec<,>))
                return (type.GenericTypeArguments[0].Alias(), type.GenericTypeArguments[1].Alias());
        }
        return null;
    }
}
