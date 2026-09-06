using TSpec.Internal.Specification;

namespace TSpec.Internal.Document;

/// <summary>
/// Reads who the running test is, and whether it passed, from the xunit context.
/// </summary>
internal static class TestIdentity
{
    internal static bool Passed
        => TestContext.Current.TestState?.Result == TestResult.Passed;

    /// <summary>
    /// Skipped while it ran, rather than by its attribute — the only place that distinction is
    /// visible, since the attribute the assembly is read from carries no skip reason for it.
    /// </summary>
    internal static bool Skipped
        => TestContext.Current.TestState?.Result == TestResult.Skipped;

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
    /// <remarks>
    /// Narrowed to what the spec actually uses: a type argument states something only where the act
    /// uses it in that capacity. An act taking no subject leaves a generated value nothing reads,
    /// and one yielding no result has no return type whatever <c>TResult</c> was written as.
    /// <c>Spec&lt;T&gt;</c> needs no case of its own — being <c>Spec&lt;T, T&gt;</c>, it states T
    /// twice where both are used and once where one is.
    /// </remarks>
    internal static (string? SubjectUnderTest, string? ReturnType)? Declares(
        Type testClass, bool actsOnSubject, bool yieldsResult)
    {
        for (var type = testClass; type is not null; type = type.BaseType)
        {
            if (type == typeof(Spec))
                return null;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Spec<,>))
                return actsOnSubject || yieldsResult
                    ? (actsOnSubject ? type.GenericTypeArguments[0].Alias() : null,
                        yieldsResult ? type.GenericTypeArguments[1].Alias() : null)
                    : null;
        }
        return null;
    }
}
