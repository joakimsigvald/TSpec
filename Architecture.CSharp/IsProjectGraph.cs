using TSpec.Assert.Continuations;

namespace TSpec.Architecture.CSharp;

/// <summary>
/// Continuation that allows assertions to be made on the dependencies between projects
/// </summary>
public record IsProjectGraph : Constraint<SourceProjects, IsProjectGraph>
{
    private const string WhatIsAllowed = "what is allowed";

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
        AddTable(Actual.Headers, Actual.AllowedBy(rules));
        return Expect(WhatIsAllowed, _ => Xunit.Assert.Empty(_found), WhatIsAllowed);
    }

    /// <summary>
    /// Asserts that no project depends on what it already reaches through another of its dependencies
    /// </summary>
    /// <returns>A continuation for making further assertions on the dependencies</returns>
    public ContinueWith<IsProjectGraph> NonRedundant()
    {
        _found = [.. Actual!.RedundantReferences()];
        return Expect(Ignore.Me, _ => Xunit.Assert.Empty(_found));
    }

    /// <inheritdoc/>
    protected override string Found(SourceProjects? actual, string? methodName) => Counted(_found.Count, _found);
}
