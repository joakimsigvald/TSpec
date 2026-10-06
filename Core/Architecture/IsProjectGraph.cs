using System.Runtime.CompilerServices;
using TSpec.Assert.Continuations;
using TSpec.Internal.Specification;

namespace TSpec.Architecture;

/// <summary>
/// Continuation that allows assertions to be made on the dependencies between projects
/// </summary>
public record IsProjectGraph : Constraint<ProjectGraph, IsProjectGraph>
{
    private IReadOnlyList<string> _found = [];

    /// <summary>
    /// Asserts that each project depends only on what one of the given rules allows it. A rule is a
    /// function from a project to what it may reference, typically a switch expression; one that
    /// ignores the project, such as <c>_ => ["P:Microsoft.*"]</c>, allows its targets to all. A
    /// target ending in "*" allows every name that begins with what precedes it
    /// </summary>
    /// <param name="rules">The projects each project may reference</param>
    /// <returns>A continuation for making further assertions on the dependencies</returns>
    public ContinueWith<IsProjectGraph> Within(params Func<string, IEnumerable<string>>[] rules)
    {
        _found = [.. Actual!.ReferencesOutside(rules)];
        return Assert("what is allowed", _ => Xunit.Assert.Empty(_found)).And();
    }

    /// <summary>
    /// Asserts that some project depends on what it already reaches through another of its dependencies
    /// </summary>
    /// <returns>A continuation for making further assertions on the dependencies</returns>
    public ContinueWith<IsProjectGraph> Redundant()
    {
        _found = [.. Actual!.RedundantReferences()];
        return Assert(Ignore.Me, _ => Xunit.Assert.NotEmpty(_found)).And();
    }

    private protected override string Describe(ProjectGraph? value, string? methodName = null) => $"{_found.Count}: {_found.FormatValue()}";
}

/// <summary>
/// Fluent assertions on the dependencies between projects
/// </summary>
public static class AssertionExtensionsArchitecture
{
    /// <summary>
    /// Get available assertions for the dependencies between projects
    /// </summary>
    /// <param name="actual">The dependencies to assert on</param>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making assertions on the dependencies</returns>
    public static IsProjectGraph Is(
        this ProjectGraph? actual,
        Ignore _ = default,
        [CallerArgumentExpression(nameof(actual))] string? actualExpr = null)
        => IsProjectGraph.Create(actual, actualExpr!);
}
