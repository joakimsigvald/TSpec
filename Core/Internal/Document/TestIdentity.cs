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
}
