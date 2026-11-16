using System.Runtime.CompilerServices;
using TSpec.Assert.Continuations;
using TSpec.Extensibility;

namespace TSpec.Architecture.CSharp;

/// <summary>
/// Continuation that allows assertions to be made on the dependencies between the nodes of a graph
/// </summary>
public record IsDependencyGraph : Constraint<DependencyGraph, IsDependencyGraph>
{
    private const string WhatIsAllowed = "what is allowed";

    private IReadOnlyList<string> _found = [];

    /// <summary>
    /// Asserts that each node depends only on what one of the given rules allows it. A rule is a
    /// function from a node to what it may depend on, typically a switch expression; one that ignores
    /// the node, such as <c>_ => ["Shared"]</c>, allows its targets to all. A target ending in "*"
    /// allows every name that begins with what precedes it
    /// </summary>
    /// <param name="rules">The nodes each node may depend on</param>
    /// <returns>A continuation for making further assertions on the dependencies</returns>
    public ContinueWith<IsDependencyGraph> Within(params Func<string, IEnumerable<string>>[] rules)
    {
        _found = [.. Actual!.DependenciesOutside(rules)];
        AddTable(Actual.Headers, Actual.AllowedBy(rules), Actual.ArrowsAllowedBy(rules));
        return Expect(WhatIsAllowed, _ => Xunit.Assert.Empty(_found), WhatIsAllowed);
    }

    /// <summary>
    /// Asserts that no nodes depend on each other around a loop, directly or through others; a failure
    /// names the first loop
    /// </summary>
    /// <returns>A continuation for making further assertions on the dependencies</returns>
    public ContinueWith<IsDependencyGraph> Acyclic()
    {
        _found = [.. Actual!.FirstLoop()];
        return Expect(Ignore.Me, _ => Xunit.Assert.Empty(_found));
    }

    /// <inheritdoc/>
    protected override string Found(DependencyGraph? actual, string? methodName) => Counted(_found.Count, _found);
}

/// <summary>
/// Fluent assertions on the dependencies between the nodes of a graph
/// </summary>
public static class DependencyGraphAssertions
{
    /// <summary>
    /// Get available assertions for the dependencies between the nodes of a graph
    /// </summary>
    /// <param name="actual">The dependencies to assert on</param>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making assertions on the dependencies</returns>
    public static IsDependencyGraph Is(
        this DependencyGraph? actual,
        Ignore _ = default,
        [CallerArgumentExpression(nameof(actual))] string? actualExpr = null)
        => Constraints.Create<DependencyGraph, IsDependencyGraph>(actual, actualExpr!);
}
