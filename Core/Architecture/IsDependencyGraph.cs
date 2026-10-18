using TSpec.Assert.Continuations;
using TSpec.Internal.Specification;

namespace TSpec.Architecture;

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
        SpecificationContext.Current.AddTable(Actual.AllowedBy(rules));
        return Assert(WhatIsAllowed, _ => Xunit.Assert.Empty(_found), WhatIsAllowed).And();
    }

    private protected override string Found(DependencyGraph? actual, string? methodName) => Counted(_found.Count, _found);
}
